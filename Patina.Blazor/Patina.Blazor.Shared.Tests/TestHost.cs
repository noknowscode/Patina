namespace Patina.Blazor.Shared.Tests;

/// <summary>A bUnit context with the real Shared service graph over a temporary data folder, seeded like a first run.</summary>
public abstract class TestHost : BunitContext
{
    protected string Folder { get; } = Path.Combine(Path.GetTempPath(), "patina-tests", Guid.NewGuid().ToString("N"));

    protected PatinaStore Store => Services.GetRequiredService<PatinaStore>();

    protected TestHost()
    {
        Services.AddSingleton<IFileSaver, FakeFileSaver>();
        Services.AddPatinaCore(Folder);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    protected PatinaDocument Doc() => Store.GetAsync().GetAwaiter().GetResult();

    [TearDown]
    public void CleanUp()
    {
        Dispose();
        if (Directory.Exists(Folder))
        {
            Directory.Delete(Folder, recursive: true);
        }
    }

    private sealed class FakeFileSaver : IFileSaver
    {
        public List<string> Saved { get; } = [];

        public Task<bool> SaveAsync(string fileName, string contentType, byte[] content, CancellationToken ct)
        {
            Saved.Add(fileName);
            return Task.FromResult(true);
        }
    }
}
