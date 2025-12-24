using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiriMovies.Server.Data;
using GiriMovies.Shared.DTOs;
using System.Security.Claims;

namespace GiriMovies.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MoviesController : ControllerBase
{
    private readonly GiriMoviesDbContext _context;
    
    public MoviesController(GiriMoviesDbContext context)
    {
        _context = context;
    }
    
    // GET: api/movies
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetMovies()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var movies = await _context.Movies
            .Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ThumbnailUrl = m.ThumbnailUrl,
                VideoUrl = m.VideoUrl,
                DurationInSeconds = m.DurationInSeconds,
                Genre = m.Genre,
                ReleaseYear = m.ReleaseYear,
                Rating = m.Rating,
                CurrentPositionInSeconds = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (int?)wp.CurrentPositionInSeconds)
                    .FirstOrDefault(),
                ProgressPercentage = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (double?)wp.ProgressPercentage)
                    .FirstOrDefault(),
                IsCompleted = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (bool?)wp.IsCompleted)
                    .FirstOrDefault(),
                LastWatchedAt = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (DateTime?)wp.LastWatchedAt)
                    .FirstOrDefault(),
                LastWatchedDevice = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => wp.LastWatchedDevice)
                    .FirstOrDefault()
            })
            .ToListAsync();
        
        return Ok(movies);
    }
    
    // GET: api/movies/5
    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDto>> GetMovie(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var movie = await _context.Movies
            .Where(m => m.Id == id)
            .Select(m => new MovieDto
            {
                Id = m.Id,
                Title = m.Title,
                Description = m.Description,
                ThumbnailUrl = m.ThumbnailUrl,
                VideoUrl = m.VideoUrl,
                DurationInSeconds = m.DurationInSeconds,
                Genre = m.Genre,
                ReleaseYear = m.ReleaseYear,
                Rating = m.Rating,
                CurrentPositionInSeconds = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (int?)wp.CurrentPositionInSeconds)
                    .FirstOrDefault(),
                ProgressPercentage = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (double?)wp.ProgressPercentage)
                    .FirstOrDefault(),
                IsCompleted = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (bool?)wp.IsCompleted)
                    .FirstOrDefault(),
                LastWatchedAt = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => (DateTime?)wp.LastWatchedAt)
                    .FirstOrDefault(),
                LastWatchedDevice = m.WatchProgresses
                    .Where(wp => wp.UserId == userId)
                    .Select(wp => wp.LastWatchedDevice)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();
        
        if (movie == null)
            return NotFound();
        
        return Ok(movie);
    }
    
    // GET: api/movies/continue-watching
    [HttpGet("continue-watching")]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetContinueWatching()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var movies = await _context.WatchProgresses
            .Where(wp => wp.UserId == userId && !wp.IsCompleted && wp.CurrentPositionInSeconds > 0)
            .OrderByDescending(wp => wp.LastWatchedAt)
            .Take(10)
            .Select(wp => new MovieDto
            {
                Id = wp.Movie.Id,
                Title = wp.Movie.Title,
                Description = wp.Movie.Description,
                ThumbnailUrl = wp.Movie.ThumbnailUrl,
                VideoUrl = wp.Movie.VideoUrl,
                DurationInSeconds = wp.Movie.DurationInSeconds,
                Genre = wp.Movie.Genre,
                ReleaseYear = wp.Movie.ReleaseYear,
                Rating = wp.Movie.Rating,
                CurrentPositionInSeconds = wp.CurrentPositionInSeconds,
                ProgressPercentage = wp.ProgressPercentage,
                IsCompleted = wp.IsCompleted,
                LastWatchedAt = wp.LastWatchedAt,
                LastWatchedDevice = wp.LastWatchedDevice
            })
            .ToListAsync();
        
        return Ok(movies);
    }
}
