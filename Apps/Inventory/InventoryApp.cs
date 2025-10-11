namespace IvyOneBusinessOnePlatform.Apps.Inventory;

[App(icon: Icons.Box, title: "Inventory")]
public class InventoryApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Inventory Management")
                .Add("Track stock levels, manage warehouses, and optimize inventory")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Stock Levels", _ => client.Toast("Inventory tracking coming soon!")))
                    .Add(new Button("Warehouse Management", _ => client.Toast("Warehouse tools coming soon!")))
                    .Add(new Button("Reorder Reports", _ => client.Toast("Reorder alerts coming soon!")))
                )
        );
    }
}
