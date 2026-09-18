using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using UrenRegistratie;
using UrenRegistratie.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Onze eigen services voor opslag en tijdregistratie-logica
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<TimeTrackingService>();
builder.Services.AddScoped<WerkbonService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PinLockService>();

await builder.Build().RunAsync();
