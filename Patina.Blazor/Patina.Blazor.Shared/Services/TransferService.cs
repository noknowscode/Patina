using System.IO.Compression;
using System.Text;
using Patina.Core;
using Patina.Core.Model;
using Patina.Core.Rules;
using Patina.Core.Storage;

namespace Patina.Blazor.Shared.Services;

/// <summary>Hands a file the app produced to the user: a download in the browser, a share sheet on MAUI.</summary>
public interface IFileSaver
{
    Task<bool> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken ct);
}

public sealed record ImportPreview(PatinaDocument Document, string FileName, byte[] Archive);

/// <summary>
/// The <c>.patina</c> archive: a zip with <c>collection.json</c> (an <see cref="ExportEnvelope"/>) and <c>photos/</c>.
/// Same format as the Uno app's <c>DataTransferService</c>, so a collection moves between the two apps. Picking and
/// saving files is the host's job (<see cref="IFileSaver"/>, <c>InputFile</c>); this class only builds and reads bytes.
/// </summary>
public sealed class TransferService(IPhotoStore photos, IClock clock)
{
    public const string Extension = ".patina";
    private const string DocumentEntry = "collection.json";
    private const string PhotosPrefix = "photos/";

    public string SuggestedFileName => $"patina-{clock.Today:yyyy-MM-dd}{Extension}";

    public async Task<byte[]> ExportAsync(PatinaDocument doc, CancellationToken ct)
    {
        using var stream = new MemoryStream();
        using (var zip = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            var entry = zip.CreateEntry(DocumentEntry, CompressionLevel.Optimal);
            await using (var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false)))
            {
                await writer.WriteAsync(PatinaJson.Export(doc, clock.Now));
            }

            foreach (var photo in doc.Photos)
            {
                var path = photos.PathFor(photo);
                if (File.Exists(path))
                {
                    zip.CreateEntryFromFile(path, PhotosPrefix + photo.FileName, CompressionLevel.NoCompression);
                }
            }
        }

        return stream.ToArray();
    }

    /// <summary>Reads and validates an archive without applying it.</summary>
    public Result<ImportPreview> Preview(byte[] archive, string fileName)
    {
        try
        {
            using var zip = new ZipArchive(new MemoryStream(archive), ZipArchiveMode.Read);
            var entry = zip.GetEntry(DocumentEntry);
            if (entry is null)
            {
                return Result<ImportPreview>.Fail(Problem.ImportNotPatinaFile);
            }

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            var result = PatinaJson.Import(reader.ReadToEnd());
            return result.IsOk
                ? Result<ImportPreview>.Ok(new ImportPreview(result.Value!, fileName, archive))
                : Result<ImportPreview>.Fail(result.Problem);
        }
        catch (InvalidDataException)
        {
            return Result<ImportPreview>.Fail(Problem.ImportNotPatinaFile);
        }
    }

    /// <summary>Copies the archive's photos into the photo folder; call after the document has been replaced.</summary>
    public async Task RestorePhotosAsync(ImportPreview preview, CancellationToken ct)
    {
        Directory.CreateDirectory(photos.Folder);
        using var zip = new ZipArchive(new MemoryStream(preview.Archive), ZipArchiveMode.Read);
        foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith(PhotosPrefix, StringComparison.Ordinal)))
        {
            var name = Path.GetFileName(entry.FullName);
            if (string.IsNullOrEmpty(name))
            {
                continue;
            }

            await using var source = entry.Open();
            await using var target = File.Create(Path.Combine(photos.Folder, name));
            await source.CopyToAsync(target, ct);
        }
    }
}
