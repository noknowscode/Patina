using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Patina.Core;
using Patina.Core.Storage;

namespace Patina.Blazor.Shared.Services;

/// <summary>
/// The service graph both hosts share. A host registers its own <see cref="IAppSettings"/>, <see cref="IPhotoCapture"/>
/// and <see cref="IFileSaver"/> before calling this (TryAdd keeps the host's choice); everything else matches the
/// Uno app's composition in <c>App.xaml.cs</c>.
/// </summary>
public static class PatinaServices
{
    public static IServiceCollection AddPatinaCore(this IServiceCollection services, string dataFolder)
    {
        services.AddLocalization();
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<IDocumentFile>(_ => new FileSystemDocumentFile(dataFolder));
        services.TryAddSingleton<PatinaStore>();
        services.TryAddSingleton<IAppSettings>(_ => new JsonFileAppSettings(dataFolder));
        services.TryAddSingleton<IPhotoStore>(sp => new FileSystemPhotoStore(dataFolder, sp.GetRequiredService<IClock>()));
        services.TryAddSingleton<IPhotoCapture, NoPhotoCapture>();
        services.TryAddSingleton<TransferService>();
        services.TryAddSingleton<Text>();
        return services;
    }
}
