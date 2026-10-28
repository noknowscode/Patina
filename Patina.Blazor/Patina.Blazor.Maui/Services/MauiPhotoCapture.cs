using Patina.Blazor.Shared.Services;
using Patina.Core.Model;

namespace Patina.Blazor.Maui.Services;

/// <summary>
/// Camera capture through MAUI's <see cref="MediaPicker"/>. The counterpart of Uno's <c>CameraCaptureUI</c>, with
/// one difference worth logging: MediaPicker captures on Windows too, where the Uno app falls back to a file picker.
/// </summary>
public sealed class MauiPhotoCapture(IPhotoStore store) : IPhotoCapture
{
    public bool CanCapture => MediaPicker.Default.IsCaptureSupported;

    public async Task<PhotoRef?> CaptureAsync(CancellationToken ct)
    {
        if (!CanCapture)
        {
            return null;
        }

        // The picker needs the UI thread; Blazor event handlers run on the circuit's dispatcher, which is the UI
        // thread in a BlazorWebView, but a Task continuation may not be. Marshal explicitly, as the Uno app does.
        var result = await MainThread.InvokeOnMainThreadAsync(() => MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Patina" }));
        if (result is null)
        {
            return null;
        }

        await using var source = await result.OpenReadAsync();
        return await store.SaveAsync(source, result.FileName, ct);
    }
}
