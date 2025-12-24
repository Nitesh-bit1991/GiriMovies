using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiriMovies.Server.Data;
using GiriMovies.Server.Services;
using GiriMovies.Shared.DTOs;
using GiriMovies.Shared.Models;
using System.Security.Claims;

namespace GiriMovies.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WatchProgressController : ControllerBase
{
    private readonly GiriMoviesDbContext _context;
    private readonly IDeviceFingerprinting _deviceFingerprinting;
    
    public WatchProgressController(GiriMoviesDbContext context, IDeviceFingerprinting deviceFingerprinting)
    {
        _context = context;
        _deviceFingerprinting = deviceFingerprinting;
    }
    
    // POST: api/watchprogress
    [HttpPost]
    public async Task<ActionResult<WatchProgressDto>> UpdateWatchProgress(UpdateWatchProgressRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        // Get device information from current session
        var sessionToken = GetSessionTokenFromRequest();
        var currentSession = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionToken == sessionToken && s.IsActive);
            
        // If session not found by token, try to find the most recent active session for this user
        if (currentSession == null && !string.IsNullOrEmpty(sessionToken))
        {
            currentSession = await _context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .OrderByDescending(s => s.LastActivity ?? s.LoginTime)
                .FirstOrDefaultAsync();
        }
            
        if (currentSession == null)
        {
            // Additional debug info
            var debugInfo = new 
            {
                UserId = userId,
                SessionToken = sessionToken,
                HasSessionToken = !string.IsNullOrEmpty(sessionToken),
                ActiveSessionsCount = await _context.UserSessions.CountAsync(s => s.UserId == userId && s.IsActive)
            };
            return BadRequest($"Device session not found. Debug info: {System.Text.Json.JsonSerializer.Serialize(debugInfo)}");
        }
        
        // Check if movie exists
        var movie = await _context.Movies.FindAsync(request.MovieId);
        if (movie == null)
            return NotFound("Movie not found");
        
        // Find existing watch progress or create new
        var watchProgress = await _context.WatchProgresses
            .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.MovieId == request.MovieId);
        
        if (watchProgress == null)
        {
            watchProgress = new WatchProgress
            {
                UserId = userId,
                MovieId = request.MovieId
            };
            _context.WatchProgresses.Add(watchProgress);
        }
        
        // Update progress with device information
        watchProgress.CurrentPositionInSeconds = request.CurrentPositionInSeconds;
        watchProgress.LastWatchedDevice = currentSession.DeviceType;
        watchProgress.LastWatchedAt = DateTime.UtcNow;
        
        // Store additional device context for cross-device sync
        watchProgress.LastWatchedDeviceId = currentSession.DeviceId;
        watchProgress.LastWatchedDeviceName = currentSession.DeviceName ?? currentSession.ComputerName;
        
        // Calculate progress percentage
        if (movie.DurationInSeconds > 0)
        {
            watchProgress.ProgressPercentage = (double)request.CurrentPositionInSeconds / movie.DurationInSeconds * 100;
            
            // Mark as completed if watched more than 95%
            watchProgress.IsCompleted = watchProgress.ProgressPercentage >= 95;
        }
        
        // Update session activity
        currentSession.LastActivity = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var dto = new WatchProgressDto
        {
            Id = watchProgress.Id,
            UserId = watchProgress.UserId,
            MovieId = watchProgress.MovieId,
            CurrentPositionInSeconds = watchProgress.CurrentPositionInSeconds,
            LastWatchedDevice = watchProgress.LastWatchedDevice,
            LastWatchedDeviceId = watchProgress.LastWatchedDeviceId,
            LastWatchedDeviceName = watchProgress.LastWatchedDeviceName,
            LastWatchedAt = watchProgress.LastWatchedAt,
            IsCompleted = watchProgress.IsCompleted,
            ProgressPercentage = watchProgress.ProgressPercentage
        };
        
        return Ok(dto);
    }
    
    // GET: api/watchprogress/sync
    [HttpGet("sync")]
    public async Task<ActionResult<IEnumerable<WatchProgressDto>>> GetSyncData()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        // Get all watch progress for user, showing latest from any device
        var allProgress = await _context.WatchProgresses
            .Where(wp => wp.UserId == userId)
            .OrderByDescending(wp => wp.LastWatchedAt)
            .ToListAsync();
        
        var progressDtos = allProgress.Select(wp => new WatchProgressDto
        {
            Id = wp.Id,
            UserId = wp.UserId,
            MovieId = wp.MovieId,
            CurrentPositionInSeconds = wp.CurrentPositionInSeconds,
            LastWatchedDevice = wp.LastWatchedDevice,
            LastWatchedDeviceId = wp.LastWatchedDeviceId,
            LastWatchedDeviceName = wp.LastWatchedDeviceName,
            LastWatchedAt = wp.LastWatchedAt,
            IsCompleted = wp.IsCompleted,
            ProgressPercentage = wp.ProgressPercentage
        });
        
        return Ok(progressDtos);
    }
    
    // GET: api/watchprogress/movie/5
    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<WatchProgressDto>> GetWatchProgress(int movieId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var watchProgress = await _context.WatchProgresses
            .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.MovieId == movieId);
        
        if (watchProgress == null)
            return NotFound();
        
        var dto = new WatchProgressDto
        {
            Id = watchProgress.Id,
            UserId = watchProgress.UserId,
            MovieId = watchProgress.MovieId,
            CurrentPositionInSeconds = watchProgress.CurrentPositionInSeconds,
            LastWatchedDevice = watchProgress.LastWatchedDevice,
            LastWatchedDeviceId = watchProgress.LastWatchedDeviceId,
            LastWatchedDeviceName = watchProgress.LastWatchedDeviceName,
            LastWatchedAt = watchProgress.LastWatchedAt,
            IsCompleted = watchProgress.IsCompleted,
            ProgressPercentage = watchProgress.ProgressPercentage
        };
        
        return Ok(dto);
    }
    
    // DELETE: api/watchprogress/movie/5
    [HttpDelete("movie/{movieId}")]
    public async Task<IActionResult> DeleteWatchProgress(int movieId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var watchProgress = await _context.WatchProgresses
            .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.MovieId == movieId);
        
        if (watchProgress == null)
            return NotFound();
        
        _context.WatchProgresses.Remove(watchProgress);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    // GET: api/watchprogress/debug/session
    [HttpGet("debug/session")]
    public async Task<ActionResult> DebugSession()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        var sessionToken = GetSessionTokenFromRequest();
        
        var allSessions = await _context.UserSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginTime)
            .Select(s => new 
            {
                s.Id,
                s.DeviceId,
                s.DeviceName,
                s.SessionToken,
                s.IsActive,
                s.LoginTime,
                s.LastActivity
            })
            .ToListAsync();
        
        return Ok(new 
        {
            CurrentSessionToken = sessionToken,
            UserId = userId,
            AllSessions = allSessions,
            Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
        });
    }
    
    private string? GetSessionTokenFromRequest()
    {
        // First try to get from JWT token claims (preferred method)
        var sessionClaim = User.FindFirst("session_token");
        if (!string.IsNullOrEmpty(sessionClaim?.Value))
        {
            return sessionClaim.Value;
        }
        
        // Fallback: Try to get session token from custom header
        if (Request.Headers.TryGetValue("X-Session-Token", out var sessionToken))
        {
            return sessionToken.FirstOrDefault();
        }
        
        return null;
    }
}
