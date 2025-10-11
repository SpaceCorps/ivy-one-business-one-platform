namespace IvyOneBusinessOnePlatform.Apps.Manufacturing;

[App(icon: Icons.Settings, title: "Manufacturing")]
public class ManufacturingApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Manufacturing Management")
                .Add("Manage production lines, quality control, and manufacturing processes")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Production Schedule", _ => client.Toast("Production planning coming soon!")))
                    .Add(new Button("Quality Control", _ => client.Toast("QC management coming soon!")))
                    .Add(new Button("Work Orders", _ => client.Toast("Work order system coming soon!")))
                )
        );
    }
}
