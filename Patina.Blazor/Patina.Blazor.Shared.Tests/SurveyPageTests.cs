using Microsoft.AspNetCore.Components;

namespace Patina.Blazor.Shared.Tests;

public class SurveyPageTests : TestHost
{
    private string ArtworkId => Doc().Artworks.First(a => a.Material == ArtMaterial.Bronze).Id;

    [Test]
    public void Finding_needs_a_type_and_a_zone_and_the_grade_follows_the_worst_finding()
    {
        var cut = Render<SurveyPage>(p => p.Add(x => x.ArtworkId, ArtworkId));
        cut.WaitForAssertion(() => cut.Find("#finding-type"));

        cut.Find("button.btn-outlined:not(label)").Click();
        cut.Find("p.message").TextContent.Should().Be("Choose a type for the finding.");

        cut.Find("#finding-type").Change(nameof(FindingType.Corrosion));
        cut.Find("button.btn-outlined:not(label)").Click();
        cut.Find("p.message").TextContent.Should().Be("Say where on the object each finding is.");

        cut.Find("#finding-zone").Input("North face");
        cut.FindAll(".severity button").First(b => b.TextContent.Contains("Urgent")).Click();
        cut.Find("button.btn-outlined:not(label)").Click();

        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().BeEmpty());
        cut.Find(".condition.large").GetAttribute("data-grade").Should().Be("4");
        cut.Markup.Should().Contain("Corrosion · North face · Urgent");

        // the draft was saved at once
        var draft = SurveyOps.DraftFor(Doc(), ArtworkId);
        draft.Should().NotBeNull();
        draft!.Findings.Should().ContainSingle().Which.Severity.Should().Be(Severity.Urgent);
    }

    [Test]
    public void Submit_requires_a_surveyor_then_grades_and_proposes_treatments()
    {
        var cut = Render<SurveyPage>(p => p.Add(x => x.ArtworkId, ArtworkId));
        cut.WaitForAssertion(() => cut.Find("#finding-type"));
        var nav = Services.GetRequiredService<NavigationManager>();

        cut.Find("#finding-type").Change(nameof(FindingType.Corrosion));
        cut.Find("#finding-zone").Input("Base");
        cut.FindAll(".severity button").First(b => b.TextContent.Contains("Serious")).Click();
        cut.Find("button.btn-outlined:not(label)").Click();
        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Corrosion · Base · Serious"));

        cut.Find("#surveyor").Input("");
        cut.FindAll("button.btn-filled").First(b => b.TextContent == "Submit survey").Click();
        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().Be("Enter the surveyor's name before submitting."));

        cut.Find("#surveyor").Input("Marie Tremblay");
        cut.FindAll("button.btn-filled").First(b => b.TextContent == "Submit survey").Click();

        cut.WaitForAssertion(() => nav.Uri.Should().EndWith($"artwork/{ArtworkId}"));
        var doc = Doc();
        var survey = doc.Surveys.Where(s => s.ArtworkId == ArtworkId).OrderByDescending(s => s.Date).First();
        survey.Status.Should().Be(SurveyStatus.Submitted);
        survey.Grade.Should().Be(ConditionGrade.Poor);
        doc.Treatments.Should().Contain(t => t.SurveyId == survey.Id && t.Priority == TreatmentPriority.High && t.Title == "Treat corrosion: Base");
        Services.GetRequiredService<IAppSettings>().Surveyor.Should().Be("Marie Tremblay");
    }

    [Test]
    public void Back_keeps_the_draft_and_discard_removes_it_after_confirmation()
    {
        var cut = Render<SurveyPage>(p => p.Add(x => x.ArtworkId, ArtworkId));
        cut.WaitForAssertion(() => cut.Find("#notes"));
        var nav = Services.GetRequiredService<NavigationManager>();

        cut.Find("#notes").Input("Cold morning, light frost on the base.");
        cut.Find("header button").Click();
        cut.WaitForAssertion(() => nav.Uri.Should().EndWith($"artwork/{ArtworkId}"));
        SurveyOps.DraftFor(Doc(), ArtworkId)!.Notes.Should().Be("Cold morning, light frost on the base.");

        var resumed = Render<SurveyPage>(p => p.Add(x => x.ArtworkId, ArtworkId));
        resumed.WaitForAssertion(() => resumed.Find("#notes").GetAttribute("value").Should().Be("Cold morning, light frost on the base."));

        resumed.FindAll("button.btn-text").First(b => b.TextContent == "Discard draft").Click();
        resumed.Find("[role=dialog]").TextContent.Should().Contain("Discard this draft?");
        resumed.Find("[role=dialog] .btn-filled").Click();

        resumed.WaitForAssertion(() => SurveyOps.DraftFor(Doc(), ArtworkId).Should().BeNull());
    }
}
