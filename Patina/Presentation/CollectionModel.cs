using Patina.Core.Model;
using Patina.Core.Queries;
using Patina.Core.Storage;

namespace Patina.Presentation;

/// <summary>
/// The collection: every artwork, ordered by urgency, filtered by search and a filter chip.
/// Reads only; every write happens on the artwork, survey and treatment pages.
/// </summary>
public partial record CollectionModel(PatinaStore Store)
{
    /// <summary>
    /// The filter chips. Plain enum values, labelled by the EnumText converter: a record item does not survive the
    /// generated state proxy, so a chip bound to an <c>IState</c> of a record never changed the filter.
    /// </summary>
    public IImmutableList<CollectionFilter> Filters { get; } = Enum.GetValues<CollectionFilter>().ToImmutableList();

    public IState<string> Query => State.Value(this, () => string.Empty);

    public IState<CollectionFilter> Filter => State.Value(this, () => CollectionFilter.All);

    /// <summary>The artwork whose pin is selected on the map.</summary>
    public IState<string> SelectedId => State<string>.Empty(this);

    /// <summary>One instance per model, so a Retry on any FeedView reloads the source every feed here shares.</summary>
    private IFeed<PatinaDocument> Document { get; } = StoreFeed.Document(Store);

    public IFeed<CollectionView> View =>
        Feed.Combine(Document, Query, Filter)
            .Select(x => PatinaQueries.Collection(x.Item1, Store.Clock.Today, x.Item2, x.Item3));

    public IListFeed<ArtworkSummary> Artworks => View.Select(v => v.Items).AsListFeed();

    public IFeed<CollectionStats> Stats => View.Select(v => v.Stats);

    /// <summary>The selected pin's artwork, for the map callout. None when nothing is selected.</summary>
    public IFeed<ArtworkSummary> Selected =>
        Feed.Combine(View, SelectedId)
            .Select(x => x.Item1.Items.FirstOrDefault(a => a.Id == x.Item2)!);

    public async ValueTask ClearFilters(CancellationToken ct)
    {
        await Query.UpdateAsync(_ => string.Empty, ct);
        await Filter.UpdateAsync(_ => CollectionFilter.All, ct);
    }

    public async ValueTask ClearSelection(CancellationToken ct) => await SelectedId.UpdateAsync(_ => null!, ct);
}
