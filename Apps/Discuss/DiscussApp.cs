namespace IvyOneBusinessOnePlatform.Apps.Discuss;

[App(icon: Icons.MessageCircle, title: "Discuss", path: new[] { "Customer Service" })]
public class DiscussApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new DiscussRootBlade(), "Discuss", Size.Units(80));
    }
}

public class DiscussRootBlade : ViewBase
{
    public override object? Build()
    {
        var blades = this.UseContext<IBladeController>();
        
        var channels = new[]
        {
            new { Name = "general", Description = "General discussion", Count = 45 },
            new { Name = "support", Description = "Customer support", Count = 12 },
            new { Name = "sales", Description = "Sales team chat", Count = 8 },
            new { Name = "development", Description = "Dev team discussions", Count = 23 }
        };
        
        var listItems = channels.Select(ch => new ListItem(
            title: $"#{ch.Name}",
            subtitle: ch.Description,
            icon: Icons.MessageCircle,
            badge: ch.Count.ToString(),
            onClick: _ => { blades.Push(this, new ChannelBlade(ch.Name), $"#{ch.Name}"); }
        ));
        
        return new List(listItems);
    }
}

public class ChannelBlade(string channelName) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        var messages = new[]
        {
            new { User = "John Doe", Message = "Welcome to the discussion!", Time = DateTime.Now.AddMinutes(-30) },
            new { User = "Jane Smith", Message = "Thanks! Happy to be here.", Time = DateTime.Now.AddMinutes(-25) },
            new { User = "Bob Johnson", Message = "How can I help you today?", Time = DateTime.Now.AddMinutes(-10) }
        };
        
        var listItems = messages.Select(msg => new ListItem(
            title: msg.User,
            subtitle: $"{msg.Time:HH:mm} - {msg.Message}",
            icon: Icons.User
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3($"#{channelName}"))
            .Add(new List(listItems))
            .Add(Layout.Horizontal()
                .Gap(8)
                .Add(new TextInput(UseState("")).Placeholder("Type your message..."))
                .Add(new Button("Send", _ => client.Toast("Message sent!"))
                    .Variant(ButtonVariant.Primary)
                    .Icon(Icons.Send)));
    }
}
