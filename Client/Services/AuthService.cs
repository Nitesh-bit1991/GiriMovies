using System.Net.Http.Json;
using System.Net.Http.Headers;
using GiriMovies.Shared.DTOs;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using GiriMovies.Client.Authentication;

namespace GiriMovies.Client.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<UserDto?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;
    private const string TokenKey = "authToken";
    private const string UserKey = "currentUser";
    
    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }
    
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
        {
            await _localStorage.SetItemAsync(TokenKey, result.Token);
            await _localStorage.SetItemAsync(UserKey, result.User);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", result.Token);
            
            // Notify authentication state changed
            ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated();
        }
        
        return result ?? new LoginResponse { Success = false, Message = "Unknown error" };
    }
    
    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        
        if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
        {
            await _localStorage.SetItemAsync(TokenKey, result.Token);
            await _localStorage.SetItemAsync(UserKey, result.User);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", result.Token);
            
            // Notify authentication state changed
            ((CustomAuthStateProvider)_authStateProvider).MarkUserAsAuthenticated();
        }
        
        return result ?? new LoginResponse { Success = false, Message = "Unknown error" };
    }
    
    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        await _localStorage.RemoveItemAsync(UserKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        
        // Notify authentication state changed
        ((CustomAuthStateProvider)_authStateProvider).MarkUserAsLoggedOut();
    }
    
    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>(TokenKey);
    }
    
    public async Task<UserDto?> GetCurrentUserAsync()
    {
        return await _localStorage.GetItemAsync<UserDto>(UserKey);
    }
    
    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }
}
