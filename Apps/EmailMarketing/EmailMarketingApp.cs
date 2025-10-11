namespace IvyOneBusinessOnePlatform.Apps.EmailMarketing;

[App(icon: Icons.Mail, title: "Email Marketing", path: new[] { "Marketing & Communication" })]
public class EmailMarketingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Email Marketing")
                .Add("Create and manage email campaigns and newsletters")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Create Campaign", _ => client.Toast("Email campaigns coming soon!")))
                    .Add(new Button("Subscriber Lists", _ => client.Toast("List management coming soon!")))
                    .Add(new Button("Email Templates", _ => client.Toast("Template designer coming soon!")))
                )
        );
    }
}
