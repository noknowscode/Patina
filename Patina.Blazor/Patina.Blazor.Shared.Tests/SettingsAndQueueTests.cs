namespace Patina.Blazor.Shared.Tests;

public class SettingsAndQueueTests : TestHost
{
    [Test]
    public void Queue_filters_by_status_and_offers_the_open_list_when_empty()
    {
        var cut = Render<QueuePage>();
        cut.WaitForAssertion(() => cut.FindAll("a.list-row").Should().HaveCount(5));
        cut.Find("h1.t-display").TextContent.Should().Be("2");
        cut.FindAll("a.list-row")[0].TextContent.Should().Contain("Late");

        cut.FindAll("button.chip").First(c => c.TextContent == "In progress").Click();
        cut.FindAll("a.list-row").Should().HaveCount(1);

        var doneChip = cut.FindAll("button.chip").First(c => c.TextContent == "Done");
        doneChip.Click();
        var done = Doc().Treatments.Count(t => t.Status == TreatmentStatus.Done);
        if (done == 0)
        {
            cut.Markup.Should().Contain("Nothing here");
            cut.Find(".state button").Click();
            cut.FindAll("a.list-row").Should().HaveCount(5);
        }
        else
        {
            cut.FindAll("a.list-row").Should().HaveCount(done);
        }
    }

    [Test]
    public void Settings_persist_surveyor_theme_language_and_motion()
    {
        var settings = Services.GetRequiredService<IAppSettings>();
        var cut = Render<SettingsPage>();

        cut.Find("#surveyor").Change("C. Lavoie");
        settings.Surveyor.Should().Be("C. Lavoie");

        cut.FindAll(".segmented button").First(b => b.TextContent == "Dark").Click();
        settings.Theme.Should().Be(AppTheme.Dark);

        cut.Find("#reduce-motion").Change(true);
        settings.ReduceMotion.Should().BeTrue();

        cut.FindAll(".segmented button").First(b => b.TextContent == "Français").Click();
        settings.Language.Should().Be("fr");
        cut.Find("p.message").TextContent.Should().Be("Restart Patina to switch language.");
    }

    [Test]
    public void Restore_sample_asks_first_then_replaces_the_collection()
    {
        var store = Store;
        var artwork = Doc().Artworks[0];
        store.UpdateAsync(doc => SurveyOps.SaveDraft(doc, SurveyOps.NewDraft(artwork.Id, "x", new DateOnly(2026, 10, 9)))).GetAwaiter().GetResult();
        SurveyOps.DraftFor(Doc(), artwork.Id).Should().NotBeNull();

        var cut = Render<SettingsPage>();
        cut.FindAll("button.btn-text").First(b => b.TextContent == "Restore sample collection").Click();
        cut.Find("[role=dialog]").TextContent.Should().Contain("Restore the sample collection?");

        cut.Find("[role=dialog] .btn-text").Click();
        SurveyOps.DraftFor(Doc(), artwork.Id).Should().NotBeNull("cancel changes nothing");

        cut.FindAll("button.btn-text").First(b => b.TextContent == "Restore sample collection").Click();
        cut.Find("[role=dialog] .btn-filled").Click();
        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().Be("Sample collection restored."));
        SurveyOps.DraftFor(Doc(), artwork.Id).Should().BeNull();
    }

    [Test]
    public void Export_hands_the_archive_to_the_file_saver()
    {
        var cut = Render<SettingsPage>();

        cut.FindAll("button.btn-outlined").First(b => b.TextContent == "Export collection").Click();

        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().Be("Collection exported."));
    }
}
