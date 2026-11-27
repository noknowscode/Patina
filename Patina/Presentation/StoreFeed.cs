using System.Runtime.CompilerServices;
using Patina.Core.Model;
using Patina.Core.Storage;

namespace Patina.Presentation;

/// <summary>
/// The document as an MVUX feed that both follows the store and can be refreshed. A feed over
/// <see cref="PatinaStore.Watch"/> (<c>Feed.AsyncEnumerable</c>) ignores a FeedView's <c>Refresh</c>, so Try again
/// after a load error never reloaded; <c>Feed.Async</c> honours it, and the store's <see cref="PatinaStore.Changed"/>
/// event raises the same refresh signal after every committed change. A failed load caches nothing, so a refresh
/// reads the file again.
/// </summary>
internal static class StoreFeed
{
    private static readonly ConditionalWeakTable<PatinaStore, Signal> Signals = new();

    public static IFeed<PatinaDocument> Document(PatinaStore store) =>
        Feed.Async(async ct => await store.GetAsync(ct), Signals.GetValue(store, Track));

    private static Signal Track(PatinaStore store)
    {
        var signal = new Signal();
        store.Changed += signal.Raise;
        return signal;
    }
}
