using Microsoft.JSInterop;
using Patina.Blazor.Shared.Services;

namespace Patina.Blazor.Web.Services;

/// <summary>Export in the browser: the bytes stream over the circuit and the page triggers a download. Scoped: IJSRuntime is per circuit.</summary>
public sealed class DownloadFileSaver(IJSRuntime js) : IFileSaver, IAsyncDisposable
{
    private IJSObjectReference? _module;

    public async Task<bool> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken ct)
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", ct, "./_content/Patina.Blazor.Shared/js/patina.js");
        using var stream = new MemoryStream(content);
        using var reference = new DotNetStreamReference(stream, leaveOpen: true);
        await _module.InvokeVoidAsync("downloadFile", ct, fileName, contentType, reference);
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
            }
        }
    }
}
