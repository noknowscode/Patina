using Patina.Core.Model;
using Patina.Core.Queries;
using Patina.Core.Rules;
using Patina.Core.Storage;
using Patina.Services;

namespace Patina.Presentation;

/// <summary>One step of the Proposed → Scheduled → In progress → Done stepper.</summary>
public sealed record StepView(int Number, string Label, bool IsDone, bool IsCurrent)
{
    public bool IsUpcoming => !IsDone && !IsCurrent;
}

/// <summary>The treatment's editable schedule, edited two-way through the generated bindable proxy.</summary>
public partial record TreatmentFields(DateTimeOffset? ScheduledFor, string Assignee);

/// <summary>One treatment: where it came from, its schedule and assignee, its log, and moving it forward.</summary>
public partial record TreatmentModel(TreatmentSummary Summary, PatinaStore Store, IAppSettings Settings, INavigator Navigator)
{
    /// <summary>One instance per model, so a Retry on any FeedView reloads the source every feed here shares.</summary>
    private IFeed<PatinaDocument> Document { get; } = StoreFeed.Document(Store);

    public IFeed<TreatmentDetail> Detail => Document.Select(doc => PatinaQueries.Treatment(doc, Summary.Id, Store.Clock.Today)!);

    public IListFeed<StepView> Steps => Detail.Select(d => Stepper(d.Treatment.Status)).AsListFeed();

    public IFeed<TreatmentStatus> Status => Detail.Select(d => d.Treatment.Status);

    public IFeed<bool> IsEditable => Detail.Select(d => d.Treatment.Status != TreatmentStatus.Done);

    public IFeed<bool> CanStepBack => Detail.Select(d => TreatmentWorkflow.Previous(d.Treatment.Status) is not null);

    /// <summary>
    /// The editable fields follow the stored treatment, so they load with the page's Retry and show what was saved
    /// after Save, Advance or Step back.
    /// </summary>
    public IState<TreatmentFields> Fields => State.FromFeed(this, Detail.Select(d => new TreatmentFields(
        d.Treatment.ScheduledFor is { } date ? new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue)) : null,
        d.Treatment.Assignee ?? string.Empty)));

    public IState<string> Note => State.Value(this, () => string.Empty);

    public IState<string> Message => State.Value(this, () => string.Empty);

    public async ValueTask Save(CancellationToken ct)
    {
        var (date, assignee, note) = await ReadFieldsAsync();
        var problem = await Store.UpdateAsync(doc => TreatmentWorkflow.Save(doc, Summary.Id, date, assignee, note, Author, Store.Clock.Now), ct);
        await AfterWrite(problem, Text.Get("Treatment_Saved"), ct);
    }

    public async ValueTask Advance(CancellationToken ct)
    {
        var (date, assignee, note) = await ReadFieldsAsync();
        var problem = await Store.UpdateAsync(doc => TreatmentWorkflow.Advance(doc, Summary.Id, date, assignee, note, Author, Store.Clock.Now), ct);
        await AfterWrite(problem, string.Empty, ct);
    }

    public async ValueTask StepBack(CancellationToken ct)
    {
        var problem = await Store.UpdateAsync(doc => TreatmentWorkflow.StepBack(doc, Summary.Id, Author, Store.Clock.Now), ct);
        await AfterWrite(problem, string.Empty, ct);
    }

    public async ValueTask OpenArtwork(TreatmentDetail detail, CancellationToken ct) =>
        await Navigator.OpenArtworkAsync(this, detail.ArtworkSummary, ct);

    private string Author => Settings.Surveyor is { Length: > 0 } name ? name : Text.Get("Common_UnnamedSurveyor");

    private async Task<(DateOnly? Date, string Assignee, string Note)> ReadFieldsAsync()
    {
        var fields = await Fields;
        return (fields?.ScheduledFor is { } d ? DateOnly.FromDateTime(d.Date) : null, fields?.Assignee ?? string.Empty, await Note ?? string.Empty);
    }

    private async ValueTask AfterWrite(Problem problem, string success, CancellationToken ct)
    {
        if (problem == Problem.None)
        {
            await Note.UpdateAsync(_ => string.Empty, ct);
        }

        await Message.UpdateAsync(_ => problem == Problem.None ? success : Text.Problem(problem), ct);
    }

    private static IImmutableList<StepView> Stepper(TreatmentStatus status) =>
        Enum.GetValues<TreatmentStatus>()
            .Select(s => new StepView((int)s + 1, Text.Enum(s), s < status || status == TreatmentStatus.Done, s == status && s != TreatmentStatus.Done))
            .ToImmutableList();
}
