using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Patina.Blazor.Shared.Components;
using Patina.Blazor.Shared.Services;
using Patina.Core;
using Patina.Core.Model;
using Patina.Core.Rules;
using Patina.Core.Storage;

namespace Patina.Blazor.Shared.Pages;

/// <summary>
/// The survey page's state and commands: the port of the Uno <c>SurveyModel</c>. Edits live in fields; findings
/// and photos save the draft at once; Submit grades it and proposes treatments. This page is a writer, so it reads
/// the store once instead of watching it.
/// </summary>
public partial class SurveyPage : ComponentBase
{
    [Parameter] public string ArtworkId { get; set; } = "";

    [Inject] private PatinaStore Store { get; set; } = default!;
    [Inject] private IAppSettings Settings { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private Text T { get; set; } = default!;

    private ConfirmDialog? _confirm;

    private Artwork? _artwork;
    private bool _gone;
    private string? _error;

    private string? _draftId;
    private DateOnly? _draftDate;
    private readonly string _newSurveyId = Ids.New();

    private string _surveyor = "";
    private Weather _weather = Weather.Dry;
    private string _notes = "";
    private List<PhotoRef> _surveyPhotos = [];
    private List<Finding> _findings = [];
    private IReadOnlyList<FindingType> _findingTypes = [];
    private ConditionGrade _liveGrade = ConditionGrade.Good;
    private string _message = "";

    private string _newType = "";
    private string _newZone = "";
    private Severity _newSeverity = Severity.Minor;
    private string _newNote = "";
    private List<PhotoRef> _findingPhotos = [];

    private string SurveyId => _draftId ?? _newSurveyId;

    protected override Task OnInitializedAsync() => LoadAsync();

    private async Task LoadAsync()
    {
        _error = null;
        PatinaDocument doc;
        try
        {
            doc = await Store.GetAsync();
        }
        catch (PatinaStoreException ex)
        {
            _error = ex.Message;
            return;
        }

        _artwork = doc.Artworks.FirstOrDefault(a => a.Id == ArtworkId);
        if (_artwork is null)
        {
            _gone = true;
            return;
        }

        _findingTypes = ConditionRules.FindingTypesFor(_artwork.Material);

        var draft = SurveyOps.DraftFor(doc, ArtworkId);
        _draftId = draft?.Id;
        _draftDate = draft?.Date;
        _surveyor = draft?.Surveyor is { Length: > 0 } s ? s : Settings.Surveyor;
        _weather = draft?.Weather ?? Weather.Dry;
        _notes = draft?.Notes ?? "";
        _findings = draft?.Findings.ToList() ?? [];
        var ids = draft?.PhotoIds ?? [];
        _surveyPhotos = doc.Photos.Where(p => ids.Contains(p.Id)).ToList();
        _liveGrade = ConditionRules.Grade(_findings);
    }

    private void SetMessage(string message) => _message = message;

    // ---- findings ----

    private async Task AddFindingAsync()
    {
        if (!Enum.TryParse<FindingType>(_newType, out var type) || !_findingTypes.Contains(type))
        {
            _message = T["Survey_FindingTypeRequired"];
            return;
        }

        if (string.IsNullOrWhiteSpace(_newZone))
        {
            _message = T.Problem(Problem.FindingZoneRequired);
            return;
        }

        _findings.Add(new Finding(Ids.New(), type, _newZone.Trim(), _newSeverity, _newNote.Trim(), _findingPhotos.Select(p => p.Id).ToImmutableList()));
        _newType = "";
        _newZone = "";
        _newSeverity = Severity.Minor;
        _newNote = "";
        _findingPhotos = [];
        await AfterFindingsChangedAsync();
    }

    private async Task RemoveFindingAsync(Finding finding)
    {
        _findings.RemoveAll(f => f.Id == finding.Id);
        await AfterFindingsChangedAsync();
    }

    private async Task AfterFindingsChangedAsync()
    {
        _liveGrade = ConditionRules.Grade(_findings);
        await SaveAsync();
        _message = "";
    }

    // ---- photos ----

    private async Task AddSurveyPhotoAsync(PhotoRef photo)
    {
        _surveyPhotos.Add(photo);
        await SaveAsync();
    }

    private async Task RemoveSurveyPhotoAsync(PhotoRef photo)
    {
        _surveyPhotos.RemoveAll(p => p.Id == photo.Id);
        await SaveAsync();
    }

    private void AddFindingPhoto(PhotoRef photo) => _findingPhotos.Add(photo);

    private void RemoveFindingPhoto(PhotoRef photo) => _findingPhotos.RemoveAll(p => p.Id == photo.Id);

    // ---- commands ----

    private async Task SaveDraftAsync()
    {
        var problem = await SaveAsync();
        _message = problem == Problem.None ? T["Survey_DraftSaved"] : T.Problem(problem);
    }

    private async Task SubmitAsync()
    {
        var survey = Build();
        var photos = AllPhotos();
        var problem = await Store.UpdateAsync(doc => SurveyOps.Submit(WithPhotos(doc, photos), survey, Store.Clock.Now, T.TreatmentTitle));
        if (problem != Problem.None)
        {
            _message = T.Problem(problem);
            return;
        }

        Settings.Surveyor = survey.Surveyor;
        Nav.NavigateTo($"artwork/{ArtworkId}");
    }

    private async Task DiscardAsync()
    {
        if (_confirm is null || !await _confirm.AskAsync(T["Survey_DiscardTitle"], T["Survey_DiscardBody"], T["Survey_DiscardConfirm"]))
        {
            return;
        }

        await Store.UpdateAsync(doc => SurveyOps.DiscardDraft(doc, SurveyId));
        Nav.NavigateTo($"artwork/{ArtworkId}");
    }

    /// <summary>Back from the bar: keep whatever was entered as a draft, then leave.</summary>
    private async Task CloseAsync()
    {
        if (_artwork is not null && HasContent())
        {
            await SaveAsync();
        }

        Nav.NavigateTo($"artwork/{ArtworkId}");
    }

    // ---- building and saving ----

    private Survey Build() => new(
        SurveyId,
        ArtworkId,
        SurveyStatus.Draft,
        _draftDate ?? Store.Clock.Today,
        _surveyor.Trim(),
        _weather,
        _notes.Trim(),
        _findings.ToImmutableList(),
        _surveyPhotos.Select(p => p.Id).ToImmutableList(),
        ConditionRules.Grade(_findings));

    private bool HasContent()
    {
        var survey = Build();
        return _draftId is not null || survey.Findings.Count > 0 || survey.PhotoIds.Count > 0 || survey.Notes.Length > 0;
    }

    private ImmutableList<PhotoRef> AllPhotos() => [.. _surveyPhotos, .. _findingPhotos];

    private static PatinaDocument WithPhotos(PatinaDocument doc, IEnumerable<PhotoRef> photos)
    {
        var known = doc.Photos.Select(p => p.Id).ToHashSet();
        var added = photos.Where(p => known.Add(p.Id)).ToList();
        return added.Count == 0 ? doc : doc with { Photos = doc.Photos.AddRange(added) };
    }

    private async Task<Problem> SaveAsync()
    {
        var survey = Build();
        var photos = AllPhotos();
        var problem = await Store.UpdateAsync(doc => SurveyOps.SaveDraft(WithPhotos(doc, photos), survey));
        if (problem == Problem.None)
        {
            _draftId = survey.Id;
            _draftDate = survey.Date;
        }

        return problem;
    }
}
