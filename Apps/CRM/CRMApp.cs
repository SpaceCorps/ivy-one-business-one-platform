using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.CRM;

[App(icon: Icons.Users, title: "CRM", path: new[] { "Business Operations" })]
public class CRMApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CRMRootBlade(), "CRM", Size.Units(80));
    }
}

public class CRMRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var contacts = context.Contacts.Include(c => c.Opportunities).ToList();
        var leads = context.Leads.ToList();
        var opportunities = context.Opportunities.Include(o => o.Contact).ToList();
        
        var menuItems = new[]
        {
            new ListItem("Contacts",
                subtitle: $"{contacts.Count} total, {contacts.Sum(c => c.Opportunities.Count)} opportunities",
                icon: Icons.Users,
                badge: contacts.Count.ToString(),
                onClick: _ => blades.Push(this, new ContactsBlade(), "Contacts")),
            new ListItem("Leads",
                subtitle: $"{leads.Count} total, {leads.Count(l => l.Status == "Qualified")} qualified",
                icon: Icons.UserPlus,
                badge: leads.Count.ToString(),
                onClick: _ => blades.Push(this, new LeadsBlade(), "Leads")),
            new ListItem("Opportunities",
                subtitle: $"${opportunities.Sum(o => o.Amount):N2} pipeline value",
                icon: Icons.TrendingUp,
                badge: opportunities.Count.ToString(),
                onClick: _ => blades.Push(this, new OpportunitiesBlade(), "Opportunities"))
        };
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Text.H3("CRM Dashboard"))
            .Add(Layout.Grid()
                .Columns(3)
                .Gap(12)
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Contacts").Add(contacts.Count.ToString())))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Leads").Add(leads.Count.ToString())))
                .Add(new Card(Layout.Vertical().Gap(4).Padding(12).Add("Pipeline").Add($"${opportunities.Sum(o => o.Amount):N2}"))))
            .Add(new List(menuItems));
    }
}

public class ContactsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var contacts = context.Contacts.Include(c => c.Opportunities).OrderByDescending(c => c.CreatedAt).ToList();
        
        var listItems = contacts.Select(contact => new ListItem(
            title: $"{contact.FirstName} {contact.LastName}",
            subtitle: $"{contact.Company} - {contact.JobTitle} - {contact.Opportunities.Count} opportunities",
            icon: Icons.User,
            badge: contact.Email,
            onClick: _ => blades.Push(this, new ContactDetailBlade(contact.Id), $"{contact.FirstName} {contact.LastName}")
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Contacts"))
                .Add(new Button("Add Contact", _ => client.Toast("Add contact"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(contacts.Count == 0 
                ? Text.Block("No contacts found. Add your first contact!")
                : new List(listItems));
    }
}

public class ContactDetailBlade(int contactId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var contact = context.Contacts.Include(c => c.Opportunities).FirstOrDefault(c => c.Id == contactId);
        
        if (contact == null)
            return "Contact not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{contact.FirstName} {contact.LastName}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Company: {contact.Company}")
                    .Add($"Job Title: {contact.JobTitle}")
                    .Add($"Email: {contact.Email}")
                    .Add($"Phone: {contact.Phone}")
                    .Add($"Address: {contact.Address}, {contact.City}, {contact.State} {contact.ZipCode}")
                    .Add(new Badge($"{contact.Opportunities.Count} opportunities").Variant(BadgeVariant.Info)))
                .Add(contact.Notes != string.Empty ? $"Notes: {contact.Notes}" : null)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit contact"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Add Opportunity", _ => client.Toast("Create opportunity"))
                        .Variant(ButtonVariant.Success)))
        );
    }
}

public class LeadsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var leads = context.Leads.OrderByDescending(l => l.CreatedAt).ToList();
        
        var listItems = leads.Select(lead => new ListItem(
            title: $"{lead.FirstName} {lead.LastName}",
            subtitle: $"{lead.Company} - {lead.JobTitle} - ${lead.EstimatedValue:N2}",
            icon: Icons.UserPlus,
            badge: lead.Status,
            onClick: _ => blades.Push(this, new LeadDetailBlade(lead.Id), $"{lead.FirstName} {lead.LastName}")
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Leads"))
                .Add(new Button("Add Lead", _ => client.Toast("Add lead"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(leads.Count == 0 
                ? Text.Block("No leads found. Add your first lead!")
                : new List(listItems));
    }
}

public class LeadDetailBlade(int leadId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var lead = context.Leads.FirstOrDefault(l => l.Id == leadId);
        
        if (lead == null)
            return "Lead not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add($"{lead.FirstName} {lead.LastName}")
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Company: {lead.Company}")
                    .Add($"Job Title: {lead.JobTitle}")
                    .Add($"Email: {lead.Email}")
                    .Add($"Phone: {lead.Phone}")
                    .Add($"Source: {lead.Source}")
                    .Add($"Estimated Value: ${lead.EstimatedValue:N2}")
                    .Add(new Badge(lead.Status)
                        .Variant(lead.Status == "Qualified" ? BadgeVariant.Success :
                               lead.Status == "Converted" ? BadgeVariant.Info :
                               BadgeVariant.Secondary)))
                .Add(lead.Notes != string.Empty ? $"Notes: {lead.Notes}" : null)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit lead"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Convert to Contact", _ => client.Toast("Convert lead"))
                        .Variant(ButtonVariant.Success)))
        );
    }
}

public class OpportunitiesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var opportunities = context.Opportunities.Include(o => o.Contact).OrderByDescending(o => o.Amount).ToList();
        
        var listItems = opportunities.Select(opp => new ListItem(
            title: opp.Name,
            subtitle: $"{opp.Contact.FirstName} {opp.Contact.LastName} - ${opp.Amount:N2} - {opp.Probability}%",
            icon: Icons.TrendingUp,
            badge: opp.Stage,
            onClick: _ => blades.Push(this, new OpportunityDetailBlade(opp.Id), opp.Name)
        ));
        
        return Layout.Vertical()
            .Gap(16)
            .Padding(24)
            .Add(Layout.Horizontal()
                .Gap(12)
                .Add(Text.H3("Opportunities"))
                .Add(new Button("Add Opportunity", _ => client.Toast("Add opportunity"))
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)))
            .Add(opportunities.Count == 0 
                ? Text.Block("No opportunities found. Add your first opportunity!")
                : new List(listItems));
    }
}

public class OpportunityDetailBlade(int opportunityId) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var opportunity = context.Opportunities.Include(o => o.Contact).FirstOrDefault(o => o.Id == opportunityId);
        
        if (opportunity == null)
            return "Opportunity not found";
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(16)
                .Add(opportunity.Name)
                .Add(Layout.Vertical()
                    .Gap(8)
                    .Add($"Contact: {opportunity.Contact.FirstName} {opportunity.Contact.LastName}")
                    .Add($"Amount: ${opportunity.Amount:N2}")
                    .Add($"Probability: {opportunity.Probability}%")
                    .Add($"Expected Close: {opportunity.ExpectedCloseDate:MMM dd, yyyy}")
                    .Add($"Description: {opportunity.Description}")
                    .Add(new Badge(opportunity.Stage)
                        .Variant(opportunity.Stage == "Closed Won" ? BadgeVariant.Success :
                               opportunity.Stage == "Closed Lost" ? BadgeVariant.Destructive :
                               BadgeVariant.Primary)))
                .Add(opportunity.Notes != string.Empty ? $"Notes: {opportunity.Notes}" : null)
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Edit", _ => client.Toast("Edit opportunity"))
                        .Variant(ButtonVariant.Primary))
                    .Add(new Button("Mark Won", _ => client.Toast("Mark as won"))
                        .Variant(ButtonVariant.Success)))
        );
    }
}
