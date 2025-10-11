using IvyOneBusinessOnePlatform.Apps.Accounting;
using IvyOneBusinessOnePlatform.Apps.Knowledge;
using IvyOneBusinessOnePlatform.Apps.Sign;
using IvyOneBusinessOnePlatform.Apps.CRM;
using IvyOneBusinessOnePlatform.Apps.Studio;
using IvyOneBusinessOnePlatform.Apps.Subscriptions;
using IvyOneBusinessOnePlatform.Apps.Rental;
using IvyOneBusinessOnePlatform.Apps.PointOfSale;
using IvyOneBusinessOnePlatform.Apps.Discuss;
using IvyOneBusinessOnePlatform.Apps.Documents;
using IvyOneBusinessOnePlatform.Apps.Project;
using IvyOneBusinessOnePlatform.Apps.Timesheets;
using IvyOneBusinessOnePlatform.Apps.FieldService;
using IvyOneBusinessOnePlatform.Apps.Planning;
using IvyOneBusinessOnePlatform.Apps.Helpdesk;
using IvyOneBusinessOnePlatform.Apps.Website;
using IvyOneBusinessOnePlatform.Apps.SocialMarketing;
using IvyOneBusinessOnePlatform.Apps.EmailMarketing;
using IvyOneBusinessOnePlatform.Apps.Purchase;
using IvyOneBusinessOnePlatform.Apps.Inventory;
using IvyOneBusinessOnePlatform.Apps.Manufacturing;
using IvyOneBusinessOnePlatform.Apps.Sales;
using IvyOneBusinessOnePlatform.Apps.HR;
using IvyOneBusinessOnePlatform.Apps.Dashboard;

namespace IvyOneBusinessOnePlatform.Apps;

[App(icon: Icons.PartyPopper, title: "Main Dashboard", path: new[] { "Dashboard" })]
public class MainDashboardApp : ViewBase
{
    private readonly AppInfo[] AppList =
    [
        new("Accounting", Icons.Percent, typeof(AccountingApp)),
        new("Knowledge", Icons.Bookmark, typeof(KnowledgeApp)),
        new("Sign", Icons.Signature, typeof(SignApp)),
        new("CRM", Icons.Users, typeof(CRMApp)),
        new("Studio", Icons.Wrench, typeof(StudioApp)),
        new("Subscriptions", Icons.RefreshCw, typeof(SubscriptionsApp)),
        new("Rental", Icons.Key, typeof(RentalApp)),
        new("Point of Sale", Icons.ShoppingCart, typeof(PointOfSaleApp)),
        new("Discuss", Icons.MessageCircle, typeof(DiscussApp)),
        new("Documents", Icons.FileText, typeof(DocumentsApp)),
        new("Project", Icons.Check, typeof(ProjectApp)),
        new("Timesheets", Icons.Clock, typeof(TimesheetsApp)),
        new("Field Service", Icons.Zap, typeof(FieldServiceApp)),
        new("Planning", Icons.Calendar, typeof(PlanningApp)),
        new("Helpdesk", Icons.Info, typeof(HelpdeskApp)),
        new("Website", Icons.Globe, typeof(WebsiteApp)),
        new("Social Marketing", Icons.Heart, typeof(SocialMarketingApp)),
        new("Email Marketing", Icons.Mail, typeof(EmailMarketingApp)),
        new("Purchase", Icons.ShoppingBag, typeof(PurchaseApp)),
        new("Inventory", Icons.Box, typeof(InventoryApp)),
        new("Manufacturing", Icons.Settings, typeof(ManufacturingApp)),
        new("Sales", Icons.TrendingUp, typeof(SalesApp)),
        new("HR", Icons.User, typeof(HRApp)),
        new("Dashboard", Icons.Grid3x3, typeof(DashboardApp))
    ];

    public override object? Build()
    {
        var navigator = this.UseNavigation();
        
        return new Card(
            Layout.Grid()
                .Columns(6)
                .Gap(4)
                | AppList.Select(app => new Card(
                    Layout.Vertical()
                        .Gap(4)
                        .Add(new Icon(app.Icon))
                        .Add(app.Name)
                ).HandleClick(_ => navigator.Navigate(app.AppType)))
        );
    }

    private record AppInfo(string Name, Icons Icon, Type AppType);
}