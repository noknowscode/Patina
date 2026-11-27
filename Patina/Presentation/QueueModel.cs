using Patina.Core.Model;
using Patina.Core.Queries;
using Patina.Core.Storage;

namespace Patina.Presentation;

/// <summary>The treatment queue: late work first, then urgent, then by date.</summary>
public partial record QueueModel(PatinaStore Store)
{
    /// <summary>The filter chips as plain enum values (see <see cref="CollectionModel.Filters"/>).</summary>
    public IImmutableList<QueueFilter> Filters { get; } = Enum.GetValues<QueueFilter>().ToImmutableList();

    public IState<QueueFilter> Filter => State.Value(this, () => QueueFilter.Open);

    /// <summary>One instance per model, so a Retry on any FeedView reloads the source every feed here shares.</summary>
    private IFeed<PatinaDocument> Document { get; } = StoreFeed.Document(Store);

    public IListFeed<TreatmentSummary> Treatments =>
        Feed.Combine(Document, Filter)
            .Select(x => PatinaQueries.Queue(x.Item1, Store.Clock.Today, x.Item2))
            .AsListFeed();

    public IFeed<QueueStats> Stats => Document.Select(doc => PatinaQueries.QueueStatistics(doc, Store.Clock.Today));

    public async ValueTask ShowOpen(CancellationToken ct) => await Filter.UpdateAsync(_ => QueueFilter.Open, ct);
}
