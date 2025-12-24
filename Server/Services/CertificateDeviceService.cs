using System.Security.Cryptography.X509Certificates;
using GiriMovies.Server.Authentication;

namespace GiriMovies.Server.Services;

public interface ICertificateDeviceService
{
    DeviceCertificateInfo ExtractDeviceInfoFromContext(HttpContext context);
    string GetDeviceIdFromContext(HttpContext context);
    bool HasValidCertificate(HttpContext context);
    Task<DeviceRegistrationResult> RegisterDeviceWithCertificateAsync(X509Certificate2 certificate, int userId);
    Task<bool> RevokeCertificateAsync(string deviceId);
    Task<List<DeviceCertificateDto>> GetUserDeviceCertificatesAsync(int userId);
}

public class CertificateDeviceService : ICertificateDeviceService
{
    private readonly ILogger<CertificateDeviceService> _logger;
    
    public CertificateDeviceService(ILogger<CertificateDeviceService> logger)
    {
        _logger = logger;
    }
    
    public DeviceCertificateInfo ExtractDeviceInfoFromContext(HttpContext context)
    {
        var deviceInfo = context.Items["DeviceInfo"] as DeviceCertificateInfo;
        
        if (deviceInfo != null)
            return deviceInfo;
        
        var certificate = context.Connection.ClientCertificate;
        if (certificate == null)
            throw new InvalidOperationException("No client certificate found");
        
        return ExtractDeviceInfoFromCertificate(certificate);
    }
    
    public string GetDeviceIdFromContext(HttpContext context)
    {
        var deviceId = context.Items["DeviceId"] as string;
        
        if (!string.IsNullOrEmpty(deviceId))
            return deviceId;
        
        var certificate = context.Connection.ClientCertificate;
        if (certificate == null)
            throw new InvalidOperationException("No client certificate found");
        
        return certificate.Thumbprint;
    }
    
    public bool HasValidCertificate(HttpContext context)
    {
        var certificate = context.Connection.ClientCertificate;
        return certificate != null && IsValidCertificate(certificate);
    }
    
    public async Task<DeviceRegistrationResult> RegisterDeviceWithCertificateAsync(X509Certificate2 certificate, int userId)
    {
        try
        {
            var deviceInfo = ExtractDeviceInfoFromCertificate(certificate);
            
            var result = new DeviceRegistrationResult
            {
                Success = true,
                DeviceId = certificate.Thumbprint,
                DeviceName = deviceInfo.DeviceName,
                DeviceType = deviceInfo.DeviceType,
                CertificateThumbprint = certificate.Thumbprint,
                CertificateSubject = certificate.Subject,
                CertificateIssuer = certificate.Issuer,
                CertificateValidFrom = certificate.NotBefore,
                CertificateValidTo = certificate.NotAfter,
                Message = "Device registered successfully with certificate"
            };
            
            _logger.LogInformation("Device registered with certificate: {DeviceId} for user {UserId}", 
                certificate.Thumbprint, userId);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device with certificate for user {UserId}", userId);
            
            return new DeviceRegistrationResult
            {
                Success = false,
                Message = $"Failed to register device: {ex.Message}"
            };
        }
    }
    
    public Task<bool> RevokeCertificateAsync(string deviceId)
    {
        try
        {
            // In a real implementation, you would:
            // 1. Add certificate to Certificate Revocation List (CRL)
            // 2. Update database to mark device as revoked
            // 3. Notify other services
            
            _logger.LogInformation("Certificate revoked for device: {DeviceId}", deviceId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking certificate for device {DeviceId}", deviceId);
            return Task.FromResult(false);
        }
    }
    
    public Task<List<DeviceCertificateDto>> GetUserDeviceCertificatesAsync(int userId)
    {
        // In a real implementation, query database for user's registered certificates
        var certificates = new List<DeviceCertificateDto>();
        
        return Task.FromResult(certificates);
    }
    
    private DeviceCertificateInfo ExtractDeviceInfoFromCertificate(X509Certificate2 certificate)
    {
        var deviceInfo = new DeviceCertificateInfo
        {
            DeviceId = certificate.Thumbprint,
            CertificateIssued = certificate.NotBefore,
            CertificateExpires = certificate.NotAfter
        };
        
        // Parse certificate subject (CN=DeviceName,OU=DeviceType,O=GiriMovies,C=US)
        var subjectParts = certificate.Subject.Split(',').Select(p => p.Trim()).ToArray();
        
        foreach (var part in subjectParts)
        {
            if (part.StartsWith("CN="))
                deviceInfo.DeviceName = part.Substring(3);
            else if (part.StartsWith("OU="))
                deviceInfo.DeviceType = part.Substring(3);
        }
        
        // Parse Subject Alternative Name for additional info
        foreach (var extension in certificate.Extensions)
        {
            if (extension.Oid?.Value == "2.5.29.17") // Subject Alternative Name
            {
                var sanData = extension.Format(false);
                ParseSubjectAlternativeName(sanData, deviceInfo);
            }
        }
        
        return deviceInfo;
    }
    
    private void ParseSubjectAlternativeName(string sanData, DeviceCertificateInfo deviceInfo)
    {
        var parts = sanData.Split(',').Select(p => p.Trim()).ToArray();
        
        foreach (var part in parts)
        {
            if (part.StartsWith("DNS Name="))
                deviceInfo.ComputerName = part.Substring(9);
            else if (part.StartsWith("IP Address="))
                deviceInfo.LocalIP = part.Substring(11);
        }
    }
    
    private bool IsValidCertificate(X509Certificate2 certificate)
    {
        try
        {
            // Check expiration
            if (certificate.NotAfter < DateTime.UtcNow || certificate.NotBefore > DateTime.UtcNow)
                return false;
            
            // Check issuer
            if (!certificate.Issuer.Contains("GiriMovies Local CA"))
                return false;
            
            // Additional validations can be added here
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class DeviceRegistrationResult
{
    public bool Success { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public string CertificateSubject { get; set; } = string.Empty;
    public string CertificateIssuer { get; set; } = string.Empty;
    public DateTime CertificateValidFrom { get; set; }
    public DateTime CertificateValidTo { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class DeviceCertificateDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public string CertificateSubject { get; set; } = string.Empty;
    public DateTime CertificateValidFrom { get; set; }
    public DateTime CertificateValidTo { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime? LastUsed { get; set; }
}