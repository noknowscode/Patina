using Microsoft.AspNetCore.Components;
using Patina.Blazor.Shared.Services;
using Patina.Core;
using Patina.Core.Model;
using Patina.Core.Storage;

namespace Patina.Blazor.Shared.Components;

/// <summary>
/// Base for every page that reads the collection. Loads the document once (so prerendering already shows data),
/// then follows <see cref="PatinaStore.Watch"/> for every commit and re-renders. This is what MVUX gives the Uno
/// app for free with <c>Feed.AsyncEnumerable(Store.Watch)</c>; here it is one base class.
/// </summary>
public abstract class StoreComponent : ComponentBase, IDisposable
{
    private CancellationTokenSource? _watch;

    [Inject] protected PatinaStore Store { get; set; } = default!;
    [Inject] protected IClock Clock { get; set; } = default!;
    [Inject] protected Text T { get; set; } = default!;

    /// <summary>The current document; null until the first read completes or when the last read failed.</summary>
    protected PatinaDocument? Document { get; private set; }

    protected string? Error { get; private set; }

    protected bool Loading => Document is null && Error is null;

    protected DateOnly Today => Clock.Today;

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    /// <summary>Retry after a load error: forget the cached document and read the file again.</summary>
    protected async Task RetryAsync()
    {
        Store.Invalidate();
        Error = null;
        Document = null;
        await LoadAsync();
    }

    /// <summary>Override to recompute derived view state; runs after every document change, before rendering.</summary>
    protected virtual void OnDocument(PatinaDocument doc)
    {
    }

    private async Task LoadAsync()
    {
        try
        {
            Apply(await Store.GetAsync());
        }
        catch (PatinaStoreException ex)
        {
            Error = ex.Message;
            return;
        }

        StartWatch();
    }

    private void StartWatch()
    {
        _watch?.Cancel();
        _watch = new CancellationTokenSource();
        _ = WatchAsync(_watch.Token);
    }

    private async Task WatchAsync(CancellationToken ct)
    {
        try
        {
            await foreach (var doc in Store.Watch(ct))
            {
                if (ReferenceEquals(doc, Document))
                {
                    continue;
                }

                await InvokeAsync(() =>
                {
                    Apply(doc);
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (PatinaStoreException ex)
        {
            await InvokeAsync(() =>
            {
                Error = ex.Message;
                StateHasChanged();
            });
        }
    }

    private void Apply(PatinaDocument doc)
    {
        Document = doc;
        Error = null;
        OnDocument(doc);
    }

    public virtual void Dispose()
    {
        _watch?.Cancel();
        _watch?.Dispose();
        _watch = null;
        GC.SuppressFinalize(this);
    }
}
