namespace Patina.Blazor.Shared.Services;

/// <summary>
/// Device preferences. Each host owns the storage: MAUI keeps them in <c>Preferences</c>, the web host in a JSON
/// file beside the collection. Theme and language live here too, because Blazor has no framework service for
/// either (the Uno app delegates those to <c>IThemeService</c> and <c>ILocalizationService</c>).
/// </summary>
public interface IAppSettings
{
    /// <summary>Name stamped on surveys and treatment log entries.</summary>
    string Surveyor { get; set; }

    /// <summary>In-app reduced motion, applied on top of the system <c>prefers-reduced-motion</c> query.</summary>
    bool ReduceMotion { get; set; }

    /// <summary>Theme choice; <see cref="AppTheme.System"/> follows <c>prefers-color-scheme</c>.</summary>
    AppTheme Theme { get; set; }

    /// <summary>UI culture, "en" or "fr".</summary>
    string Language { get; set; }
}

public enum AppTheme
{
    System,
    Light,
    Dark,
}
