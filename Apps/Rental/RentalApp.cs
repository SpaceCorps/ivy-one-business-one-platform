namespace IvyOneBusinessOnePlatform.Apps.Rental;

[App(icon: Icons.Key, title: "Rental")]
public class RentalApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Rental Management")
                .Add("Manage property rentals, bookings, and tenant information")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("View Properties", _ => client.Toast("Property listings coming soon!")))
                    .Add(new Button("Manage Bookings", _ => client.Toast("Booking management coming soon!")))
                    .Add(new Button("Tenant Portal", _ => client.Toast("Tenant portal coming soon!")))
                )
        );
    }
}
