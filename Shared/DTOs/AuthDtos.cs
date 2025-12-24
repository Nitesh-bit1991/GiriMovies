namespace GiriMovies.Shared.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty; // Computer, Mobile, Tablet
    public DeviceInfoDto? DeviceInfo { get; set; } // Enhanced device information
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public string? DeviceId { get; set; } // Return device ID for client storage
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DeviceInfoDto? DeviceInfo { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class UserSessionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DeviceType { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public string? DeviceName { get; set; }
    public string? ComputerName { get; set; }
    public string? MacAddress { get; set; }
    public string? LocalIP { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? Location { get; set; }
    public DateTime LoginTime { get; set; }
    public DateTime? LastActivity { get; set; }
    public DateTime? LogoutTime { get; set; }
    public bool IsActive { get; set; }
}

public class DeviceInfoDto
{
    public string? MacAddress { get; set; }
    public string? ProcessorId { get; set; }
    public string? MachineGuid { get; set; }
    public string? ComputerName { get; set; }
    public string? UserName { get; set; }
    public string? OSVersion { get; set; }
    public string? LocalIP { get; set; }
    public string? TimeZone { get; set; }
    public string? UserAgent { get; set; }
    public string? ScreenResolution { get; set; }
    public string? DeviceName { get; set; } // User-friendly name
}

public class CreateUserSessionRequest
{
    public string DeviceType { get; set; } = string.Empty;
    public DeviceInfoDto DeviceInfo { get; set; } = new();
}

public class CrossDeviceSyncRequest
{
    public string? TargetDeviceId { get; set; } // If null, sync to all devices
}

public class WatchStatusSyncDto
{
    public int MovieId { get; set; }
    public int CurrentPositionInSeconds { get; set; }
    public double ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime LastWatchedAt { get; set; }
    public string LastWatchedDevice { get; set; } = string.Empty;
    public string LastWatchedDeviceName { get; set; } = string.Empty;
}

public class UpdateSessionActivityRequest
{
    public int SessionId { get; set; }
}

// Certificate-based authentication DTOs
public class CertificateEnrollmentRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string ComputerName { get; set; } = string.Empty;
    public string LocalIP { get; set; } = string.Empty;
}

public class CertificateEnrollmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string CertificateThumbprint { get; set; } = string.Empty;
    public string CertificatePem { get; set; } = string.Empty;
    public string PrivateKeyPem { get; set; } = string.Empty;
    public DateTime CertificateValidFrom { get; set; }
    public DateTime CertificateValidTo { get; set; }
}

public class CertificateAuthenticationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string SessionToken { get; set; } = string.Empty;
    public UserDto? User { get; set; }
}
