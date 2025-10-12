using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.CRM;

[App(icon: Icons.Users, title: "CRM", path: new[] { "Business Operations" })]
public class CRMApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new CRMRootBlade(), "CRM", Size.Units(110));
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
        var searchQuery = this.UseState("");
        var isNewContactOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Contacts.Include(c => c.Opportunities).AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(c => 
                EF.Functions.Like(c.FirstName, searchPattern) ||
                EF.Functions.Like(c.LastName, searchPattern) ||
                EF.Functions.Like(c.Email, searchPattern) ||
                EF.Functions.Like(c.Company, searchPattern) ||
                EF.Functions.Like(c.JobTitle, searchPattern));
        }
        
        var contacts = query.OrderByDescending(c => c.CreatedAt).ToList();
        
        var listItems = contacts.Select(contact => new ListItem(
            title: $"{contact.FirstName} {contact.LastName}",
            subtitle: $"{contact.Company} - {contact.JobTitle} - {contact.Opportunities.Count} opportunities",
            icon: Icons.User,
            badge: contact.Email,
            onClick: _ => blades.Push(this, new ContactDetailBlade(contact.Id, () => refreshToken.Refresh()), $"{contact.FirstName} {contact.LastName}")
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by name, email, company, or job title..."))
                .Add(new Button("Add Contact")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewContactOpen.Set(true))),
            contacts.Count == 0 
                ? Text.Block("No contacts found. Try a different search or add your first contact!")
                : new List(listItems)
        );

        return isNewContactOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewContactOpen.Set(false),
            new ContactFormSheet(null, () => {
                isNewContactOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Contact",
            description: "Add a new contact to your CRM"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class ContactDetailBlade(int contactId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialContact = context.Contacts.Include(c => c.Opportunities).FirstOrDefault(c => c.Id == contactId);
        
        if (initialContact == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Contact Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var contactData = this.UseState(initialContact);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        // Refresh contact data when refresh token changes
        this.UseEffect(() =>
        {
            var updatedContact = context.Contacts.Include(c => c.Opportunities).FirstOrDefault(c => c.Id == contactId);
            if (updatedContact != null)
            {
                contactData.Set(updatedContact);
            }
        }, [refreshToken.ToTrigger()]);
        
        var contactDetails = new
        {
            Name = $"{contactData.Value.FirstName} {contactData.Value.LastName}",
            Company = contactData.Value.Company,
            JobTitle = contactData.Value.JobTitle,
            Email = contactData.Value.Email,
            Phone = contactData.Value.Phone,
            Address = $"{contactData.Value.Address}, {contactData.Value.City}, {contactData.Value.State} {contactData.Value.ZipCode}",
            Country = contactData.Value.Country,
            OpportunitiesCount = $"{contactData.Value.Opportunities.Count} opportunities",
            Notes = contactData.Value.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{contactData.Value.FirstName} {contactData.Value.LastName}"))
            .Add(contactDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes).MultiLine(x => x.Address))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Edit Contact")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Contact")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var contactToDelete = context.Contacts.FirstOrDefault(c => c.Id == contactId);
                            if (contactToDelete != null)
                            {
                                context.Contacts.Remove(contactToDelete);
                                context.SaveChanges();
                                client.Toast($"Contact {contactData.Value.FirstName} {contactData.Value.LastName} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting contact: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new ContactFormSheet(contactId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Contact",
                description: $"Edit contact {contactData.Value.FirstName} {contactData.Value.LastName}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class LeadsBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewLeadOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Leads.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(l => 
                EF.Functions.Like(l.FirstName, searchPattern) ||
                EF.Functions.Like(l.LastName, searchPattern) ||
                EF.Functions.Like(l.Email, searchPattern) ||
                EF.Functions.Like(l.Company, searchPattern) ||
                EF.Functions.Like(l.Status, searchPattern) ||
                EF.Functions.Like(l.Source, searchPattern));
        }
        
        var leads = query.OrderByDescending(l => l.CreatedAt).ToList();
        
        var listItems = leads.Select(lead => new ListItem(
            title: $"{lead.FirstName} {lead.LastName}",
            subtitle: $"{lead.Company} - {lead.JobTitle} - ${lead.EstimatedValue:N2}",
            icon: Icons.UserPlus,
            badge: lead.Status,
            onClick: _ => blades.Push(this, new LeadDetailBlade(lead.Id, () => refreshToken.Refresh()), $"{lead.FirstName} {lead.LastName}")
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by name, email, company, status, or source..."))
                .Add(new Button("Add Lead")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewLeadOpen.Set(true))),
            leads.Count == 0 
                ? Text.Block("No leads found. Try a different search or add your first lead!")
                : new List(listItems)
        );

        return isNewLeadOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewLeadOpen.Set(false),
            new LeadFormSheet(null, () => {
                isNewLeadOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Lead",
            description: "Add a new lead to your CRM"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class LeadDetailBlade(int leadId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialLead = context.Leads.FirstOrDefault(l => l.Id == leadId);
        
        if (initialLead == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Lead Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var leadData = this.UseState(initialLead);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedLead = context.Leads.FirstOrDefault(l => l.Id == leadId);
            if (updatedLead != null)
            {
                leadData.Set(updatedLead);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(leadData.Value.Status)
            .Variant(leadData.Value.Status == "Qualified" ? BadgeVariant.Success :
                   leadData.Value.Status == "Converted" ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);

        var leadDetails = new
        {
            Name = $"{leadData.Value.FirstName} {leadData.Value.LastName}",
            Company = leadData.Value.Company,
            JobTitle = leadData.Value.JobTitle,
            Email = leadData.Value.Email,
            Phone = leadData.Value.Phone,
            Source = leadData.Value.Source,
            EstimatedValue = $"${leadData.Value.EstimatedValue:N2}",
            Status = statusBadge,
            Notes = leadData.Value.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3($"{leadData.Value.FirstName} {leadData.Value.LastName}"))
            .Add(leadDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(new Button("Qualify Lead", _ => {
                    var dbLead = context.Leads.FirstOrDefault(l => l.Id == leadId);
                    if (dbLead != null)
                    {
                        dbLead.Status = "Qualified";
                        dbLead.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Lead {leadData.Value.FirstName} {leadData.Value.LastName} qualified!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check))
                .Add(new Button("Edit Lead")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Lead")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var leadToDelete = context.Leads.FirstOrDefault(l => l.Id == leadId);
                            if (leadToDelete != null)
                            {
                                context.Leads.Remove(leadToDelete);
                                context.SaveChanges();
                                client.Toast($"Lead {leadData.Value.FirstName} {leadData.Value.LastName} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting lead: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new LeadFormSheet(leadId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Lead",
                description: $"Edit lead {leadData.Value.FirstName} {leadData.Value.LastName}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class OpportunitiesBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewOpportunityOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Opportunities.Include(o => o.Contact).AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(o => 
                EF.Functions.Like(o.Name, searchPattern) ||
                EF.Functions.Like(o.Stage, searchPattern) ||
                EF.Functions.Like(o.Contact.FirstName, searchPattern) ||
                EF.Functions.Like(o.Contact.LastName, searchPattern));
        }
        
        var opportunities = query.OrderByDescending(o => o.Amount).ToList();
        
        var listItems = opportunities.Select(opp => new ListItem(
            title: opp.Name,
            subtitle: $"{opp.Contact.FirstName} {opp.Contact.LastName} - ${opp.Amount:N2} - {opp.Probability}%",
            icon: Icons.TrendingUp,
            badge: opp.Stage,
            onClick: _ => blades.Push(this, new OpportunityDetailBlade(opp.Id, () => refreshToken.Refresh()), opp.Name)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search by name, stage, or contact name..."))
                .Add(new Button("Add Opportunity")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewOpportunityOpen.Set(true))),
            opportunities.Count == 0 
                ? Text.Block("No opportunities found. Try a different search or add your first opportunity!")
                : new List(listItems)
        );

        return isNewOpportunityOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewOpportunityOpen.Set(false),
            new OpportunityFormSheet(null, () => {
                isNewOpportunityOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Opportunity",
            description: "Add a new sales opportunity"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class OpportunityDetailBlade(int opportunityId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialOpportunity = context.Opportunities.Include(o => o.Contact).FirstOrDefault(o => o.Id == opportunityId);
        
        if (initialOpportunity == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Opportunity Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var opportunityData = this.UseState(initialOpportunity);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedOpportunity = context.Opportunities.Include(o => o.Contact).FirstOrDefault(o => o.Id == opportunityId);
            if (updatedOpportunity != null)
            {
                opportunityData.Set(updatedOpportunity);
            }
        }, [refreshToken.ToTrigger()]);
        
        var stageBadge = new Badge(opportunityData.Value.Stage)
            .Variant(opportunityData.Value.Stage == "Closed Won" ? BadgeVariant.Success :
                   opportunityData.Value.Stage == "Closed Lost" ? BadgeVariant.Destructive :
                   BadgeVariant.Primary);

        var opportunityDetails = new
        {
            Name = opportunityData.Value.Name,
            Contact = $"{opportunityData.Value.Contact.FirstName} {opportunityData.Value.Contact.LastName}",
            Amount = $"${opportunityData.Value.Amount:N2}",
            Probability = $"{opportunityData.Value.Probability}%",
            ExpectedCloseDate = opportunityData.Value.ExpectedCloseDate.ToString("MMM dd, yyyy"),
            Description = opportunityData.Value.Description,
            Stage = stageBadge,
            Notes = opportunityData.Value.Notes
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(opportunityData.Value.Name))
            .Add(opportunityDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description).MultiLine(x => x.Notes))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(opportunityData.Value.Stage != "Closed Won" ? new Button("Mark as Won", _ => {
                    var dbOpportunity = context.Opportunities.FirstOrDefault(o => o.Id == opportunityId);
                    if (dbOpportunity != null)
                    {
                        dbOpportunity.Stage = "Closed Won";
                        dbOpportunity.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Opportunity {opportunityData.Value.Name} marked as won!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Check)
                    : null)
                .Add(opportunityData.Value.Stage != "Closed Lost" ? new Button("Mark as Lost", _ => {
                    var dbOpportunity = context.Opportunities.FirstOrDefault(o => o.Id == opportunityId);
                    if (dbOpportunity != null)
                    {
                        dbOpportunity.Stage = "Closed Lost";
                        dbOpportunity.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Opportunity {opportunityData.Value.Name} marked as lost!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.X)
                    : null)
                .Add(new Button("Edit Opportunity")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Opportunity")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var opportunityToDelete = context.Opportunities.FirstOrDefault(o => o.Id == opportunityId);
                            if (opportunityToDelete != null)
                            {
                                context.Opportunities.Remove(opportunityToDelete);
                                context.SaveChanges();
                                client.Toast($"Opportunity {opportunityData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting opportunity: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new OpportunityFormSheet(opportunityId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Opportunity",
                description: $"Edit opportunity {opportunityData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class ContactFormSheet(int? contactId = null, Action? onClose = null) : ViewBase
{
    private Contact CloneContact(Contact source) => new Contact
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Email = source.Email,
        Phone = source.Phone,
        Company = source.Company,
        JobTitle = source.JobTitle,
        Address = source.Address,
        City = source.City,
        State = source.State,
        ZipCode = source.ZipCode,
        Country = source.Country,
        Notes = source.Notes,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = contactId.HasValue;
        var existingContact = isEdit ? context.Contacts.FirstOrDefault(c => c.Id == contactId!.Value) : null;
        
        var contactForm = this.UseState(existingContact ?? new Contact
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Phone = "",
            Company = "",
            JobTitle = "",
            Address = "",
            City = "",
            State = "",
            ZipCode = "",
            Country = "",
            Notes = ""
        });
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(contactForm.Value.FirstName))
                            {
                                client.Toast("First Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(contactForm.Value.LastName))
                            {
                                client.Toast("Last Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(contactForm.Value.Email))
                            {
                                client.Toast("Email is required", "Validation Error");
                                return;
                            }
                            
                            if (contactForm.Value.FirstName.Length > 100 || contactForm.Value.LastName.Length > 100)
                            {
                                client.Toast("First Name and Last Name cannot exceed 100 characters", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingContact != null)
                            {
                                existingContact.FirstName = contactForm.Value.FirstName;
                                existingContact.LastName = contactForm.Value.LastName;
                                existingContact.Email = contactForm.Value.Email;
                                existingContact.Phone = contactForm.Value.Phone;
                                existingContact.Company = contactForm.Value.Company;
                                existingContact.JobTitle = contactForm.Value.JobTitle;
                                existingContact.Address = contactForm.Value.Address;
                                existingContact.City = contactForm.Value.City;
                                existingContact.State = contactForm.Value.State;
                                existingContact.ZipCode = contactForm.Value.ZipCode;
                                existingContact.Country = contactForm.Value.Country;
                                existingContact.Notes = contactForm.Value.Notes;
                                existingContact.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newContact = new Contact
                                {
                                    FirstName = contactForm.Value.FirstName,
                                    LastName = contactForm.Value.LastName,
                                    Email = contactForm.Value.Email,
                                    Phone = contactForm.Value.Phone,
                                    Company = contactForm.Value.Company,
                                    JobTitle = contactForm.Value.JobTitle,
                                    Address = contactForm.Value.Address,
                                    City = contactForm.Value.City,
                                    State = contactForm.Value.State,
                                    ZipCode = contactForm.Value.ZipCode,
                                    Country = contactForm.Value.Country,
                                    Notes = contactForm.Value.Notes,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Contacts.Add(newContact);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Contact updated successfully!" : "Contact created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Personal Information"))
                        .Add(Text.Small("First Name"))
                        .Add(new TextInput(contactForm.Value.FirstName, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.FirstName = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("John"))
                        .Add(Text.Small("Last Name"))
                        .Add(new TextInput(contactForm.Value.LastName, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.LastName = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("Doe"))
                        .Add(Text.Small("Email"))
                        .Add(new TextInput(contactForm.Value.Email, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Email = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("john@example.com"))
                        .Add(Text.Small("Phone"))
                        .Add(new TextInput(contactForm.Value.Phone, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Phone = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("+1 (555) 123-4567"))
                ).Title("Contact Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Company Information"))
                        .Add(Text.Small("Company"))
                        .Add(new TextInput(contactForm.Value.Company, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Company = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("Acme Corp"))
                        .Add(Text.Small("Job Title"))
                        .Add(new TextInput(contactForm.Value.JobTitle, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.JobTitle = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("Sales Manager"))
                ).Title("Company"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Address Information"))
                        .Add(Text.Small("Address"))
                        .Add(new TextInput(contactForm.Value.Address, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Address = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("123 Main St"))
                        .Add(Text.Small("City"))
                        .Add(new TextInput(contactForm.Value.City, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.City = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("New York"))
                        .Add(Text.Small("State"))
                        .Add(new TextInput(contactForm.Value.State, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.State = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("NY"))
                        .Add(Text.Small("Zip Code"))
                        .Add(new TextInput(contactForm.Value.ZipCode, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.ZipCode = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("10001"))
                        .Add(Text.Small("Country"))
                        .Add(new TextInput(contactForm.Value.Country, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Country = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("USA"))
                ).Title("Address"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(contactForm.Value.Notes, e => {
                            var cloned = CloneContact(contactForm.Value);
                            cloned.Notes = e.Value;
                            contactForm.Set(cloned);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Notes"))
        );
    }
}

public class LeadFormSheet(int? leadId = null, Action? onClose = null) : ViewBase
{
    private Lead CloneLead(Lead source) => new Lead
    {
        Id = source.Id,
        FirstName = source.FirstName,
        LastName = source.LastName,
        Email = source.Email,
        Phone = source.Phone,
        Company = source.Company,
        JobTitle = source.JobTitle,
        Source = source.Source,
        Status = source.Status,
        EstimatedValue = source.EstimatedValue,
        Notes = source.Notes,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = leadId.HasValue;
        var existingLead = isEdit ? context.Leads.FirstOrDefault(l => l.Id == leadId!.Value) : null;
        
        var leadForm = this.UseState(existingLead ?? new Lead
        {
            FirstName = "",
            LastName = "",
            Email = "",
            Phone = "",
            Company = "",
            JobTitle = "",
            Source = "",
            Status = "New",
            EstimatedValue = 0.00m,
            Notes = ""
        });
        
        var statusOptions = new[] { "New", "Qualified", "Contacted", "Converted", "Lost" };
        var sourceOptions = new[] { "Website", "Referral", "Cold Call", "Email", "Social Media", "Event", "Other" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(leadForm.Value.FirstName))
                            {
                                client.Toast("First Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(leadForm.Value.LastName))
                            {
                                client.Toast("Last Name is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(leadForm.Value.Email))
                            {
                                client.Toast("Email is required", "Validation Error");
                                return;
                            }
                            
                            if (leadForm.Value.EstimatedValue < 0)
                            {
                                client.Toast("Estimated Value cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingLead != null)
                            {
                                existingLead.FirstName = leadForm.Value.FirstName;
                                existingLead.LastName = leadForm.Value.LastName;
                                existingLead.Email = leadForm.Value.Email;
                                existingLead.Phone = leadForm.Value.Phone;
                                existingLead.Company = leadForm.Value.Company;
                                existingLead.JobTitle = leadForm.Value.JobTitle;
                                existingLead.Source = leadForm.Value.Source;
                                existingLead.Status = leadForm.Value.Status;
                                existingLead.EstimatedValue = leadForm.Value.EstimatedValue;
                                existingLead.Notes = leadForm.Value.Notes;
                                existingLead.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newLead = new Lead
                                {
                                    FirstName = leadForm.Value.FirstName,
                                    LastName = leadForm.Value.LastName,
                                    Email = leadForm.Value.Email,
                                    Phone = leadForm.Value.Phone,
                                    Company = leadForm.Value.Company,
                                    JobTitle = leadForm.Value.JobTitle,
                                    Source = leadForm.Value.Source,
                                    Status = leadForm.Value.Status,
                                    EstimatedValue = leadForm.Value.EstimatedValue,
                                    Notes = leadForm.Value.Notes,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Leads.Add(newLead);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Lead updated successfully!" : "Lead created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Lead Information"))
                        .Add(Text.Small("First Name"))
                        .Add(new TextInput(leadForm.Value.FirstName, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.FirstName = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("John"))
                        .Add(Text.Small("Last Name"))
                        .Add(new TextInput(leadForm.Value.LastName, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.LastName = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("Doe"))
                        .Add(Text.Small("Email"))
                        .Add(new TextInput(leadForm.Value.Email, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Email = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("john@example.com"))
                        .Add(Text.Small("Phone"))
                        .Add(new TextInput(leadForm.Value.Phone, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Phone = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("+1 (555) 123-4567"))
                        .Add(Text.Small("Company"))
                        .Add(new TextInput(leadForm.Value.Company, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Company = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("Acme Corp"))
                        .Add(Text.Small("Job Title"))
                        .Add(new TextInput(leadForm.Value.JobTitle, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.JobTitle = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("CEO"))
                ).Title("Lead Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Lead Tracking"))
                        .Add(Text.Small("Source"))
                        .Add(new SelectInput<string>(leadForm.Value.Source, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Source = e.Value;
                            leadForm.Set(cloned);
                        }, sourceOptions.ToOptions()))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(leadForm.Value.Status, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Status = e.Value;
                            leadForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                        .Add(Text.Small("Estimated Value ($)"))
                        .Add(new NumberInput<decimal>(leadForm.Value.EstimatedValue, v => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.EstimatedValue = v;
                            leadForm.Set(cloned);
                        }).Placeholder("0.00"))
                ).Title("Lead Status"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(leadForm.Value.Notes, e => {
                            var cloned = CloneLead(leadForm.Value);
                            cloned.Notes = e.Value;
                            leadForm.Set(cloned);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Notes"))
        );
    }
}

public class OpportunityFormSheet(int? opportunityId = null, Action? onClose = null) : ViewBase
{
    private Opportunity CloneOpportunity(Opportunity source) => new Opportunity
    {
        Id = source.Id,
        ContactId = source.ContactId,
        Name = source.Name,
        Description = source.Description,
        Amount = source.Amount,
        Stage = source.Stage,
        Probability = source.Probability,
        ExpectedCloseDate = source.ExpectedCloseDate,
        Notes = source.Notes,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = opportunityId.HasValue;
        var existingOpportunity = isEdit ? context.Opportunities.FirstOrDefault(o => o.Id == opportunityId!.Value) : null;
        
        var contacts = context.Contacts.OrderBy(c => c.LastName).ToList();
        
        var opportunityForm = this.UseState(existingOpportunity ?? new Opportunity
        {
            ContactId = contacts.FirstOrDefault()?.Id ?? 0,
            Name = "",
            Description = "",
            Amount = 0.00m,
            Stage = "Prospecting",
            Probability = 50,
            ExpectedCloseDate = DateTime.UtcNow.AddDays(30),
            Notes = ""
        });
        
        var stageOptions = new[] { "Prospecting", "Qualification", "Proposal", "Negotiation", "Closed Won", "Closed Lost" };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (opportunityForm.Value.ContactId == 0)
                            {
                                client.Toast("Contact is required", "Validation Error");
                                return;
                            }
                            
                            if (string.IsNullOrWhiteSpace(opportunityForm.Value.Name))
                            {
                                client.Toast("Opportunity Name is required", "Validation Error");
                                return;
                            }
                            
                            if (opportunityForm.Value.Amount < 0)
                            {
                                client.Toast("Amount cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (opportunityForm.Value.Probability < 0 || opportunityForm.Value.Probability > 100)
                            {
                                client.Toast("Probability must be between 0 and 100", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingOpportunity != null)
                            {
                                existingOpportunity.ContactId = opportunityForm.Value.ContactId;
                                existingOpportunity.Name = opportunityForm.Value.Name;
                                existingOpportunity.Description = opportunityForm.Value.Description;
                                existingOpportunity.Amount = opportunityForm.Value.Amount;
                                existingOpportunity.Stage = opportunityForm.Value.Stage;
                                existingOpportunity.Probability = opportunityForm.Value.Probability;
                                existingOpportunity.ExpectedCloseDate = opportunityForm.Value.ExpectedCloseDate;
                                existingOpportunity.Notes = opportunityForm.Value.Notes;
                                existingOpportunity.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newOpportunity = new Opportunity
                                {
                                    ContactId = opportunityForm.Value.ContactId,
                                    Name = opportunityForm.Value.Name,
                                    Description = opportunityForm.Value.Description,
                                    Amount = opportunityForm.Value.Amount,
                                    Stage = opportunityForm.Value.Stage,
                                    Probability = opportunityForm.Value.Probability,
                                    ExpectedCloseDate = opportunityForm.Value.ExpectedCloseDate,
                                    Notes = opportunityForm.Value.Notes,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Opportunities.Add(newOpportunity);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Opportunity updated successfully!" : "Opportunity created successfully!");
                            onClose?.Invoke();
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel")
                    .Variant(ButtonVariant.Outline)
                    .HandleClick(_ => onClose?.Invoke())),
            
            Layout.Vertical().Gap(4)
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Opportunity Information"))
                        .Add(Text.Small("Opportunity Name"))
                        .Add(new TextInput(opportunityForm.Value.Name, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Name = e.Value;
                            opportunityForm.Set(cloned);
                        }).Placeholder("Q1 Software License"))
                        .Add(Text.Small("Contact"))
                        .Add(new SelectInput<int>(opportunityForm.Value.ContactId, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.ContactId = e.Value;
                            opportunityForm.Set(cloned);
                        }, contacts.Select(c => (c.Id, $"{c.FirstName} {c.LastName} - {c.Company}")).ToOptions()))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(opportunityForm.Value.Description, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Description = e.Value;
                            opportunityForm.Set(cloned);
                        }).Placeholder("Opportunity description...").Variant(TextInputs.Textarea))
                ).Title("Opportunity Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Sales Information"))
                        .Add(Text.Small("Amount ($)"))
                        .Add(new NumberInput<decimal>(opportunityForm.Value.Amount, v => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Amount = v;
                            opportunityForm.Set(cloned);
                        }).Placeholder("0.00"))
                        .Add(Text.Small("Stage"))
                        .Add(new SelectInput<string>(opportunityForm.Value.Stage, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Stage = e.Value;
                            opportunityForm.Set(cloned);
                        }, stageOptions.ToOptions()))
                        .Add(Text.Small("Probability (%)"))
                        .Add(new NumberInput<int>(opportunityForm.Value.Probability, v => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Probability = v;
                            opportunityForm.Set(cloned);
                        }).Placeholder("50"))
                        .Add(Text.Small("Expected Close Date"))
                        .Add(new DateTimeInput<DateTime>(opportunityForm.Value.ExpectedCloseDate, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.ExpectedCloseDate = e.Value;
                            opportunityForm.Set(cloned);
                        }))
                ).Title("Sales Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Additional Information"))
                        .Add(Text.Small("Notes"))
                        .Add(new TextInput(opportunityForm.Value.Notes, e => {
                            var cloned = CloneOpportunity(opportunityForm.Value);
                            cloned.Notes = e.Value;
                            opportunityForm.Set(cloned);
                        }).Placeholder("Additional notes...").Variant(TextInputs.Textarea))
                ).Title("Notes"))
        );
    }
}
