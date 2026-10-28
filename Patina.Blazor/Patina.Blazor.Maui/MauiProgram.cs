using System.Globalization;
using Microsoft.Extensions.Logging;
using Patina.Blazor.Maui.Services;
using Patina.Blazor.Shared.Services;

namespace Patina.Blazor.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();

        // Host-owned seams first so AddPatinaCore's TryAdd keeps them. The collection lives in the app data
        // folder, the same place the Uno app's ApplicationData.LocalFolder resolves to.
        builder.Services.AddSingleton<IAppSettings, PreferencesAppSettings>();
        builder.Services.AddSingleton<IPhotoCapture, MauiPhotoCapture>();
        builder.Services.AddSingleton<IFileSaver, ShareFileSaver>();
        builder.Services.AddPatinaCore(FileSystem.AppDataDirectory);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Language applies at startup, like the Uno app's ILocalizationService (switch takes effect on restart).
        var culture = new CultureInfo(app.Services.GetRequiredService<IAppSettings>().Language);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        return app;
    }
}
