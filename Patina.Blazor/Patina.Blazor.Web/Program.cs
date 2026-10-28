using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.FileProviders;
using Patina.Blazor.Shared.Services;
using Patina.Blazor.Web.Components;
using Patina.Blazor.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// One collection per server, in Patina:DataFolder (default App_Data). The web head has no per-device store, so
// "local-first" here means local to the server; settings and photos share the same folder.
var dataFolder = builder.Configuration["Patina:DataFolder"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "App_Data");
builder.Services.AddScoped<IFileSaver, DownloadFileSaver>();
builder.Services.AddPatinaCore(dataFolder);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Language: the setting decides the culture of every circuit, applied on the next page load (the Uno app
// applies it on restart). A custom provider keeps the single-server storage model: no cookies, no query string.
var cultures = new[] { new CultureInfo("en"), new CultureInfo("fr") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = cultures,
    SupportedUICultures = cultures,
    RequestCultureProviders = [new SettingsCultureProvider(app.Services.GetRequiredService<IAppSettings>())],
});

app.UseAntiforgery();

app.MapStaticAssets();

// Photos: the data folder's photos/ is served at /photos, the same path the MAUI head's file provider answers.
var photoFolder = app.Services.GetRequiredService<IPhotoStore>().Folder;
Directory.CreateDirectory(photoFolder);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(photoFolder),
    RequestPath = "/photos",
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(Patina.Blazor.Shared._Imports).Assembly);

app.Run();

/// <summary>Resolves the request culture from the app setting instead of the browser.</summary>
file sealed class SettingsCultureProvider(IAppSettings settings) : IRequestCultureProvider
{
    public Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext) =>
        Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(settings.Language));
}
