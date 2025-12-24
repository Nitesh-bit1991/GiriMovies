using System.ComponentModel.DataAnnotations;

namespace GiriMovies.Shared.Models;

public class WatchProgress
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    public User User { get; set; } = null!;
    
    [Required]
    public int MovieId { get; set; }
    
    public Movie Movie { get; set; } = null!;
    
    // Current position in seconds
    public int CurrentPositionInSeconds { get; set; }
    
    // Device type: Computer, Mobile, Tablet
    public string LastWatchedDevice { get; set; } = string.Empty;
    
    // Device fingerprint/ID for tracking specific device
    public string? LastWatchedDeviceId { get; set; }
    
    // User-friendly device name
    public string? LastWatchedDeviceName { get; set; }
    
    public DateTime LastWatchedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsCompleted { get; set; }
    
    // Percentage watched (0-100)
    public double ProgressPercentage { get; set; }
}
