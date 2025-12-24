using GiriMovies.Shared.DTOs;
using System.Net.Http.Json;

namespace GiriMovies.Client.Services;

public interface IUserSessionService
{
    Task<List<UserSessionDto>> GetUserSessionsAsync();
    Task<List<UserSessionDto>> GetActiveSessionsAsync();
    Task<UserSessionDto?> GetCurrentSessionAsync();
    Task UpdateActivityAsync();
    Task LogoutSessionAsync(int sessionId);
    Task DeleteSessionAsync(int sessionId);
}

public class UserSessionService : IUserSessionService
{
    private readonly HttpClient _httpClient;
    
    public UserSessionService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<UserSessionDto>> GetUserSessionsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<UserSessionDto>>("api/usersession");
            return response ?? new List<UserSessionDto>();
        }
        catch
        {
            return new List<UserSessionDto>();
        }
    }
    
    public async Task<List<UserSessionDto>> GetActiveSessionsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<UserSessionDto>>("api/usersession/active");
            return response ?? new List<UserSessionDto>();
        }
        catch
        {
            return new List<UserSessionDto>();
        }
    }
    
    public async Task<UserSessionDto?> GetCurrentSessionAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserSessionDto>("api/usersession/current");
        }
        catch
        {
            return null;
        }
    }
    
    public async Task UpdateActivityAsync()
    {
        try
        {
            await _httpClient.PostAsync("api/usersession/activity", null);
        }
        catch
        {
            // Silently fail - this is just for activity tracking
        }
    }
    
    public async Task LogoutSessionAsync(int sessionId)
    {
        try
        {
            await _httpClient.PostAsync($"api/usersession/{sessionId}/logout", null);
        }
        catch
        {
            throw new Exception("Failed to logout session");
        }
    }
    
    public async Task DeleteSessionAsync(int sessionId)
    {
        try
        {
            await _httpClient.DeleteAsync($"api/usersession/{sessionId}");
        }
        catch
        {
            throw new Exception("Failed to delete session");
        }
    }
}