using Microsoft.JSInterop;
using UrenRegistratie.Models;

namespace UrenRegistratie.Services;

/// <summary>
/// Regelt inloggen/uitloggen via Firebase Authentication. Bepaalt ook of de
/// ingelogde gebruiker admin-rechten heeft (op basis van een vaste lijst e-mailadressen -
/// dit vermijdt de noodzaak van een betaalde Cloud Function om rollen te beheren).
/// </summary>
public class AuthService
{
    private static readonly HashSet<string> AdminEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "Y.Wolda@outlook.com"
    };

    private readonly IJSRuntime _js;
    private DotNetObjectReference<AuthService>? _dotNetRef;
    private bool _initialized;

    public AppUser? CurrentUser { get; private set; }
    public bool IsAuthReady { get; private set; }

    public event Action? OnChange;

    public AuthService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        _initialized = true;

        _dotNetRef = DotNetObjectReference.Create(this);
        await _js.InvokeVoidAsync("firebaseAuth.registerCallback", _dotNetRef);
    }

    [JSInvokable]
    public Task OnAuthStateChanged(string? uid, string? email)
    {
        CurrentUser = uid is null
            ? null
            : new AppUser
            {
                Uid = uid,
                Email = email ?? string.Empty,
                IsAdmin = email is not null && AdminEmails.Contains(email)
            };

        IsAuthReady = true;
        OnChange?.Invoke();
        return Task.CompletedTask;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        var result = await _js.InvokeAsync<LoginResult>("firebaseAuth.login", email, password);
        return (result.Success, result.Message);
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("firebaseAuth.logout");
    }

    private class LoginResult
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
