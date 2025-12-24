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
public class UserSessionController : ControllerBase
{
    private readonly GiriMoviesDbContext _context;
    private readonly IDeviceFingerprinting _deviceFingerprinting;
    
    public UserSessionController(GiriMoviesDbContext context, IDeviceFingerprinting deviceFingerprinting)
    {
        _context = context;
        _deviceFingerprinting = deviceFingerprinting;
    }
    
    // GET: api/usersession
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserSessions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LoginTime)
            .ToListAsync();
        
        var sessionDtos = sessions.Select(s => new UserSessionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            DeviceType = s.DeviceType,
            DeviceId = s.DeviceId,
            DeviceName = s.DeviceName,
            ComputerName = s.ComputerName,
            MacAddress = s.MacAddress,
            LocalIP = s.LocalIP,
            UserAgent = s.UserAgent,
            IpAddress = s.IpAddress,
            Location = s.Location,
            LoginTime = s.LoginTime,
            LastActivity = s.LastActivity,
            LogoutTime = s.LogoutTime,
            IsActive = s.IsActive
        });
        
        return Ok(sessionDtos);
    }
    
    // GET: api/usersession/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetActiveSessions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var activeSessions = await _context.UserSessions
            .Where(s => s.UserId == userId && s.IsActive)
            .OrderByDescending(s => s.LastActivity ?? s.LoginTime)
            .ToListAsync();
        
        var sessionDtos = activeSessions.Select(s => new UserSessionDto
        {
            Id = s.Id,
            UserId = s.UserId,
            DeviceType = s.DeviceType,
            DeviceId = s.DeviceId,
            DeviceName = s.DeviceName,
            ComputerName = s.ComputerName,
            MacAddress = s.MacAddress,
            LocalIP = s.LocalIP,
            UserAgent = s.UserAgent,
            IpAddress = s.IpAddress,
            Location = s.Location,
            LoginTime = s.LoginTime,
            LastActivity = s.LastActivity,
            LogoutTime = s.LogoutTime,
            IsActive = s.IsActive
        });
        
        return Ok(sessionDtos);
    }
    
    // POST: api/usersession/activity
    [HttpPost("activity")]
    public async Task<IActionResult> UpdateActivity()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        var sessionToken = GetSessionTokenFromRequest();
        
        if (string.IsNullOrEmpty(sessionToken))
            return BadRequest("Session token not found");
        
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionToken == sessionToken && s.IsActive);
        
        if (session == null)
            return NotFound("Session not found");
        
        session.LastActivity = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    // POST: api/usersession/{sessionId}/logout
    [HttpPost("{sessionId}/logout")]
    public async Task<IActionResult> LogoutSession(int sessionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        
        if (session == null)
            return NotFound("Session not found");
        
        session.IsActive = false;
        session.LogoutTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Ok();
    }
    
    // DELETE: api/usersession/{sessionId}
    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> DeleteSession(int sessionId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        
        if (session == null)
            return NotFound("Session not found");
        
        _context.UserSessions.Remove(session);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    // GET: api/usersession/current
    [HttpGet("current")]
    public async Task<ActionResult<UserSessionDto>> GetCurrentSession()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        var sessionToken = GetSessionTokenFromRequest();
        
        if (string.IsNullOrEmpty(sessionToken))
            return BadRequest("Session token not found");
        
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionToken == sessionToken && s.IsActive);
        
        if (session == null)
            return NotFound("Session not found");
        
        var sessionDto = new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            DeviceType = session.DeviceType,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
            ComputerName = session.ComputerName,
            MacAddress = session.MacAddress,
            LocalIP = session.LocalIP,
            UserAgent = session.UserAgent,
            IpAddress = session.IpAddress,
            Location = session.Location,
            LoginTime = session.LoginTime,
            LastActivity = session.LastActivity,
            LogoutTime = session.LogoutTime,
            IsActive = session.IsActive
        };
        
        return Ok(sessionDto);
    }
    
    // GET: api/usersession/sync/watch-status
    [HttpGet("sync/watch-status")]
    public async Task<ActionResult<IEnumerable<WatchStatusSyncDto>>> GetWatchStatusForSync()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        // Get all watch progress for the user
        var watchProgresses = await _context.WatchProgresses
            .Include(wp => wp.Movie)
            .Where(wp => wp.UserId == userId)
            .OrderByDescending(wp => wp.LastWatchedAt)
            .ToListAsync();
        
        var syncData = watchProgresses.Select(wp => new WatchStatusSyncDto
        {
            MovieId = wp.MovieId,
            CurrentPositionInSeconds = wp.CurrentPositionInSeconds,
            ProgressPercentage = wp.ProgressPercentage,
            IsCompleted = wp.IsCompleted,
            LastWatchedAt = wp.LastWatchedAt,
            LastWatchedDevice = wp.LastWatchedDevice ?? "Unknown",
            LastWatchedDeviceName = GetDeviceNameFromProgress(wp.LastWatchedDevice, userId)
        });
        
        return Ok(syncData);
    }
    
    // POST: api/usersession/register-device
    [HttpPost("register-device")]
    public async Task<ActionResult<UserSessionDto>> RegisterDevice(CreateUserSessionRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
            return Unauthorized();
        
        var userId = int.Parse(userIdClaim.Value);
        
        // Convert DTO to server model
        var deviceInfo = new DeviceInfo
        {
            MacAddress = request.DeviceInfo.MacAddress,
            ProcessorId = request.DeviceInfo.ProcessorId,
            MachineGuid = request.DeviceInfo.MachineGuid,
            ComputerName = request.DeviceInfo.ComputerName,
            UserName = request.DeviceInfo.UserName,
            OSVersion = request.DeviceInfo.OSVersion,
            LocalIP = request.DeviceInfo.LocalIP,
            TimeZone = request.DeviceInfo.TimeZone,
            UserAgent = request.DeviceInfo.UserAgent,
            ScreenResolution = request.DeviceInfo.ScreenResolution,
            DeviceName = request.DeviceInfo.DeviceName
        };
        
        var deviceId = _deviceFingerprinting.GenerateDeviceFingerprint(deviceInfo);
        
        // Check if device is already registered
        var existingSession = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == userId && s.DeviceId == deviceId);
        
        if (existingSession != null)
        {
            // Update existing session
            existingSession.LastActivity = DateTime.UtcNow;
            existingSession.IsActive = true;
            await _context.SaveChangesAsync();
            
            return Ok(MapToDto(existingSession));
        }
        
        // Create new device session
        var newSession = new UserSession
        {
            UserId = userId,
            DeviceId = deviceId,
            DeviceType = request.DeviceType,
            DeviceName = deviceInfo.DeviceName ?? deviceInfo.ComputerName ?? "Unknown Device",
            ComputerName = deviceInfo.ComputerName,
            MacAddress = deviceInfo.MacAddress,
            ProcessorId = deviceInfo.ProcessorId,
            LocalIP = deviceInfo.LocalIP,
            UserAgent = deviceInfo.UserAgent,
            IpAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            LoginTime = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            SessionToken = Guid.NewGuid().ToString(),
            IsActive = true
        };
        
        _context.UserSessions.Add(newSession);
        await _context.SaveChangesAsync();
        
        return Ok(MapToDto(newSession));
    }
    
    private UserSessionDto MapToDto(UserSession session)
    {
        return new UserSessionDto
        {
            Id = session.Id,
            UserId = session.UserId,
            DeviceType = session.DeviceType,
            DeviceId = session.DeviceId,
            DeviceName = session.DeviceName,
            ComputerName = session.ComputerName,
            MacAddress = session.MacAddress,
            LocalIP = session.LocalIP,
            UserAgent = session.UserAgent,
            IpAddress = session.IpAddress,
            Location = session.Location,
            LoginTime = session.LoginTime,
            LastActivity = session.LastActivity,
            LogoutTime = session.LogoutTime,
            IsActive = session.IsActive
        };
    }
    
    private string GetDeviceNameFromProgress(string? deviceType, int userId)
    {
        // Try to find the device name from the latest session with matching device type
        var session = _context.UserSessions
            .Where(s => s.UserId == userId && s.DeviceType == deviceType && s.IsActive)
            .OrderByDescending(s => s.LastActivity)
            .FirstOrDefault();
            
        return session?.DeviceName ?? session?.ComputerName ?? deviceType ?? "Unknown Device";
    }
    
    private string? GetSessionTokenFromRequest()
    {
        // Try to get session token from custom header
        if (Request.Headers.TryGetValue("X-Session-Token", out var sessionToken))
        {
            return sessionToken.FirstOrDefault();
        }
        
        // Alternative: Get from JWT token claims if stored there
        var sessionClaim = User.FindFirst("session_token");
        return sessionClaim?.Value;
    }
}