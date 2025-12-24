using System.Security.Cryptography.X509Certificates;


namespace GiriMovies.Server.Authentication;

public class ClientCertificateAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClientCertificateAuthenticationMiddleware> _logger;
    
    public ClientCertificateAuthenticationMiddleware(
        RequestDelegate next, 
        ILogger<ClientCertificateAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip certificate validation for certain endpoints
        if (ShouldSkipCertificateValidation(context.Request.Path))
        {
            await _next(context);
            return;
        }
        
        var clientCertificate = context.Connection.ClientCertificate;
        
        if (clientCertificate == null)
        {
            _logger.LogWarning("No client certificate provided");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Client certificate required");
            return;
        }
        
        try
        {
            // Validate certificate
            if (!IsValidClientCertificate(clientCertificate))
            {
                _logger.LogWarning("Invalid client certificate: {Subject}", clientCertificate.Subject);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Invalid client certificate");
                return;
            }
            
            // Extract device information from certificate
            var deviceInfo = ExtractDeviceInfoFromCertificate(clientCertificate);
            
            // Add device info to HTTP context for downstream use
            context.Items["ClientCertificate"] = clientCertificate;
            context.Items["DeviceInfo"] = deviceInfo;
            context.Items["DeviceId"] = GetDeviceIdFromCertificate(clientCertificate);
            
            _logger.LogInformation("Valid client certificate for device: {DeviceId}", 
                GetDeviceIdFromCertificate(clientCertificate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating client certificate");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Certificate validation error");
            return;
        }
        
        await _next(context);
    }
    
    private bool ShouldSkipCertificateValidation(PathString path)
    {
        // Skip certificate validation for static files and certain API endpoints
        var staticFileExtensions = new[] { ".css", ".js", ".json", ".wasm", ".dll", ".pdb", ".br", ".gz", ".woff", ".woff2", ".ttf", ".eot", ".svg", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".html" };
        
        // Check if path is a static file by extension
        if (staticFileExtensions.Any(ext => path.Value?.EndsWith(ext, StringComparison.OrdinalIgnoreCase) == true))
        {
            return true;
        }
        
        // Skip specific paths
        var skipPaths = new[]
        {
            "/api/certificates/enroll",
            "/api/auth/login",
            "/api/auth/register",
            "/api/health",
            "/swagger",
            "/_framework",
            "/css",
            "/js",
            "/favicon.ico",
            "/index.html"
        };
        
        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }
    
    private bool IsValidClientCertificate(X509Certificate2 certificate)
    {
        try
        {
            // Check if certificate is not expired
            if (certificate.NotAfter < DateTime.UtcNow)
            {
                _logger.LogWarning("Certificate expired: {NotAfter}", certificate.NotAfter);
                return false;
            }
            
            if (certificate.NotBefore > DateTime.UtcNow)
            {
                _logger.LogWarning("Certificate not yet valid: {NotBefore}", certificate.NotBefore);
                return false;
            }
            
            // Check if certificate is issued by our CA
            if (!IsIssuedByTrustedCA(certificate))
            {
                _logger.LogWarning("Certificate not issued by trusted CA");
                return false;
            }
            
            // Verify certificate chain
            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck; // Offline scenario
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreInvalidBasicConstraints;
            
            var chainValid = chain.Build(certificate);
            if (!chainValid)
            {
                _logger.LogWarning("Certificate chain validation failed");
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating certificate");
            return false;
        }
    }
    
    private bool IsIssuedByTrustedCA(X509Certificate2 certificate)
    {
        // Check if issued by our GiriMovies Local CA
        var expectedIssuer = "CN=GiriMovies Local CA, O=GiriMovies Blazor, C=US";
        return certificate.Issuer.Contains("GiriMovies Local CA") || 
               certificate.Issuer == expectedIssuer;
    }
    
    private DeviceCertificateInfo ExtractDeviceInfoFromCertificate(X509Certificate2 certificate)
    {
        var deviceInfo = new DeviceCertificateInfo();
        
        // Extract device information from certificate subject
        var subject = certificate.Subject;
        
        // Parse CN=DeviceName,OU=DeviceType,O=GiriMovies,C=US format
        var parts = subject.Split(',').Select(p => p.Trim()).ToArray();
        
        foreach (var part in parts)
        {
            if (part.StartsWith("CN="))
                deviceInfo.DeviceName = part.Substring(3);
            else if (part.StartsWith("OU="))
                deviceInfo.DeviceType = part.Substring(3);
        }
        
        // Extract device ID from certificate serial number or subject alternative name
        deviceInfo.DeviceId = GetDeviceIdFromCertificate(certificate);
        
        // Extract additional info from certificate extensions
        foreach (var extension in certificate.Extensions)
        {
            if (extension.Oid?.Value == "2.5.29.17") // Subject Alternative Name
            {
                // Parse SAN for additional device info
                var sanData = extension.Format(false);
                ParseSubjectAlternativeName(sanData, deviceInfo);
            }
        }
        
        return deviceInfo;
    }
    
    private void ParseSubjectAlternativeName(string sanData, DeviceCertificateInfo deviceInfo)
    {
        // Parse SAN data like: "DNS Name=DESKTOP-ABC123, IP Address=192.168.1.100"
        var parts = sanData.Split(',').Select(p => p.Trim()).ToArray();
        
        foreach (var part in parts)
        {
            if (part.StartsWith("DNS Name="))
                deviceInfo.ComputerName = part.Substring(9);
            else if (part.StartsWith("IP Address="))
                deviceInfo.LocalIP = part.Substring(11);
        }
    }
    
    private string GetDeviceIdFromCertificate(X509Certificate2 certificate)
    {
        // Use certificate thumbprint as device ID (unique per certificate)
        return certificate.Thumbprint;
    }
}

public class DeviceCertificateInfo
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string LocalIP { get; set; } = string.Empty;
    public DateTime CertificateIssued { get; set; }
    public DateTime CertificateExpires { get; set; }
}