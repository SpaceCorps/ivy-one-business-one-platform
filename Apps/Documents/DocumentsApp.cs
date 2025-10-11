namespace IvyOneBusinessOnePlatform.Apps.Documents;

[App(icon: Icons.FileText, title: "Documents")]
public class DocumentsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Document Management")
                .Add("Store, organize, and manage your documents")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Upload Document", _ => client.Toast("Document upload coming soon!")))
                    .Add(new Button("Browse Files", _ => client.Toast("File browser coming soon!")))
                    .Add(new Button("Search Documents", _ => client.Toast("Document search coming soon!")))
                )
        );
    }
}
