using System.Text.Json;
using Microsoft.JSInterop;

namespace UrenRegistratie.Services;

/// <summary>
/// Slaat data op in de localStorage van de browser. Dit werkt volledig offline:
/// er is geen server of internetverbinding voor nodig. De data blijft bewaard
/// zolang de gebruiker de site-data van de app niet handmatig wist.
/// </summary>
public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<List<T>> GetListAsync<T>(string key)
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", key);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<T>();
        }

        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public async Task SetListAsync<T>(string key, List<T> items)
    {
        var json = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public async Task SetStringAsync(string key, string value)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", key, value);
    }

    public async Task RemoveAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
