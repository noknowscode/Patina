using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.FileProviders;

namespace Patina.Blazor.Maui;

/// <summary>
/// A BlazorWebView that also serves the app data folder, so <c>img src="photos/{id}.jpg"</c> resolves to the
/// photo files the survey saved. The web head does the same with a static-files mapping; the page markup is
/// identical on both.
/// </summary>
public class PatinaWebView : BlazorWebView
{
    public override IFileProvider CreateFileProvider(string contentRootDir)
    {
        var data = FileSystem.AppDataDirectory;
        Directory.CreateDirectory(data);
        return new CompositeFileProvider(base.CreateFileProvider(contentRootDir), new PhysicalFileProvider(data));
    }
}
