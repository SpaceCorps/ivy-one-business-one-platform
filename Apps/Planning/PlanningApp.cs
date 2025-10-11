namespace IvyOneBusinessOnePlatform.Apps.Planning;

[App(icon: Icons.Calendar, title: "Planning", path: new[] { "Project & Time Management" })]
public class PlanningApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new PlanningRootBlade(), "Planning");
    }
}

public class PlanningRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var events = new[]
        {
            new { Title = "Team Meeting", Time = "10:00 AM", Duration = "1h", Type = "Meeting" },
            new { Title = "Project Review", Time = "2:00 PM", Duration = "2h", Type = "Review" },
            new { Title = "Client Call", Time = "4:00 PM", Duration = "30m", Type = "Call" }
        };
        
        var listItems = events.Select(evt => new ListItem(
            title: evt.Title,
            subtitle: $"{evt.Time} ({evt.Duration})",
            icon: Icons.Calendar,
            badge: evt.Type,
            onClick: _ => { blades.Push(this, new EventDetailBlade(evt.Title, evt.Time, evt.Duration, evt.Type), evt.Title); }
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Upcoming Events"))
                .Add(new Button("New Event", _ => client.Toast("Create event"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(new List(listItems));
    }
}

public class EventDetailBlade(string title, string time, string duration, string type) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(title)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Time: {time}")
                    .Add($"Duration: {duration}")
                    .Add(new Badge(type).Variant(BadgeVariant.Secondary)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit event"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Delete", _ => client.Toast("Delete event"))
                        .Variant(ButtonVariant.Destructive)))
        );
    }
}
