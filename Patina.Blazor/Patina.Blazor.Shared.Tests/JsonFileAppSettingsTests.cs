namespace Patina.Blazor.Shared.Tests;

public class JsonFileAppSettingsTests
{
    private string _folder = "";

    [SetUp]
    public void SetUp() => _folder = Path.Combine(Path.GetTempPath(), "patina-tests", Guid.NewGuid().ToString("N"));

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
    }

    [Test]
    public void Defaults_match_the_uno_app()
    {
        var settings = new JsonFileAppSettings(_folder);

        settings.Surveyor.Should().BeEmpty();
        settings.ReduceMotion.Should().BeFalse();
        settings.Theme.Should().Be(AppTheme.System);
        settings.Language.Should().Be("en");
    }

    [Test]
    public void Values_persist_across_instances_and_surveyor_is_trimmed()
    {
        var first = new JsonFileAppSettings(_folder)
        {
            Surveyor = "  Marie Tremblay ",
            ReduceMotion = true,
            Theme = AppTheme.Dark,
            Language = "fr",
        };

        var second = new JsonFileAppSettings(_folder);

        second.Surveyor.Should().Be("Marie Tremblay");
        second.ReduceMotion.Should().BeTrue();
        second.Theme.Should().Be(AppTheme.Dark);
        second.Language.Should().Be("fr");
        first.Surveyor.Should().Be(second.Surveyor);
    }

    [Test]
    public void Damaged_settings_file_falls_back_to_defaults()
    {
        Directory.CreateDirectory(_folder);
        File.WriteAllText(Path.Combine(_folder, "settings.json"), "{ nope");

        var settings = new JsonFileAppSettings(_folder);

        settings.Theme.Should().Be(AppTheme.System);
        settings.Language.Should().Be("en");
    }
}
