using Patina.Core;
using Patina.Core.Model;

namespace Patina.Blazor.Shared.Services;

/// <summary>
/// Where photo files live and how the page reaches them. Picking happens in the page through <c>InputFile</c>
/// on both hosts (a file input opens the native picker inside a BlazorWebView too); only camera capture needs
/// a host service (<see cref="IPhotoCapture"/>).
/// </summary>
public interface IPhotoStore
{
    string Folder { get; }

    /// <summary>The URL the page uses in an <c>img</c>; each host serves <see cref="Folder"/> at <c>photos/</c>.</summary>
    string Url(PhotoRef photo);

    string PathFor(PhotoRef photo);

    /// <summary>Copies the stream into the photo folder under a new id so the survey keeps its photos even when the original moves.</summary>
    Task<PhotoRef> SaveAsync(Stream source, string originalName, CancellationToken ct);
}

/// <summary>Camera capture where the platform has one (MAUI <c>MediaPicker</c>); the web head uses <c>InputFile capture</c> instead.</summary>
public interface IPhotoCapture
{
    bool CanCapture { get; }

    /// <summary>Opens the camera and stores the photo; null on cancel or where capture is not available.</summary>
    Task<PhotoRef?> CaptureAsync(CancellationToken ct);
}

public sealed class FileSystemPhotoStore(string dataFolder, IClock clock) : IPhotoStore
{
    private static readonly HashSet<string> ImageTypes = [".jpg", ".jpeg", ".png", ".webp", ".heic"];

    public string Folder { get; } = Path.Combine(dataFolder, "photos");

    public string Url(PhotoRef photo) => "photos/" + photo.FileName;

    public string PathFor(PhotoRef photo) => Path.Combine(Folder, photo.FileName);

    public async Task<PhotoRef> SaveAsync(Stream source, string originalName, CancellationToken ct)
    {
        var extension = Path.GetExtension(originalName).ToLowerInvariant();
        if (!ImageTypes.Contains(extension))
        {
            extension = ".jpg";
        }

        var photo = new PhotoRef(Ids.New(), extension, Path.GetFileName(originalName), clock.Now);
        Directory.CreateDirectory(Folder);
        await using var target = File.Create(PathFor(photo));
        await source.CopyToAsync(target, ct);
        return photo;
    }
}

public sealed class NoPhotoCapture : IPhotoCapture
{
    public bool CanCapture => false;

    public Task<PhotoRef?> CaptureAsync(CancellationToken ct) => Task.FromResult<PhotoRef?>(null);
}
