namespace IvyOneBusinessOnePlatform.Apps.Rental;

[App(icon: Icons.Key, title: "Rental", path: new[] { "Commerce & Services" })]
public class RentalApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new RentalRootBlade(), "Rental Management");
    }
}

public class RentalRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var blades = this.UseContext<IBladeController>();
        
        var properties = new[]
        {
            new { Name = "Downtown Apartment", Type = "Apartment", Status = "Available", Price = 1200 },
            new { Name = "Suburban House", Type = "House", Status = "Rented", Price = 2500 },
            new { Name = "Beach Condo", Type = "Condo", Status = "Available", Price = 1800 }
        };
        
        var listItems = properties.Select(prop => new ListItem(
            title: prop.Name,
            subtitle: $"{prop.Type} - ${prop.Price}/month",
            icon: Icons.House,
            badge: prop.Status,
            onClick: _ => { blades.Push(this, new PropertyDetailBlade(prop.Name, prop.Type, prop.Status, prop.Price), prop.Name); }
        ));
        
        return BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(12)
                .Add(new Button("Add Property", _ => client.Toast("Add property"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary))
                .Add(new Button("Bookings", _ => blades.Push(this, new BookingsBlade(), "Bookings"))
                    .Variant(ButtonVariant.Secondary)),
            new List(listItems)
        );
    }
}

public class PropertyDetailBlade(string name, string type, string status, int price) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(name)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Type: {type}")
                    .Add($"Price: ${price}/month")
                    .Add(new Badge(status)
                        .Variant(status == "Available" ? BadgeVariant.Success : BadgeVariant.Warning)))
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit property"))
                        .Variant(ButtonVariant.Primary))
                    .Add(status == "Available" 
                        ? new Button("Create Booking", _ => client.Toast("Create booking"))
                            .Variant(ButtonVariant.Success)
                        : null))
        );
    }
}

public class BookingsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
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
}
