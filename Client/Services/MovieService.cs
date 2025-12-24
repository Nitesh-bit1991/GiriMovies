using System.Net.Http.Json;
using GiriMovies.Shared.DTOs;

namespace GiriMovies.Client.Services;

public interface IMovieService
{
    Task<List<MovieDto>> GetMoviesAsync();
    Task<MovieDto?> GetMovieAsync(int id);
    Task<List<MovieDto>> GetContinueWatchingAsync();
}

public class MovieService : IMovieService
{
    private readonly HttpClient _httpClient;
    
    public MovieService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<MovieDto>> GetMoviesAsync()
    {
        try
        {
            var movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies");
            return movies ?? new List<MovieDto>();
        }
        catch
        {
            return new List<MovieDto>();
        }
    }
    
    public async Task<MovieDto?> GetMovieAsync(int id)
    {
        try
        {
            Console.WriteLine($"🔄 MovieService: Fetching movie {id} from API");
            var movie = await _httpClient.GetFromJsonAsync<MovieDto>($"api/movies/{id}");
            
            if (movie != null)
            {
                Console.WriteLine($"✅ MovieService: Received movie from API:");
                Console.WriteLine($"  - ID: {movie.Id}");
                Console.WriteLine($"  - Title: {movie.Title}");
                Console.WriteLine($"  - CurrentPositionInSeconds: {movie.CurrentPositionInSeconds}");
                Console.WriteLine($"  - ProgressPercentage: {movie.ProgressPercentage}");
                Console.WriteLine($"  - LastWatchedAt: {movie.LastWatchedAt}");
                Console.WriteLine($"  - LastWatchedDevice: {movie.LastWatchedDevice}");
            }
            else
            {
                Console.WriteLine($"❌ MovieService: Received null movie from API");
            }
            
            return movie;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MovieService: Error fetching movie {id}: {ex.Message}");
            return null;
        }
    }
    
    public async Task<List<MovieDto>> GetContinueWatchingAsync()
    {
        try
        {
            var movies = await _httpClient.GetFromJsonAsync<List<MovieDto>>("api/movies/continue-watching");
            return movies ?? new List<MovieDto>();
        }
        catch
        {
            return new List<MovieDto>();
        }
    }
}
