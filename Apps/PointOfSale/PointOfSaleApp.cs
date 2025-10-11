namespace IvyOneBusinessOnePlatform.Apps.PointOfSale;

[App(icon: Icons.ShoppingCart, title: "Point of Sale", path: new[] { "Commerce & Services" })]
public class PointOfSaleApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Point of Sale System")
                .Add("Process sales transactions and manage inventory")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("New Sale", _ => client.Toast("POS interface coming soon!")))
                    .Add(new Button("View Sales", _ => client.Toast("Sales history coming soon!")))
                    .Add(new Button("Inventory Check", _ => client.Toast("Inventory check coming soon!")))
                )
        );
    }
}
