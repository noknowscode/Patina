using Patina.Core.Storage;

namespace Patina.Blazor.Shared.Services;

/// <summary>
/// Keeps <c>patina.json</c> in a folder the host chooses (the app data directory on MAUI, a data root on the
/// server). Writes go to a temporary file first and then replace the document, so an interrupted write never
/// leaves a half-written collection behind. Mirrors the Uno app's <c>LocalFolderDocumentFile</c>.
/// </summary>
public sealed class FileSystemDocumentFile(string folder) : IDocumentFile
{
    private const string FileName = "patina.json";

    public string Folder { get; } = folder;

    public string DocumentPath => Path.Combine(Folder, FileName);

    public async Task<string?> ReadAsync(CancellationToken ct) =>
        File.Exists(DocumentPath) ? await File.ReadAllTextAsync(DocumentPath, ct) : null;

    public async Task WriteAsync(string json, CancellationToken ct)
    {
        Directory.CreateDirectory(Folder);
        var temp = DocumentPath + ".tmp";
        await File.WriteAllTextAsync(temp, json, ct);
        File.Move(temp, DocumentPath, overwrite: true);
    }

    public Task MoveAsideAsync(string reason, CancellationToken ct)
    {
        if (File.Exists(DocumentPath))
        {
            File.Move(DocumentPath, Path.Combine(Folder, $"patina.{reason}-{DateTime.Now:yyyyMMdd-HHmmss}.json"), overwrite: true);
        }

        return Task.CompletedTask;
    }
}
