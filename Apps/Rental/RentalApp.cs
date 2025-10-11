using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Rental;

[App(icon: Icons.Key, title: "Rental", path: new[] { "Commerce & Services" })]
public class RentalApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var selectedView = this.UseState("properties");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Rental Management")
                .Add("Manage properties, bookings, and tenant information")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Properties", _ => selectedView.Set("properties"))
                        .Variant(selectedView.Value == "properties" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Bookings", _ => selectedView.Set("bookings"))
                        .Variant(selectedView.Value == "bookings" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Tenants", _ => selectedView.Set("tenants"))
                        .Variant(selectedView.Value == "tenants" ? ButtonVariant.Primary : ButtonVariant.Secondary)))
                .Add(BuildView(selectedView.Value, client))
        );
    }

    private object BuildView(string view, IClientProvider client)
    {
        return view switch
        {
            "properties" => BuildPropertiesView(client),
            "bookings" => BuildBookingsView(client),
            "tenants" => BuildTenantsView(client),
            _ => "Select a view"
        };
    }

    private object BuildPropertiesView(IClientProvider client)
    {
        var properties = new[]
        {
            new { Name = "Downtown Apartment", Type = "Apartment", Status = "Available", Price = 1200 },
            new { Name = "Suburban House", Type = "House", Status = "Rented", Price = 2500 },
            new { Name = "Beach Condo", Type = "Condo", Status = "Available", Price = 1800 }
        };

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add("Properties")
                    .Add(new Button("Add Property", _ => client.Toast("Add property"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add(properties.Select(prop => new Card(
                        Layout.Horizontal()
                            .Gap(12)
                            .Padding(16)
                            .Add(Layout.Vertical()
                                .Gap(4)
                                .Add(prop.Name)
                                .Add(prop.Type)
                                .Add(Layout.Horizontal()
                                    .Gap(8)
                                    .Add(new Badge(prop.Status)
                                        .Variant(prop.Status == "Available" ? BadgeVariant.Success : BadgeVariant.Warning))
                                    .Add(new Badge($"${prop.Price}/month")
                                        .Variant(BadgeVariant.Info))))
                            .Add(Layout.Horizontal()
                                .Gap(4)
                                .Add(new Button("View", _ => client.Toast($"Viewing: {prop.Name}"))
                                    .Small()
                                    .Variant(ButtonVariant.Primary))
                                .Add(new Button("Edit", _ => client.Toast($"Editing: {prop.Name}"))
                                    .Small()
                                    .Variant(ButtonVariant.Outline)))
                    ))))
        );
    }

    private object BuildBookingsView(IClientProvider client)
    {
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Bookings")
                .Add("No bookings yet. Create your first booking!")
                .Add(new Button("New Booking", _ => client.Toast("Create booking"))
                    .Variant(ButtonVariant.Primary))
        );
    }

    private object BuildTenantsView(IClientProvider client)
    {
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("Tenants")
                .Add("No tenants yet. Add your first tenant!")
                .Add(new Button("Add Tenant", _ => client.Toast("Add tenant"))
                    .Variant(ButtonVariant.Primary))
        );
    }
}
