using System.Text.Json;
using System.Text.Json.Serialization;

namespace Patina.Blazor.Shared.Services;

/// <summary>
/// Settings in <c>settings.json</c> beside the collection. Used by the web host, where there is no per-device
/// preference store; the MAUI host uses the platform's own <c>Preferences</c> instead.
/// </summary>
public sealed class JsonFileAppSettings : IAppSettings
{
    private const string FileName = "settings.json";
    private readonly string _path;
    private readonly Lock _gate = new();
    private Values _values;

    public JsonFileAppSettings(string folder)
    {
        _path = Path.Combine(folder, FileName);
        _values = Load(_path);
    }

    public string Surveyor
    {
        get => _values.Surveyor;
        set => Update(v => v with { Surveyor = value.Trim() });
    }

    public bool ReduceMotion
    {
        get => _values.ReduceMotion;
        set => Update(v => v with { ReduceMotion = value });
    }

    public AppTheme Theme
    {
        get => _values.Theme;
        set => Update(v => v with { Theme = value });
    }

    public string Language
    {
        get => _values.Language;
        set => Update(v => v with { Language = value });
    }

    private void Update(Func<Values, Values> change)
    {
        lock (_gate)
        {
            _values = change(_values);
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var temp = _path + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(_values, SettingsJsonContext.Default.Values));
            File.Move(temp, _path, overwrite: true);
        }
    }

    private static Values Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Values();
        }

        try
        {
            return JsonSerializer.Deserialize(File.ReadAllText(path), SettingsJsonContext.Default.Values) ?? new Values();
        }
        catch (JsonException)
        {
            return new Values();
        }
    }

    public sealed record Values(
        string Surveyor = "",
        bool ReduceMotion = false,
        AppTheme Theme = AppTheme.System,
        string Language = "en");
}

[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(JsonFileAppSettings.Values))]
internal sealed partial class SettingsJsonContext : JsonSerializerContext;
