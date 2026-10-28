using Patina.Blazor.Shared.Services;

namespace Patina.Blazor.Maui.Services;

/// <summary>Device preferences in MAUI <see cref="Preferences"/>, the counterpart of Uno's <c>ApplicationData.LocalSettings</c>.</summary>
public sealed class PreferencesAppSettings : IAppSettings
{
    private const string SurveyorKey = "surveyor";
    private const string ReduceMotionKey = "reduceMotion";
    private const string ThemeKey = "theme";
    private const string LanguageKey = "language";

    public string Surveyor
    {
        get => Preferences.Default.Get(SurveyorKey, string.Empty);
        set => Preferences.Default.Set(SurveyorKey, value.Trim());
    }

    public bool ReduceMotion
    {
        get => Preferences.Default.Get(ReduceMotionKey, false);
        set => Preferences.Default.Set(ReduceMotionKey, value);
    }

    public AppTheme Theme
    {
        get => Enum.TryParse<AppTheme>(Preferences.Default.Get(ThemeKey, nameof(AppTheme.System)), out var t) ? t : AppTheme.System;
        set => Preferences.Default.Set(ThemeKey, value.ToString());
    }

    public string Language
    {
        get => Preferences.Default.Get(LanguageKey, "en");
        set => Preferences.Default.Set(LanguageKey, value);
    }
}
