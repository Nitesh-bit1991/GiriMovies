using System.Net.Http.Json;
using GiriMovies.Shared.DTOs;

namespace GiriMovies.Client.Services;

public interface IWatchProgressService
{
    Task<WatchProgressDto?> UpdateWatchProgressAsync(UpdateWatchProgressRequest request);
    Task<WatchProgressDto?> GetWatchProgressAsync(int movieId);
    Task<bool> DeleteWatchProgressAsync(int movieId);
}

public class WatchProgressService : IWatchProgressService
{
    private readonly HttpClient _httpClient;
    
    public WatchProgressService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<WatchProgressDto?> UpdateWatchProgressAsync(UpdateWatchProgressRequest request)
    {
        try
        {
            Console.WriteLine($"🔄 Updating watch progress: Movie {request.MovieId}, Position {request.CurrentPositionInSeconds}s");
            var response = await _httpClient.PostAsJsonAsync("api/watchprogress", request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<WatchProgressDto>();
                Console.WriteLine($"✅ Progress saved successfully: {result?.CurrentPositionInSeconds}s");
                return result;
            }
            else
            {
                Console.WriteLine($"❌ Failed to save progress. Status: {response.StatusCode}");
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving progress: {ex.Message}");
            return null;
        }
    }
    
    public async Task<WatchProgressDto?> GetWatchProgressAsync(int movieId)
    {
        try
        {
            Console.WriteLine($"🔄 Getting watch progress for movie {movieId}");
            var result = await _httpClient.GetFromJsonAsync<WatchProgressDto>($"api/watchprogress/movie/{movieId}");
            Console.WriteLine($"✅ Retrieved progress: {result?.CurrentPositionInSeconds}s");
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error getting progress: {ex.Message}");
            return null;
        }
    }
    
    public async Task<bool> DeleteWatchProgressAsync(int movieId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/watchprogress/movie/{movieId}");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
