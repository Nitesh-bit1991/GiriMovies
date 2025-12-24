namespace GiriMovies.Shared.DTOs;

public class UpdateWatchProgressRequest
{
    public int MovieId { get; set; }
    public int CurrentPositionInSeconds { get; set; }
    public string DeviceType { get; set; } = string.Empty;
}

public class WatchProgressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MovieId { get; set; }
    public int CurrentPositionInSeconds { get; set; }
    public string LastWatchedDevice { get; set; } = string.Empty;
    public string? LastWatchedDeviceId { get; set; }
    public string? LastWatchedDeviceName { get; set; }
    public DateTime LastWatchedAt { get; set; }
    public bool IsCompleted { get; set; }
    public double ProgressPercentage { get; set; }
}
