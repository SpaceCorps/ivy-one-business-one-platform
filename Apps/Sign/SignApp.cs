namespace IvyOneBusinessOnePlatform.Apps.Sign;

[App(icon: Icons.Signature, title: "Sign")]
public class SignApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Digital Signatures")
                .Add("Sign documents electronically and manage signatures")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Sign Document", _ => client.Toast("Document signing coming soon!")))
                    .Add(new Button("Upload Document", _ => client.Toast("Document upload coming soon!")))
                    .Add(new Button("Signature History", _ => client.Toast("Signature history coming soon!")))
                )
        );
    }
}
