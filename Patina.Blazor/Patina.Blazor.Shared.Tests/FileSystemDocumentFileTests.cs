namespace Patina.Blazor.Shared.Tests;

public class FileSystemDocumentFileTests
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
    public async Task Read_returns_null_before_the_first_write()
    {
        var file = new FileSystemDocumentFile(_folder);

        (await file.ReadAsync(default)).Should().BeNull();
    }

    [Test]
    public async Task Write_creates_the_folder_and_round_trips()
    {
        var file = new FileSystemDocumentFile(_folder);

        await file.WriteAsync("{\"v\":1}", default);

        (await file.ReadAsync(default)).Should().Be("{\"v\":1}");
        Directory.GetFiles(_folder).Should().ContainSingle().Which.Should().EndWith("patina.json");
    }

    [Test]
    public async Task Write_replaces_and_leaves_no_temp_file()
    {
        var file = new FileSystemDocumentFile(_folder);
        await file.WriteAsync("one", default);

        await file.WriteAsync("two", default);

        (await file.ReadAsync(default)).Should().Be("two");
        Directory.GetFiles(_folder).Select(Path.GetFileName).Should().BeEquivalentTo("patina.json");
    }

    [Test]
    public async Task Move_aside_keeps_the_damaged_file_and_clears_the_document()
    {
        var file = new FileSystemDocumentFile(_folder);
        await file.WriteAsync("{ not json", default);

        await file.MoveAsideAsync("corrupt", default);

        (await file.ReadAsync(default)).Should().BeNull();
        var kept = Directory.GetFiles(_folder).Select(Path.GetFileName).Should().ContainSingle().Which;
        kept.Should().StartWith("patina.corrupt-").And.EndWith(".json");
        (await File.ReadAllTextAsync(Path.Combine(_folder, kept))).Should().Be("{ not json");
    }

    [Test]
    public async Task Store_seeds_through_the_file_on_first_run()
    {
        var store = new PatinaStore(new FileSystemDocumentFile(_folder), new SystemClock());

        var doc = await store.GetAsync();

        doc.Artworks.Should().NotBeEmpty();
        File.Exists(Path.Combine(_folder, "patina.json")).Should().BeTrue();
    }
}
