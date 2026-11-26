namespace Patina.Controls;

/// <summary>
/// Reads a <see cref="Windows.UI.Color"/> resource for a given theme, for code that draws outside XAML (the map).
/// Searches the app's theme dictionaries for the theme's key ("Light" / "Dark"), last merged dictionary first like
/// XAML lookup, so the Material palette override and Styles/Condition.xaml both resolve.
/// </summary>
internal static class ThemeColors
{
    public static Windows.UI.Color Get(string key, ElementTheme theme)
    {
        var resources = Application.Current.Resources;
        var themeKey = theme == ElementTheme.Light ? "Light" : "Dark";
        if (Find(resources, key, themeKey) is { } color)
        {
            return color;
        }

        return resources.TryGetValue(key, out var value) && value is Windows.UI.Color fallback ? fallback : default;
    }

    private static Windows.UI.Color? Find(ResourceDictionary dictionary, string key, string themeKey)
    {
        if (dictionary.ThemeDictionaries.TryGetValue(themeKey, out var themed)
            && themed is ResourceDictionary themeDictionary
            && themeDictionary.TryGetValue(key, out var value)
            && value is Windows.UI.Color color)
        {
            return color;
        }

        for (var i = dictionary.MergedDictionaries.Count - 1; i >= 0; i--)
        {
            if (Find(dictionary.MergedDictionaries[i], key, themeKey) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
