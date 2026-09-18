using System.Security.Cryptography;
using System.Text;

namespace UrenRegistratie.Services;

/// <summary>
/// Regelt de snelle pincode-ontgrendeling op dit toestel, bovenop de echte
/// Firebase-login. De pincode wordt alleen lokaal (gehasht) bewaard en vervangt
/// de echte login niet - die blijft nodig bij een nieuw toestel of na uitloggen.
/// </summary>
public class PinLockService
{
    private const string PinKey = "app_pin_hash";
    private readonly LocalStorageService _storage;

    public bool IsUnlockedThisSession { get; private set; }

    public PinLockService(LocalStorageService storage)
    {
        _storage = storage;
    }

    public async Task<bool> HasPinSetAsync()
    {
        var hash = await _storage.GetStringAsync(PinKey);
        return !string.IsNullOrEmpty(hash);
    }

    public async Task SetPinAsync(string pin)
    {
        await _storage.SetStringAsync(PinKey, Hash(pin));
        IsUnlockedThisSession = true;
    }

    public async Task<bool> TryUnlockAsync(string pin)
    {
        var stored = await _storage.GetStringAsync(PinKey);
        if (string.IsNullOrEmpty(stored)) return false;

        var match = stored == Hash(pin);
        if (match) IsUnlockedThisSession = true;
        return match;
    }

    public async Task ClearPinAsync()
    {
        await _storage.RemoveAsync(PinKey);
        IsUnlockedThisSession = false;
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
