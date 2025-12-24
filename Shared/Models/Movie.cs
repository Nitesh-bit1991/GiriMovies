using System.ComponentModel.DataAnnotations;

namespace GiriMovies.Shared.Models;

public class Movie
{
    public int Id { get; set; }
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string ThumbnailUrl { get; set; } = string.Empty;
    
    public string VideoUrl { get; set; } = string.Empty;
    
    public int DurationInSeconds { get; set; }
    
    public string Genre { get; set; } = string.Empty;
    
    public int ReleaseYear { get; set; }
    
    public double Rating { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<WatchProgress> WatchProgresses { get; set; } = new List<WatchProgress>();
}
