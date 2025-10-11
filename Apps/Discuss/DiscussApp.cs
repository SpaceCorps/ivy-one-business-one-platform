namespace IvyOneBusinessOnePlatform.Apps.Discuss;

[App(icon: Icons.MessageCircle, title: "Discuss", path: new[] { "Customer Service" })]
public class DiscussApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedChannel = this.UseState("general");
        
        var channels = new[] { "general", "support", "sales", "development" };
        var messages = new[]
        {
            new { User = "John Doe", Message = "Welcome to the discussion!", Time = DateTime.Now.AddMinutes(-30) },
            new { User = "Jane Smith", Message = "Thanks! Happy to be here.", Time = DateTime.Now.AddMinutes(-25) },
            new { User = "Bob Johnson", Message = "How can I help you today?", Time = DateTime.Now.AddMinutes(-10) }
        };
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Team Discussion")
                .Add("Communicate with your team in real-time")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(12)
                    .Add(channels.Select(ch => new Button($"#{ch}", _ => {
                        selectedChannel.Set(ch);
                        client.Toast($"Switched to #{ch}");
                    })
                        .Small()
                        .Variant(selectedChannel.Value == ch ? ButtonVariant.Primary : ButtonVariant.Outline))))
                .Add(new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Padding(16)
                        .Add($"#{selectedChannel.Value}")
                        .Add(Layout.Vertical()
                            .Gap(8)
                            .Add(messages.Select(msg => new Card(
                                Layout.Vertical()
                                    .Gap(4)
                                    .Padding(12)
                                    .Add(Layout.Horizontal()
                                        .Gap(8)
                                        .Add(new Badge(msg.User).Variant(BadgeVariant.Secondary))
                                        .Add(new Badge(msg.Time.ToString("HH:mm")).Variant(BadgeVariant.Outline)))
                                    .Add(msg.Message)
                            ))))
                        .Add(Layout.Horizontal()
                            .Gap(8)
                            .Add(new TextInput(UseState("")).Placeholder("Type your message..."))
                            .Add(new Button("Send", _ => client.Toast("Message sent!"))
                                .Variant(ButtonVariant.Primary)
                                .Icon(Icons.Send)))))
        );
    }
}
