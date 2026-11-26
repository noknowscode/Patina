using Patina.Core;
using Patina.Core.Model;
using Patina.Core.Queries;
using Patina.Core.Storage;
using Patina.Services;

namespace Patina.HotDesignPreviews;

/// <summary>The feed state a page preview forces.</summary>
public enum PreviewState { Data, Loading, Empty, Error }

/// <summary>
/// Preview-only document file under the real <see cref="PatinaStore"/>: the read stays pending (Loading), returns an
/// empty collection (Empty), fails like an unreadable file (Error), or starts with no file so the store seeds the
/// sample collection (Data). Writes stay in memory; a preview never touches the device's collection.
/// </summary>
internal sealed class PreviewDocumentFile(PreviewState state) : IDocumentFile
{
    private string? _json = state == PreviewState.Empty ? PatinaJson.Serialize(PatinaDocument.Empty) : null;

    public Task<string?> ReadAsync(CancellationToken ct) => state switch
    {
        PreviewState.Loading => Pending(ct),
        PreviewState.Error => Task.FromException<string?>(new IOException("Preview: the collection file is unavailable.")),
        _ => Task.FromResult(_json),
    };

    public Task WriteAsync(string json, CancellationToken ct)
    {
        _json = json;
        return Task.CompletedTask;
    }

    public Task MoveAsideAsync(string reason, CancellationToken ct) => Task.CompletedTask;

    private static async Task<string?> Pending(CancellationToken ct)
    {
        await Task.Delay(Timeout.Infinite, ct);
        throw new OperationCanceledException(ct);
    }
}

/// <summary>Surveyor and motion preferences held in memory, so previews leave LocalSettings alone.</summary>
internal sealed class PreviewAppSettings : IAppSettings
{
    public string Surveyor { get; set; } = "C. Lavoie";

    public bool ReduceMotion { get; set; }
}

/// <summary>No picker or camera in a preview: every request is a cancel.</summary>
internal sealed class PreviewPhotoService : IPhotoService
{
    public bool CanCapture => false;

    public Task<PhotoRef?> PickAsync(CancellationToken ct) => Task.FromResult<PhotoRef?>(null);

    public Task<PhotoRef?> CaptureAsync(CancellationToken ct) => Task.FromResult<PhotoRef?>(null);
}

/// <summary>Navigation in a preview goes nowhere: the page is not hosted in a region.</summary>
internal sealed class PreviewNavigator : INavigator
{
    public Route? Route => null;

    public Task<bool> CanNavigate(Route route) => Task.FromResult(false);

    public Task<NavigationResponse?> NavigateAsync(NavigationRequest request) => Task.FromResult<NavigationResponse?>(null);
}

/// <summary>Builds the services the generated view models take, for one preview state.</summary>
internal static class PreviewServices
{
    private static readonly IClock Clock = new SystemClock();

    public static PatinaStore Store(PreviewState state) => new(new PreviewDocumentFile(state), Clock);

    public static INavigator Navigator { get; } = new PreviewNavigator();

    private static PatinaDocument Sample => SampleCollection.Create(Clock.Today);

    /// <summary>The most urgent artwork of the sample collection: the navigation data for the artwork and survey pages.</summary>
    public static ArtworkSummary Artwork => PatinaQueries.Collection(Sample, Clock.Today, null, CollectionFilter.All).Items[0];

    /// <summary>The first open treatment of the sample collection: the navigation data for the treatment page.</summary>
    public static TreatmentSummary Treatment => PatinaQueries.Queue(Sample, Clock.Today, QueueFilter.Open)[0];
}
