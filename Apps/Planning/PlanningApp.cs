namespace IvyOneBusinessOnePlatform.Apps.Planning;

[App(icon: Icons.Calendar, title: "Planning", path: new[] { "Project & Time Management" })]
public class PlanningApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedDate = this.UseState(DateTime.Today);
        
        var events = new[]
        {
            new { Title = "Team Meeting", Time = "10:00 AM", Duration = "1h", Type = "Meeting" },
            new { Title = "Project Review", Time = "2:00 PM", Duration = "2h", Type = "Review" },
            new { Title = "Client Call", Time = "4:00 PM", Duration = "30m", Type = "Call" }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Planning & Calendar")
                .Add("Manage schedules, meetings, and events")
                .Add(Layout.Grid()
                    .Columns(2)
                    .Gap(16)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add($"Today - {selectedDate.Value:MMMM dd, yyyy}")
                            .Add(Layout.Vertical()
                                .Gap(8)
                                .Add(events.Select(evt => new Card(
                                    Layout.Horizontal()
                                        .Gap(12)
                                        .Padding(12)
                                        .Add(Layout.Vertical()
                                            .Gap(4)
                                            .Add(evt.Title)
                                            .Add($"{evt.Time} ({evt.Duration})")
                                            .Add(new Badge(evt.Type).Variant(BadgeVariant.Secondary)))
                                        .Add(new Button("Edit", _ => client.Toast($"Editing: {evt.Title}"))
                                            .Small()
                                            .Variant(ButtonVariant.Outline))
                                ))))
                            .Add(new Button("New Event", _ => client.Toast("Create new event"))
                                .Icon(Icons.Plus)
                                .Variant(ButtonVariant.Primary))))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(12)
                            .Padding(16)
                            .Add("Upcoming")
                            .Add("Week View")
                            .Add(Layout.Vertical()
                                .Gap(4)
                                .Add("Mon - 3 events")
                                .Add("Tue - 2 events")
                                .Add("Wed - 5 events")
                                .Add("Thu - 1 event")
                                .Add("Fri - 4 events")))))
        );
    }
}
