using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiriMovies.Server.Data;
using GiriMovies.Server.Services;
using GiriMovies.Shared.DTOs;
using GiriMovies.Shared.Models;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace GiriMovies.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CertificatesController : ControllerBase
{
    private readonly GiriMoviesDbContext _context;
    private readonly ICertificateDeviceService _certificateService;
    private readonly ILogger<CertificatesController> _logger;
    
    public CertificatesController(
        GiriMoviesDbContext context,
        ICertificateDeviceService certificateService,
        ILogger<CertificatesController> logger)
    {
        _context = context;
        _certificateService = certificateService;
        _logger = logger;
    }
    
    // POST: api/certificates/enroll
    [HttpPost("enroll")]
    public async Task<ActionResult<CertificateEnrollmentResponse>> EnrollDevice(CertificateEnrollmentRequest request)
    {
        try
        {
            // Validate user credentials first
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return BadRequest(new CertificateEnrollmentResponse
                {
                    Success = false,
                    Message = "Invalid user credentials"
                });
            }
            
            // Generate certificate for device
            var certificate = GenerateClientCertificate(request);
            
            // Register device with certificate
            var registrationResult = await _certificateService.RegisterDeviceWithCertificateAsync(certificate, user.Id);
            
            if (!registrationResult.Success)
            {
                return BadRequest(new CertificateEnrollmentResponse
                {
                    Success = false,
                    Message = registrationResult.Message
                });
            }
            
            // Create user session with certificate info
            var userSession = new UserSession
            {
                UserId = user.Id,
                DeviceId = registrationResult.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                ComputerName = request.ComputerName,
                LocalIP = request.LocalIP,
                CertificateThumbprint = certificate.Thumbprint,
                CertificateSubject = certificate.Subject,
                CertificateValidFrom = certificate.NotBefore,
                CertificateValidTo = certificate.NotAfter,
                LoginTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true,
                SessionToken = Guid.NewGuid().ToString()
            };
            
            _context.UserSessions.Add(userSession);
            await _context.SaveChangesAsync();
            
            return Ok(new CertificateEnrollmentResponse
            {
                Success = true,
                Message = "Device enrolled successfully",
                DeviceId = registrationResult.DeviceId,
                CertificateThumbprint = certificate.Thumbprint,
                CertificatePem = ExportCertificateToPem(certificate),
                PrivateKeyPem = ExportPrivateKeyToPem(certificate),
                CertificateValidFrom = certificate.NotBefore,
                CertificateValidTo = certificate.NotAfter
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling device");
            return StatusCode(500, new CertificateEnrollmentResponse
            {
                Success = false,
                Message = "Internal server error during enrollment"
            });
        }
    }
    
    // POST: api/certificates/authenticate
    [HttpPost("authenticate")]
    public async Task<ActionResult<CertificateAuthenticationResponse>> AuthenticateWithCertificate()
    {
        try
        {
            if (!_certificateService.HasValidCertificate(HttpContext))
            {
                return Unauthorized(new CertificateAuthenticationResponse
                {
                    Success = false,
                    Message = "No valid client certificate provided"
                });
            }
            
            var deviceId = _certificateService.GetDeviceIdFromContext(HttpContext);
            var deviceInfo = _certificateService.ExtractDeviceInfoFromContext(HttpContext);
            
            // Find user session for this certificate
            var userSession = await _context.UserSessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId && s.IsActive);
            
            if (userSession == null)
            {
                return Unauthorized(new CertificateAuthenticationResponse
                {
                    Success = false,
                    Message = "Device not registered or session inactive"
                });
            }
            
            // Update session activity
            userSession.LastActivity = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return Ok(new CertificateAuthenticationResponse
            {
                Success = true,
                Message = "Authentication successful",
                UserId = userSession.UserId,
                DeviceId = deviceId,
                DeviceName = deviceInfo.DeviceName,
                SessionToken = userSession.SessionToken,
                User = new UserDto
                {
                    Id = userSession.User.Id,
                    Email = userSession.User.Email,
                    Name = userSession.User.Name
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during certificate authentication");
            return StatusCode(500, new CertificateAuthenticationResponse
            {
                Success = false,
                Message = "Authentication error"
            });
        }
    }
    
    // GET: api/certificates/devices
    [HttpGet("devices")]
    public async Task<ActionResult<IEnumerable<DeviceCertificateDto>>> GetUserDevices([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email parameter required");
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound("User not found");
        
        var devices = await _context.UserSessions
            .Where(s => s.UserId == user.Id && !string.IsNullOrEmpty(s.CertificateThumbprint))
            .OrderByDescending(s => s.LoginTime)
            .ToListAsync();
        
        var deviceDtos = devices.Select(d => new DeviceCertificateDto
        {
            DeviceId = d.DeviceId ?? "",
            DeviceName = d.DeviceName ?? "",
            DeviceType = d.DeviceType,
            CertificateThumbprint = d.CertificateThumbprint ?? "",
            CertificateSubject = d.CertificateSubject ?? "",
            CertificateValidFrom = d.CertificateValidFrom ?? DateTime.MinValue,
            CertificateValidTo = d.CertificateValidTo ?? DateTime.MinValue,
            IsRevoked = !d.IsActive,
            RegisteredAt = d.LoginTime,
            LastUsed = d.LastActivity
        });
        
        return Ok(deviceDtos);
    }
    
    // POST: api/certificates/{deviceId}/revoke
    [HttpPost("{deviceId}/revoke")]
    public async Task<IActionResult> RevokeCertificate(string deviceId)
    {
        try
        {
            var userSession = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.DeviceId == deviceId);
            
            if (userSession == null)
                return NotFound("Device not found");
            
            // Mark session as inactive
            userSession.IsActive = false;
            userSession.LogoutTime = DateTime.UtcNow;
            
            // Revoke certificate
            var revoked = await _certificateService.RevokeCertificateAsync(deviceId);
            
            if (revoked)
            {
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Certificate revoked successfully" });
            }
            
            return BadRequest(new { Message = "Failed to revoke certificate" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking certificate for device {DeviceId}", deviceId);
            return StatusCode(500, new { Message = "Internal server error" });
        }
    }
    
    private X509Certificate2 GenerateClientCertificate(CertificateEnrollmentRequest request)
    {
        // In a production environment, you would use a proper CA to generate certificates
        // This is a simplified version for demonstration
        
        using var rsa = System.Security.Cryptography.RSA.Create(2048);
        
        var req = new System.Security.Cryptography.X509Certificates.CertificateRequest(
            $"CN={request.DeviceName},OU={request.DeviceType},O=GiriMovies Blazor,C=US",
            rsa,
            System.Security.Cryptography.HashAlgorithmName.SHA256,
            System.Security.Cryptography.RSASignaturePadding.Pkcs1);
        
        // Add Subject Alternative Name
        var sanBuilder = new System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder();
        if (!string.IsNullOrEmpty(request.ComputerName))
            sanBuilder.AddDnsName(request.ComputerName);
        if (!string.IsNullOrEmpty(request.LocalIP))
            sanBuilder.AddIpAddress(System.Net.IPAddress.Parse(request.LocalIP));
        
        req.CertificateExtensions.Add(sanBuilder.Build());
        
        // Set certificate as client authentication
        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: true));
        
        var clientAuthOid = new System.Security.Cryptography.Oid("1.3.6.1.5.5.7.3.2"); // Client Authentication
        req.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new System.Security.Cryptography.OidCollection { clientAuthOid },
                critical: true));
        
        // Create self-signed certificate (in production, use proper CA)
        var cert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));
        
        return cert;
    }
    
    private string ExportCertificateToPem(X509Certificate2 certificate)
    {
        var certBytes = certificate.Export(X509ContentType.Cert);
        return Convert.ToBase64String(certBytes);
    }
    
    private string ExportPrivateKeyToPem(X509Certificate2 certificate)
    {
        if (certificate.PrivateKey == null)
            return string.Empty;
        
        // Export private key (this is simplified - in production, handle this securely)
        var privateKeyBytes = certificate.PrivateKey.ExportPkcs8PrivateKey();
        return Convert.ToBase64String(privateKeyBytes);
    }
}