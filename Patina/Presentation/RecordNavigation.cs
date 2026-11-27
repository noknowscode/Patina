using Patina.Core.Queries;

namespace Patina.Presentation;

/// <summary>
/// Moving between an artwork record and its treatments, which link to each other. Navigation Extensions mishandles a
/// forward navigation to the route the previous page was opened with (the frame ends on a page without its data), so:
/// the record the previous page already shows is reached by going back, and a different record of the same kind
/// alternates between two route names for the same view.
/// </summary>
internal static class RecordNavigation
{
    public static Task OpenArtworkAsync(this INavigator navigator, object sender, ArtworkSummary artwork, CancellationToken ct) =>
        OpenAsync(navigator, sender, artwork, a => a.Id, "Artwork", "ArtworkRecord", ct);

    public static Task OpenTreatmentAsync(this INavigator navigator, object sender, TreatmentSummary treatment, CancellationToken ct) =>
        OpenAsync(navigator, sender, treatment, t => t.Id, "Treatment", "TreatmentRecord", ct);

    private static async Task OpenAsync<TData>(
        INavigator navigator, object sender, TData data, Func<TData, string> key, string route, string alias, CancellationToken ct)
        where TData : class
    {
        var backStack = (navigator as FrameNavigator)?.Control?.BackStack;
        if (backStack is [.., var previous] && previous.Parameter is TData shown && key(shown) == key(data))
        {
            await navigator.NavigateBackAsync(sender, cancellation: ct);
            return;
        }

        // Pages of this kind in the stack alternate route names, so the previous one never shares the new page's name.
        var sameKind = backStack?.Count(entry => entry.Parameter is TData) ?? 0;
        await navigator.NavigateRouteAsync(sender, sameKind % 2 == 0 ? route : alias, data: data, cancellation: ct);
    }
}
