namespace IvyOneBusinessOnePlatform.Apps;

[App(icon: Icons.PartyPopper, title: "Main Dashboard", path: new[] { "Dashboard" })]
public class MainDashboardApp : ViewBase
{
    private readonly AppInfo[] AppList =
    [
        new("Accounting", Icons.Percent),
        new("Knowledge", Icons.Bookmark),
        new("Sign", Icons.Signature),
        new("CRM", Icons.Users),
        new("Studio", Icons.Wrench),
        new("Subscriptions", Icons.RefreshCw),
        new("Rental", Icons.Key),
        new("Point of Sale", Icons.ShoppingCart),
        new("Discuss", Icons.MessageCircle),
        new("Documents", Icons.FileText),
        new("Project", Icons.Check),
        new("Timesheets", Icons.Clock),
        new("Field Service", Icons.Zap),
        new("Planning", Icons.Calendar),
        new("Helpdesk", Icons.Info),
        new("Website", Icons.Globe),
        new("Social Marketing", Icons.Heart),
        new("Email Marketing", Icons.Mail),
        new("Purchase", Icons.ShoppingBag),
        new("Inventory", Icons.Box),
        new("Manufacturing", Icons.Settings),
        new("Sales", Icons.TrendingUp),
        new("HR", Icons.User),
        new("Dashboard", Icons.Grid3x3)
    ];

    public override object? Build()
    {
        return new Card(
            Layout.Grid()
                .Columns(6)
                .Gap(16)
                .Padding(24)
                | AppList.Select(app => new Card(
                    Layout.Vertical()
                        .Gap(12)
                        .Padding(16)
                        .Add(new Icon(app.Icon))
                        .Add(app.Name)
                ))
        );
    }

    private record AppInfo(string Name, Icons Icon);
}