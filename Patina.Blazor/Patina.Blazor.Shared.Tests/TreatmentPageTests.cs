namespace Patina.Blazor.Shared.Tests;

public class TreatmentPageTests : TestHost
{
    [Test]
    public void Scheduling_needs_a_date_and_marking_done_needs_a_note()
    {
        var proposed = Doc().Treatments.First(t => t.Status == TreatmentStatus.Proposed);
        var cut = Render<TreatmentPage>(p => p.Add(x => x.Id, proposed.Id));
        cut.WaitForAssertion(() => cut.Find("#scheduled"));

        cut.FindAll(".step")[0].ClassList.Should().Contain("is-current");
        cut.Find("button.btn-filled").TextContent.Should().Be("Schedule");

        cut.Find("button.btn-filled").Click();
        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().Be("Pick a date to schedule the work."));

        cut.Find("#scheduled").Change("2026-10-20");
        cut.Find("#assignee").Input("M. Haddad");
        cut.Find("button.btn-filled").Click();
        cut.WaitForAssertion(() => cut.Find("button.btn-filled").TextContent.Should().Be("Start work"));
        cut.FindAll(".step")[0].ClassList.Should().Contain("is-done");
        cut.FindAll(".step")[1].ClassList.Should().Contain("is-current");

        cut.Find("button.btn-filled").Click();
        cut.WaitForAssertion(() => cut.Find("button.btn-filled").TextContent.Should().Be("Mark done"));

        cut.Find("button.btn-filled").Click();
        cut.WaitForAssertion(() => cut.Find("p.message").TextContent.Should().Be("Add a note saying what was done before marking it done."));

        cut.Find("#note").Input("Cleaned and waxed.");
        cut.Find("button.btn-filled").Click();
        cut.WaitForAssertion(() => cut.FindAll("button.btn-filled").Should().BeEmpty());

        var done = Doc().Treatments.First(t => t.Id == proposed.Id);
        done.Status.Should().Be(TreatmentStatus.Done);
        done.ScheduledFor.Should().Be(new DateOnly(2026, 10, 20));
        done.Assignee.Should().Be("M. Haddad");
        done.Log.Last().Text.Should().Be("Cleaned and waxed.");
        cut.Markup.Should().Contain("Moved to Done. Cleaned and waxed.");
    }

    [Test]
    public void Step_back_returns_to_the_previous_status_and_logs_it()
    {
        var scheduled = Doc().Treatments.First(t => t.Status == TreatmentStatus.Scheduled);
        var cut = Render<TreatmentPage>(p => p.Add(x => x.Id, scheduled.Id));
        cut.WaitForAssertion(() => cut.Find("#scheduled"));

        cut.FindAll("button.btn-text").First(b => b.TextContent == "Step back").Click();

        cut.WaitForAssertion(() => cut.Find("button.btn-filled").TextContent.Should().Be("Schedule"));
        Doc().Treatments.First(t => t.Id == scheduled.Id).Log.Last().Kind.Should().Be(LogKind.StatusReturned);
        cut.Markup.Should().Contain("Returned to Proposed.");
    }

    [Test]
    public void A_missing_treatment_shows_the_gone_state()
    {
        var cut = Render<TreatmentPage>(p => p.Add(x => x.Id, "nope"));

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("This treatment is no longer in the queue"));
    }
}
