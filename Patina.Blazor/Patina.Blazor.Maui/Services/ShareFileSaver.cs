using Patina.Blazor.Shared.Services;

namespace Patina.Blazor.Maui.Services;

/// <summary>
/// Export on MAUI: write the archive to the cache folder and hand it to the platform share sheet, which offers
/// "Save to files" on mobile and the usual targets on desktop. MAUI Essentials ships no save-file dialog; the
/// CommunityToolkit FileSaver would add one at the cost of a dependency (do-not-overbuild, see COMPARISON.md).
/// </summary>
public sealed class ShareFileSaver : IFileSaver
{
    public async Task<bool> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken ct)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(path, content, ct);
        await MainThread.InvokeOnMainThreadAsync(() => Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = fileName,
            File = new ShareFile(path, contentType),
        }));
        return true;
    }
}
