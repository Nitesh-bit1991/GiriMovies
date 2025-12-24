using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GiriMovies.Server.Data;
using GiriMovies.Server.Services;
using GiriMovies.Shared.DTOs;
using GiriMovies.Shared.Models;

namespace GiriMovies.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly GiriMoviesDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDeviceFingerprinting _deviceFingerprinting;
    
    public AuthController(
        GiriMoviesDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDeviceFingerprinting deviceFingerprinting)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _deviceFingerprinting = deviceFingerprinting;
    }
    
    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request)
    {
        // Check if user already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new LoginResponse
            {
                Success = false,
                Message = "User with this email already exists"
            });
        }
        
        // Create new user
        var user = new User
        {
            Email = request.Email,
            Name = request.Name,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        // Generate token
        var token = _tokenService.GenerateToken(user);
        
        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            Message = "Registration successful",
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            }
        });
    }
    
    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new LoginResponse
            {
                Success = false,
                Message = "Invalid email or password"
            });
        }
        
        // Generate device fingerprint
        var deviceInfo = ConvertToServerDeviceInfo(request.DeviceInfo);
        var deviceId = _deviceFingerprinting.GenerateDeviceId(Request, deviceInfo);
        
        // Check if this device already has an active session
        var existingSession = await _context.UserSessions
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.DeviceId == deviceId && s.IsActive);
        
        if (existingSession != null)
        {
            // Update existing session
            existingSession.LastActivity = DateTime.UtcNow;
            existingSession.LoginTime = DateTime.UtcNow; // Update login time
            existingSession.UserAgent = Request.Headers.UserAgent.FirstOrDefault();
            existingSession.IpAddress = GetClientIpAddress();
            // Generate new session token for security
            existingSession.SessionToken = Guid.NewGuid().ToString();
        }
        else
        {
            // Create new session
            existingSession = new UserSession
            {
                UserId = user.Id,
                DeviceType = request.DeviceType,
                DeviceId = deviceId,
                DeviceName = request.DeviceInfo?.DeviceName ?? request.DeviceInfo?.ComputerName ?? "Unknown Device",
                ComputerName = request.DeviceInfo?.ComputerName,
                MacAddress = request.DeviceInfo?.MacAddress,
                ProcessorId = request.DeviceInfo?.ProcessorId,
                LocalIP = request.DeviceInfo?.LocalIP,
                UserAgent = Request.Headers.UserAgent.FirstOrDefault(),
                IpAddress = GetClientIpAddress(),
                LoginTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true,
                SessionToken = Guid.NewGuid().ToString()
            };
            _context.UserSessions.Add(existingSession);
        }
        
        // Update user last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // Generate token with session info
        var token = _tokenService.GenerateToken(user, existingSession.SessionToken);
        
        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            DeviceId = deviceId,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name
            }
        });
    }
    
    // GET: api/auth/devices
    [HttpGet("devices")]
    public async Task<ActionResult<IEnumerable<UserSessionDto>>> GetUserDevices([FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest("Email is required");
            
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound("User not found");
        
        var devices = await _context.UserSessions
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.LastActivity ?? s.LoginTime)
            .ToListAsync();
        
        var deviceDtos = devices.Select(d => new UserSessionDto
        {
            Id = d.Id,
            UserId = d.UserId,
            DeviceType = d.DeviceType,
            DeviceId = d.DeviceId,
            DeviceName = d.DeviceName,
            ComputerName = d.ComputerName,
            MacAddress = d.MacAddress,
            LocalIP = d.LocalIP,
            UserAgent = d.UserAgent,
            IpAddress = d.IpAddress,
            LoginTime = d.LoginTime,
            LastActivity = d.LastActivity,
            LogoutTime = d.LogoutTime,
            IsActive = d.IsActive
        });
        
        return Ok(deviceDtos);
    }
    
    private DeviceInfo? ConvertToServerDeviceInfo(DeviceInfoDto? dto)
    {
        if (dto == null) return null;
        
        return new DeviceInfo
        {
            MacAddress = dto.MacAddress,
            ProcessorId = dto.ProcessorId,
            MachineGuid = dto.MachineGuid,
            ComputerName = dto.ComputerName,
            UserName = dto.UserName,
            OSVersion = dto.OSVersion,
            LocalIP = dto.LocalIP,
            TimeZone = dto.TimeZone,
            UserAgent = dto.UserAgent,
            ScreenResolution = dto.ScreenResolution,
            DeviceName = dto.DeviceName
        };
    }
    
    private string? GetDeviceId()
    {
        // Try to get device ID from header
        if (Request.Headers.TryGetValue("X-Device-Id", out var deviceId))
        {
            return deviceId.FirstOrDefault();
        }
        
        // Generate a temporary device ID based on user agent and IP
        var userAgent = GetUserAgent();
        var ipAddress = GetClientIpAddress();
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userAgent}-{ipAddress}")).Substring(0, 20);
    }
    
    private string? GetUserAgent()
    {
        return Request.Headers["User-Agent"].FirstOrDefault();
    }
    
    private string? GetClientIpAddress()
    {
        // Check for forwarded IP first (in case of proxy/load balancer)
        if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            return forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }
        
        if (Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            return realIp.FirstOrDefault();
        }
        
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}
