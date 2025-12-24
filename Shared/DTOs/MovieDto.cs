namespace GiriMovies.Shared.DTOs;

public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
    public string Genre { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public double Rating { get; set; }
    
    // User-specific progress
    public int? CurrentPositionInSeconds { get; set; }
    public double? ProgressPercentage { get; set; }
    public bool? IsCompleted { get; set; }
    public DateTime? LastWatchedAt { get; set; }
    public string? LastWatchedDevice { get; set; }
}
