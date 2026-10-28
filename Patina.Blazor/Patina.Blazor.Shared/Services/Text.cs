using System.Globalization;
using Microsoft.Extensions.Localization;
using Patina.Blazor.Shared.Resources;
using Patina.Core.Model;
using Patina.Core.Queries;
using Patina.Core.Rules;

namespace Patina.Blazor.Shared.Services;

/// <summary>
/// Localized display text for domain values: the same rules the Uno app keeps in <c>Services/Text.cs</c> and
/// <c>Converters/Converters.cs</c>, read from the same string table through <see cref="IStringLocalizer{T}"/>.
/// Keys follow <c>{EnumType}_{Value}</c> for enums.
/// </summary>
public sealed class Text(IStringLocalizer<Strings> localizer)
{
    public string this[string key]
    {
        get
        {
            var value = localizer[key];
            return value.ResourceNotFound ? key : value.Value;
        }
    }

    public string Format(string key, params object?[] args) => string.Format(CultureInfo.CurrentCulture, this[key], args);

    public string Enum<T>(T value) where T : struct, System.Enum => this[$"{typeof(T).Name}_{value}"];

    public string Date(DateOnly? date) => date is { } d ? d.ToString("d MMM yyyy", CultureInfo.CurrentCulture) : "—";

    public string Date(DateTimeOffset at) => Date(DateOnly.FromDateTime(at.LocalDateTime));

    public string Problem(Problem problem) => this[$"Problem_{problem}"];

    /// <summary>"3 works", "1 open treatment": singular and plural keys per the string table.</summary>
    public string Count(string key, int count) => Format(count == 1 ? key + "_One" : key + "_Many", count);

    public string Due(DueState due, DateOnly? next) => due switch
    {
        DueState.NeverSurveyed => this["Due_Never"],
        DueState.Overdue => Format("Due_Overdue", Date(next)),
        DueState.DueSoon => Format("Due_Soon", Date(next)),
        _ => Format("Due_Current", Date(next)),
    };

    public string Due(ArtworkSummary s) => Due(s.Due, s.NextDue);

    /// <summary>"Condition 3 of 4, Poor" for assistive tech; the word alone for the unsurveyed grade.</summary>
    public string Grade(ConditionGrade g) => g == ConditionGrade.Unsurveyed ? Enum(g) : Format("Grade_Accessible", (int)g, Enum(g));

    public string NextAction(TreatmentStatus s) => this[$"NextAction_{s}"];

    public string Log(TreatmentLogEntry e)
    {
        var headline = e.Kind switch
        {
            LogKind.Proposed => this["Log_Proposed"],
            LogKind.StatusChanged when e.Status is { } s => Format("Log_StatusChanged", Enum(s)),
            LogKind.StatusReturned when e.Status is { } s => Format("Log_StatusReturned", Enum(s)),
            _ => string.Empty,
        };

        return string.IsNullOrWhiteSpace(e.Text) ? headline
            : string.IsNullOrEmpty(headline) ? e.Text
            : $"{headline} {e.Text}";
    }

    public string Byline(Artwork a) => $"{a.Artist}, {a.Year}";

    public string Byline(ArtworkSummary s) => $"{s.Artist}, {s.Year}";

    public string Place(Artwork a) => $"{Enum(a.Material)} · {a.Site} · {a.District}";

    /// <summary>"Bronze works are surveyed every 12 months; at Poor this one is due every 6 months."</summary>
    public string Interval(ArtworkDossier d)
    {
        var material = d.Artwork.Material;
        var baseMonths = ConditionRules.BaseIntervalMonths(material);
        var months = ConditionRules.IntervalMonths(material, d.Summary.Grade);
        return months == baseMonths
            ? Format("Artwork_Interval", Enum(material), baseMonths)
            : Format("Artwork_IntervalAdjusted", Enum(material), baseMonths, Enum(d.Summary.Grade), months);
    }

    public string SurveyAction(ArtworkDossier d) => d.Draft is null ? this["Artwork_StartSurvey"] : this["Artwork_ResumeSurvey"];

    public string Finding(Finding f) => $"{Enum(f.Type)} · {f.Zone} · {Enum(f.Severity)}";

    public string SurveyMeta(Survey s) => $"{s.Surveyor} · {Enum(s.Weather)}";

    /// <summary>The treatment title proposed from a finding, in the surveyor's language.</summary>
    public string TreatmentTitle(Finding finding)
    {
        var verb = this[$"TreatmentVerb_{finding.Type}"];
        return string.IsNullOrWhiteSpace(finding.Zone) ? verb : verb + this["TitleSeparator"] + finding.Zone.Trim();
    }
}
