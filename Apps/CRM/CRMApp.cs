using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.CRM;

[App(icon: Icons.Users, title: "CRM", path: new[] { "Business Operations" })]
public class CRMApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var selectedView = this.UseState("dashboard");
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Customer Relationship Management")
                .Add("Manage contacts, leads, and sales opportunities")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Dashboard", _ => selectedView.Set("dashboard"))
                        .Variant(selectedView.Value == "dashboard" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Contacts", _ => selectedView.Set("contacts"))
                        .Variant(selectedView.Value == "contacts" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Leads", _ => selectedView.Set("leads"))
                        .Variant(selectedView.Value == "leads" ? ButtonVariant.Primary : ButtonVariant.Secondary))
                    .Add(new Button("Opportunities", _ => selectedView.Set("opportunities"))
                        .Variant(selectedView.Value == "opportunities" ? ButtonVariant.Primary : ButtonVariant.Secondary)))
                .Add(BuildSelectedView(selectedView.Value, context, client))
        );
    }

    private object BuildSelectedView(string view, ApplicationDbContext context, IClientProvider client)
    {
        return view switch
        {
            "dashboard" => BuildDashboard(context, client),
            "contacts" => BuildContactsView(context, client),
            "leads" => BuildLeadsView(context, client),
            "opportunities" => BuildOpportunitiesView(context, client),
            _ => "Select a view"
        };
    }

    private object BuildDashboard(ApplicationDbContext context, IClientProvider client)
    {
        var contacts = context.Contacts.ToList();
        var leads = context.Leads.ToList();
        var opportunities = context.Opportunities.Include(o => o.Contact).ToList();

        var stats = new
        {
            TotalContacts = contacts.Count,
            TotalLeads = leads.Count,
            QualifiedLeads = leads.Count(l => l.Status == "Qualified"),
            TotalOpportunities = opportunities.Count,
            OpenOpportunities = opportunities.Count(o => o.Stage != "Closed Won" && o.Stage != "Closed Lost"),
            PipelineValue = opportunities.Where(o => o.Stage != "Closed Won" && o.Stage != "Closed Lost").Sum(o => o.Amount),
            WonDeals = opportunities.Count(o => o.Stage == "Closed Won")
        };

        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add("CRM Dashboard")
                .Add(Layout.Grid()
                    .Columns(4)
                    .Gap(16)
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Contacts")
                            .Add(stats.TotalContacts.ToString())
                            .Add("Total in database")))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Leads")
                            .Add($"{stats.TotalLeads} ({stats.QualifiedLeads} qualified)")
                            .Add("Active leads")))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Opportunities")
                            .Add($"{stats.OpenOpportunities} open")
                            .Add($"${stats.PipelineValue:N2} in pipeline")))
                    .Add(new Card(
                        Layout.Vertical()
                            .Gap(8)
                            .Padding(16)
                            .Add("Won Deals")
                            .Add(stats.WonDeals.ToString())
                            .Add("Closed successfully"))))
        );
    }

    private object BuildContactsView(ApplicationDbContext context, IClientProvider client)
    {
        var contacts = context.Contacts
            .Include(c => c.Opportunities)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Contacts ({contacts.Count})")
                    .Add(new Button("Add Contact", _ => client.Toast("Add contact form coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(contacts.Count == 0 
                    ? "No contacts found. Add your first contact!"
                    : BuildContactsList(contacts, client))
        );
    }

    private object BuildLeadsView(ApplicationDbContext context, IClientProvider client)
    {
        var leads = context.Leads
            .OrderByDescending(l => l.CreatedAt)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Leads ({leads.Count})")
                    .Add(new Button("Add Lead", _ => client.Toast("Add lead form coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(leads.Count == 0 
                    ? "No leads found. Add your first lead!"
                    : BuildLeadsList(leads, client))
        );
    }

    private object BuildOpportunitiesView(ApplicationDbContext context, IClientProvider client)
    {
        var opportunities = context.Opportunities
            .Include(o => o.Contact)
            .OrderByDescending(o => o.Amount)
            .ToList();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add($"Opportunities ({opportunities.Count})")
                    .Add(new Button("Add Opportunity", _ => client.Toast("Add opportunity form coming soon!"))
                        .Icon(Icons.Plus)
                        .Variant(ButtonVariant.Primary)))
                .Add(opportunities.Count == 0 
                    ? "No opportunities found. Add your first opportunity!"
                    : BuildOpportunitiesList(opportunities, client))
        );
    }

    private object BuildContactsList(List<Contact> contacts, IClientProvider client)
    {
        var contactCards = contacts.Select(contact => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{contact.FirstName} {contact.LastName}")
                    .Add(contact.Company)
                    .Add(contact.JobTitle)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(contact.Email)
                            .Variant(BadgeVariant.Secondary)
                            .Icon(Icons.Mail))
                        .Add(contact.Phone != string.Empty 
                            ? new Badge(contact.Phone)
                                .Variant(BadgeVariant.Outline)
                                .Icon(Icons.Phone)
                            : null)
                        .Add(new Badge($"{contact.Opportunities.Count} opportunities")
                            .Variant(BadgeVariant.Info))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(contact.CreatedAt.ToString("MMM dd, yyyy"))
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {contact.FirstName} {contact.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Edit", _ => client.Toast($"Editing: {contact.FirstName} {contact.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));

        return Layout.Vertical().Gap(8).Add(contactCards);
    }

    private object BuildLeadsList(List<Lead> leads, IClientProvider client)
    {
        var leadCards = leads.Select(lead => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"{lead.FirstName} {lead.LastName}")
                    .Add($"{lead.Company} - {lead.JobTitle}")
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(lead.Status)
                            .Variant(lead.Status == "Qualified" ? BadgeVariant.Success : 
                                   lead.Status == "Converted" ? BadgeVariant.Info :
                                   lead.Status == "Lost" ? BadgeVariant.Destructive :
                                   BadgeVariant.Secondary))
                        .Add(new Badge(lead.Source)
                            .Variant(BadgeVariant.Outline))
                        .Add(lead.EstimatedValue > 0 
                            ? new Badge($"${lead.EstimatedValue:N2}")
                                .Variant(BadgeVariant.Warning)
                            : null)))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(lead.CreatedAt.ToString("MMM dd, yyyy"))
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("Convert", _ => client.Toast($"Converting lead: {lead.FirstName} {lead.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Edit", _ => client.Toast($"Editing: {lead.FirstName} {lead.LastName}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));

        return Layout.Vertical().Gap(8).Add(leadCards);
    }

    private object BuildOpportunitiesList(List<Opportunity> opportunities, IClientProvider client)
    {
        var opportunityCards = opportunities.Select(opportunity => new Card(
            Layout.Horizontal()
                .Gap(12)
                .Padding(16)
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add(opportunity.Name)
                    .Add($"Contact: {opportunity.Contact.FirstName} {opportunity.Contact.LastName}")
                    .Add(opportunity.Description)
                    .Add(Layout.Horizontal()
                        .Gap(8)
                        .Add(new Badge(opportunity.Stage)
                            .Variant(opportunity.Stage == "Closed Won" ? BadgeVariant.Success : 
                                   opportunity.Stage == "Closed Lost" ? BadgeVariant.Destructive :
                                   opportunity.Stage == "Negotiation" ? BadgeVariant.Warning :
                                   BadgeVariant.Secondary))
                        .Add(new Badge($"${opportunity.Amount:N2}")
                            .Variant(BadgeVariant.Info))
                        .Add(new Badge($"{opportunity.Probability}% probability")
                            .Variant(BadgeVariant.Outline))))
                .Add(Layout.Vertical()
                    .Gap(4)
                    .Add($"Close: {opportunity.ExpectedCloseDate:MMM dd, yyyy}")
                    .Add(Layout.Horizontal()
                        .Gap(4)
                        .Add(new Button("View", _ => client.Toast($"Viewing: {opportunity.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Primary))
                        .Add(new Button("Edit", _ => client.Toast($"Editing: {opportunity.Name}"))
                            .Small()
                            .Variant(ButtonVariant.Outline))))
        ));

        return Layout.Vertical().Gap(8).Add(opportunityCards);
    }
}
