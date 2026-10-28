namespace Patina.Blazor.Shared.Tests;

public class CollectionPageTests : TestHost
{
    [Test]
    public void Lists_every_artwork_ordered_by_urgency_with_stats()
    {
        var cut = Render<Collection>();

        cut.WaitForAssertion(() => cut.FindAll("a.list-row").Should().HaveCount(18));
        cut.Find("h1.t-display").TextContent.Should().Be("10");
        cut.Markup.Should().Contain("18 works").And.Contain("2 critical").And.Contain("4 overdue");

        var first = cut.FindAll("a.list-row")[0];
        first.QuerySelector(".condition")!.GetAttribute("data-grade").Should().Be("4");
        first.QuerySelector(".condition")!.GetAttribute("aria-label").Should().Be("Condition 4 of 4, Critical");
    }

    [Test]
    public void Search_and_filter_narrow_the_list_and_clear_restores_it()
    {
        var cut = Render<Collection>();
        cut.WaitForAssertion(() => cut.FindAll("a.list-row").Should().HaveCount(18));

        cut.Find("input[type=search]").Input("Ferryman");
        cut.FindAll("a.list-row").Should().ContainSingle().Which.TextContent.Should().Contain("The Ferryman");

        cut.Find("input[type=search]").Input("zzz-no-match");
        cut.Markup.Should().Contain("No works match");
        cut.Find(".state button").Click();
        cut.FindAll("a.list-row").Should().HaveCount(18);

        var overdue = cut.FindAll("button.chip").First(c => c.TextContent == "Overdue");
        overdue.Click();
        cut.FindAll("a.list-row").Should().HaveCount(4);
        cut.FindAll("button.chip").First(c => c.TextContent == "Overdue").GetAttribute("aria-pressed").Should().Be("true");
    }

    [Test]
    public void Rows_link_to_the_artwork_record()
    {
        var cut = Render<Collection>();
        cut.WaitForAssertion(() => cut.FindAll("a.list-row").Should().HaveCount(18));

        var ids = Doc().Artworks.Select(a => a.Id).ToHashSet();
        foreach (var row in cut.FindAll("a.list-row"))
        {
            ids.Should().Contain(row.GetAttribute("href")!.Replace("artwork/", ""));
        }
    }
}
