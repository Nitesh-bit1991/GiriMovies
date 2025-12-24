using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GiriMovies.Server.Services;

public interface IDeviceFingerprinting
{
    string GenerateDeviceId(HttpRequest request, DeviceInfo? clientDeviceInfo = null);
    string GenerateDeviceFingerprint(DeviceInfo deviceInfo);
    DeviceInfo ExtractDeviceInfo(HttpRequest request);
    bool IsDeviceRecognized(string deviceId, string newFingerprint);
}

public class DeviceFingerprinting : IDeviceFingerprinting
{
    public string GenerateDeviceId(HttpRequest request, DeviceInfo? clientDeviceInfo = null)
    {
        DeviceInfo deviceInfo;
        
        if (clientDeviceInfo != null)
        {
            // Use client-provided device info (more accurate for offline scenarios)
            deviceInfo = clientDeviceInfo;
        }
        else
        {
            // Fallback to server-side detection
            deviceInfo = ExtractDeviceInfo(request);
        }
        
        return GenerateDeviceFingerprint(deviceInfo);
    }
    
    public string GenerateDeviceFingerprint(DeviceInfo deviceInfo)
    {
        // Create a unique device identifier based on HARDWARE characteristics only
        // Exclude browser-specific info so the same computer is recognized across different browsers
        var fingerprintData = new
        {
            // Hardware identifiers (most stable) - PRIMARY identifiers
            MacAddress = deviceInfo.MacAddress,
            ProcessorId = deviceInfo.ProcessorId,
            MachineGuid = deviceInfo.MachineGuid,
            
            // System identifiers - SECONDARY identifiers
            ComputerName = deviceInfo.ComputerName,
            UserName = deviceInfo.UserName,
            OSVersion = deviceInfo.OSVersion,
            
            // Network identifiers - TERTIARY identifiers
            LocalIP = deviceInfo.LocalIP,
            TimeZone = deviceInfo.TimeZone
            
            // EXCLUDED: UserAgent and ScreenResolution to ensure same device across browsers
            // UserAgent = deviceInfo.UserAgent,
            // ScreenResolution = deviceInfo.ScreenResolution
        };
        
        var json = JsonSerializer.Serialize(fingerprintData);
        return ComputeHash(json);
    }
    
    public DeviceInfo ExtractDeviceInfo(HttpRequest request)
    {
        return new DeviceInfo
        {
            // Extract from headers sent by client
            MacAddress = request.Headers["X-Mac-Address"].FirstOrDefault(),
            ProcessorId = request.Headers["X-Processor-Id"].FirstOrDefault(),
            MachineGuid = request.Headers["X-Machine-Guid"].FirstOrDefault(),
            ComputerName = request.Headers["X-Computer-Name"].FirstOrDefault(),
            UserName = request.Headers["X-User-Name"].FirstOrDefault(),
            OSVersion = request.Headers["X-OS-Version"].FirstOrDefault(),
            LocalIP = GetClientLocalIP(request),
            TimeZone = request.Headers["X-Timezone"].FirstOrDefault(),
            UserAgent = request.Headers.UserAgent.FirstOrDefault(),
            ScreenResolution = request.Headers["X-Screen-Resolution"].FirstOrDefault(),
            DeviceName = request.Headers["X-Device-Name"].FirstOrDefault()
        };
    }
    
    public bool IsDeviceRecognized(string deviceId, string newFingerprint)
    {
        // Simple comparison - in production, you might want fuzzy matching
        return deviceId == newFingerprint;
    }
    
    private string GetClientLocalIP(HttpRequest request)
    {
        // Try various headers for local IP
        var possibleHeaders = new[] 
        {
            "X-Local-IP",
            "X-Client-IP",
            "X-Forwarded-For",
            "X-Real-IP"
        };
        
        foreach (var header in possibleHeaders)
        {
            if (request.Headers.TryGetValue(header, out var value))
            {
                return value.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim() ?? "";
            }
        }
        
        return request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
    }
    
    private string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash)[..16]; // First 16 chars for readability
    }
}

public class DeviceInfo
{
    // Hardware identifiers (most reliable)
    public string? MacAddress { get; set; }
    public string? ProcessorId { get; set; }
    public string? MachineGuid { get; set; } // Windows GUID
    
    // System identifiers
    public string? ComputerName { get; set; }
    public string? UserName { get; set; }
    public string? OSVersion { get; set; }
    
    // Network identifiers
    public string? LocalIP { get; set; }
    public string? TimeZone { get; set; }
    
    // Browser/App identifiers
    public string? UserAgent { get; set; }
    public string? ScreenResolution { get; set; }
    
    // User-friendly identifier
    public string? DeviceName { get; set; }
}