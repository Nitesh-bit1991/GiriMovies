using Microsoft.JSInterop;

namespace GiriMovies.Client.Services;

public interface IDeviceDetectionService
{
    Task<string> GetDeviceTypeAsync();
}

public class DeviceDetectionService : IDeviceDetectionService
{
    private readonly IJSRuntime _jsRuntime;
    
    public DeviceDetectionService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task<string> GetDeviceTypeAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("getDeviceType");
        }
        catch
        {
            return "Computer"; // Default fallback
        }
    }
}
