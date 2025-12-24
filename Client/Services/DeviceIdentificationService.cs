using GiriMovies.Shared.DTOs;
using Microsoft.JSInterop;

namespace GiriMovies.Client.Services;

public interface IDeviceIdentificationService
{
    Task<DeviceInfoDto> GetDeviceInfoAsync();
    string GetStoredDeviceId();
    void StoreDeviceId(string deviceId);
    Task<DeviceInfoDto> CollectDeviceInfoAsync();
}

public class DeviceIdentificationService : IDeviceIdentificationService
{
    private readonly IJSRuntime _jsRuntime;
    private const string DEVICE_ID_KEY = "giri_device_id";
    private DeviceInfoDto? _cachedDeviceInfo;
    
    public DeviceIdentificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }
    
    public async Task<DeviceInfoDto> GetDeviceInfoAsync()
    {
        if (_cachedDeviceInfo != null)
            return _cachedDeviceInfo;
            
        _cachedDeviceInfo = await CollectDeviceInfoAsync();
        return _cachedDeviceInfo;
    }
    
    public string GetStoredDeviceId()
    {
        try
        {
            return _jsRuntime.InvokeAsync<string>("getStoredDeviceId").GetAwaiter().GetResult();
        }
        catch
        {
            return string.Empty;
        }
    }
    
    public void StoreDeviceId(string deviceId)
    {
        try
        {
            _jsRuntime.InvokeVoidAsync("storeDeviceId", deviceId);
        }
        catch
        {
            // Silently fail if JavaScript is not available
        }
    }
    
    public async Task<DeviceInfoDto> CollectDeviceInfoAsync()
    {
        try
        {
            // Use JavaScript to collect device information
            var deviceInfo = await _jsRuntime.InvokeAsync<DeviceInfoDto>("collectDeviceInfo");
            return deviceInfo ?? new DeviceInfoDto();
        }
        catch
        {
            // Fallback to basic information if JS fails
            return new DeviceInfoDto
            {
                DeviceName = "Unknown Device",
                TimeZone = TimeZoneInfo.Local.DisplayName,
                UserAgent = "Blazor Client",
                ComputerName = Environment.MachineName
            };
        }
    }
}