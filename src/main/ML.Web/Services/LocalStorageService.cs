using Microsoft.JSInterop;
using System.Text.Json;

namespace ML.Web.Services;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService(IJSRuntime jsRuntime) : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task<T?> GetItemAsync<T>(string key)
    {
        string? json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
            return default;

        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, JsonSerializer.Serialize(value));
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
    }
}