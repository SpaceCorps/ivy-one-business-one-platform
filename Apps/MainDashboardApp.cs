namespace IvyOneBusinessOnePlatform.Apps;

[App(icon: Icons.PartyPopper, title: "Main Dashboard")]
public class MainDashboardApp : ViewBase
{
    private string[] AppList =
    [
        "Accounting", "Knowledge", "Sign", "CRM", "Studio", "Subscriptions",
        "Rental", "Point of Sale", "Discuss", "Documents", "Project", "Timesheets",
        "Field Service", "Planning", "HelpDesk", "Website", "Social Marketing", "Email Marketing",
        "Purchase", "Inventory", "Manufacturing", "Sales", "HR", "Dashboard"
    ];

    public override object? Build()
    {
        return new Card(
            Layout.Grid()
                .Columns(4)
                .Gap(8)
                .Padding(16)
                | AppList.Select(el => new Button(el))
        );
    }
}