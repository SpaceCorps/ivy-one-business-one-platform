using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Views.Alerts;
using Ivy.Views.Blades;
using Ivy.Views.Builders;
using Ivy.Views.Forms;
using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.Sales;

[App(icon: Icons.TrendingUp, title: "Sales", path: new[] { "Business Operations" })]
public class SalesApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new OpportunitiesListBlade(), "Search", Size.Units(80));
    }
}

public record OpportunityListRecord(int Id, string Name, string ContactName, decimal Amount, string Stage, int Probability, DateTime ExpectedCloseDate);

public class OpportunitiesListBlade : ViewBase
{
    public override object? Build()
    {
        //This blade will display a list of opportunities - we choose to include the name, contact, amount, stage, probability and expected close date as these are the most relevant fields for sales management.

        var blades = this.UseContext<IBladeController>();
        var factory = this.UseService<ApplicationDbContext>();
        var refreshToken = this.UseRefreshToken();

        this.UseEffect(() =>
        {
            if (refreshToken.ReturnValue is int opportunityId)
            {
                blades.Pop(this, true); // make sure the list has been refreshed
                blades.Push(this, new OpportunityDetailsBlade(opportunityId));
            }
        }, [refreshToken]);

        var onItemClicked = new Action<Event<ListItem>>(e =>
        {
            var opportunity = (OpportunityListRecord)e.Sender.Tag!;
            blades.Push(this, new OpportunityDetailsBlade(opportunity.Id), opportunity.Name, width: Size.Units(100)); // by setting the width we avoid jank when different blades are opened   
        });

        ListItem CreateItem(OpportunityListRecord record) =>
            new(title: record.Name, onClick: onItemClicked, tag: record, subtitle: $"{record.ContactName} - ${record.Amount:N0}");

        var createBtn = new Button("Add Sale", _ =>
        {
            blades.Pop(this); // make sure only the current blade is visible
        }).ToTrigger((isOpen) => new OpportunityCreateDialog(isOpen, refreshToken));

        return new FilteredListView<OpportunityListRecord>(
            fetchRecords: (filter) => FetchOpportunities(factory, filter),
            createItem: CreateItem,
            toolButtons: createBtn,
            onFilterChanged: _ =>
            {
                blades.Pop(this);
            }
        );
    }

    private async Task<OpportunityListRecord[]> FetchOpportunities(ApplicationDbContext db, string filter)
    {
        var linq = db.Opportunities.Include(o => o.Contact).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            linq = linq.Where(e => e.Name.Contains(filter) || e.Contact.FirstName.Contains(filter) || e.Contact.LastName.Contains(filter) || e.Contact.Company.Contains(filter));
        }

        return await linq
            .OrderByDescending(e => e.ExpectedCloseDate)
            .Take(50)
            .Select(e => new OpportunityListRecord(
                e.Id,
                e.Name,
                $"{e.Contact.FirstName} {e.Contact.LastName}".Trim(),
                e.Amount,
                e.Stage,
                e.Probability,
                e.ExpectedCloseDate
            ))
            .ToArrayAsync();
    }
}

public class OpportunityDetailsBlade(int opportunityId) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var refreshToken = this.UseRefreshToken();
        var opportunity = this.UseState<Opportunity?>(() => null!);

        this.UseEffect(async () =>
        {
            opportunity.Set(await db.Opportunities.Include(e => e.Contact).SingleOrDefaultAsync(e => e.Id == opportunityId));
        }, [EffectTrigger.AfterInit(), refreshToken]);

        if (opportunity.Value == null) return new Error("Opportunity not found.");

        var _opportunity = opportunity.Value;

        var deleteBtn = new Button("Delete", onClick: e =>
            {
                Delete(db);
                blades.Pop(refresh: true);
            })
            .Variant(ButtonVariant.Destructive)
            .Icon(Icons.Trash)
            .Width(Size.Grow())
            .WithConfirm($"Are you sure you want to delete opportunity '{_opportunity.Name}'?", "Delete Opportunity");

        var editBtn = new Button("Edit")
            .Variant(ButtonVariant.Outline)
            .Icon(Icons.Pencil)
            .Width(Size.Grow())
            .ToTrigger((isOpen) => new OpportunityEditSheet(isOpen, opportunityId, refreshToken));

        var opportunityCard = new Card(
            content: new
            {
                // We include some of the interesting fields of the entity here
                _opportunity.Id,
                _opportunity.Name,
                _opportunity.Description,
                Contact = $"{_opportunity.Contact.FirstName} {_opportunity.Contact.LastName}".Trim(),
                Company = _opportunity.Contact.Company,
                Amount = _opportunity.Amount.ToString("C"),
                Stage = _opportunity.Stage,
                Probability = $"{_opportunity.Probability}%",
                ExpectedCloseDate = _opportunity.ExpectedCloseDate.ToString("MMM dd, yyyy"),
                Notes = _opportunity.Notes
            }.ToDetails()
            .RemoveEmpty() // Removes "empty" fields from the details
            .Builder(e => e.Id, e => e.CopyToClipboard()),
            footer:
                Layout.Horizontal().Gap(2).Width(Size.Full())
                | deleteBtn
                | editBtn
            ).Title("Opportunity Details");

        return Layout.Vertical().Gap(4) | new object[]
        {
            opportunityCard
        };
    }

    private void Delete(ApplicationDbContext db)
    {
        var opportunity = db.Opportunities.Find(opportunityId)!;
        db.Opportunities.Remove(opportunity);
        db.SaveChanges();
    }
}

// When creating an opportunity we only select the most relevant fields for the user to input - Only truly required fields.
public record OpportunityCreateRequest
{
    [Required]
    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    [Required]
    public decimal Amount { get; init; }

    public string Stage { get; init; } = "Prospecting";

    public int Probability { get; init; } = 25;

    public DateTime ExpectedCloseDate { get; init; } = DateTime.UtcNow.AddDays(30);

    [Required]
    public int ContactId { get; init; }

    public string Notes { get; init; } = "";
}

public class OpportunityCreateDialog(IState<bool> isOpen, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var opportunity = this.UseState(() => new OpportunityCreateRequest());

        this.UseEffect(() =>
        {
            var opportunityId = CreateOpportunity(db, opportunity.Value);
            refreshToken.Refresh(opportunityId);
        }, [opportunity]);

        return opportunity
            .ToForm()
            //We only specify Builder if we want to customize the input control for the field - ToForm() will scaffold the form based on the properties of the record
            //ToAsyncSelectInput allows us to select foreign keys
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(OpportunityHelpers.QueryContacts(db), OpportunityHelpers.LookupContact(db), placeholder: "Select Contact"))
            .Builder(e => e.Stage, e => e.ToSelectInput(new[]
            {
                new Option<string>("Prospecting", "Prospecting"),
                new Option<string>("Qualification", "Qualification"),
                new Option<string>("Proposal", "Proposal"),
                new Option<string>("Negotiation", "Negotiation"),
                new Option<string>("Closed Won", "Closed Won"),
                new Option<string>("Closed Lost", "Closed Lost")
            }, placeholder: "Select Stage"))
            .Builder(e => e.Probability, e => e.ToNumberInput().Min(0).Max(100))
            .ToDialog(isOpen, title: "Create Opportunity", submitTitle: "Create");
    }

    private int CreateOpportunity(ApplicationDbContext db, OpportunityCreateRequest request)
    {
        var id = (db.Opportunities.Max(o => (int?)o.Id) ?? 0) + 1;

        db.Opportunities.Add(new Opportunity()
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Amount = request.Amount,
            Stage = request.Stage,
            Probability = request.Probability,
            ExpectedCloseDate = request.ExpectedCloseDate,
            ContactId = request.ContactId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        db.SaveChanges();

        return id;
    }
}

public class OpportunityEditSheet(IState<bool> isOpen, int id, RefreshToken refreshToken) : ViewBase
{
    public override object? Build()
    {
        var db = this.UseService<ApplicationDbContext>();
        var opportunity = this.UseState(() => db.Opportunities.Find(id));

        // Handle case where opportunity is not found
        if (opportunity.Value == null)
        {
            return new Error("Opportunity not found.");
        }

        this.UseEffect(() =>
        {
            var existingOpportunity = db.Opportunities.Find(id);
            if (existingOpportunity != null && opportunity.Value != null)
            {
                existingOpportunity.Name = opportunity.Value.Name;
                existingOpportunity.Description = opportunity.Value.Description;
                existingOpportunity.Amount = opportunity.Value.Amount;
                existingOpportunity.Stage = opportunity.Value.Stage;
                existingOpportunity.Probability = opportunity.Value.Probability;
                existingOpportunity.ExpectedCloseDate = opportunity.Value.ExpectedCloseDate;
                existingOpportunity.ContactId = opportunity.Value.ContactId;
                existingOpportunity.Notes = opportunity.Value.Notes;
                existingOpportunity.UpdatedAt = DateTime.UtcNow;

                db.SaveChanges();
                refreshToken.Refresh();
            }
        }, [opportunity]);

        return opportunity
            .ToForm()
            // ToForm() will scaffold the form based on the properties of the record and create the appropriate builder for input controls
            // .Build(<expression>, e => To...) will allow us to customize the input control for the field 
            // NOTE! Only use this if you're sure about the syntax, otherwise leave it and let the inputs be scaffolded
            .Builder(e => e.Description, e => e.ToTextAreaInput())
            .Builder(e => e.Notes, e => e.ToTextAreaInput())
            .Place(e => e.Name, e => e.Stage) // Place will specify the order of the fields
            .Place(true, e => e.Amount, e => e.Probability) // This will place the fields side by side - useful for related fields
            .Group("Details", e => e.Description, e => e.Notes) // This will group the fields in a collapsible group - useful for related field that are less common
            .Remove(e => e.Id, e => e.CreatedAt, e => e.UpdatedAt) // We remove these fields from the form as users should not be able to edit them
            .Builder(e => e.ContactId, e => e.ToAsyncSelectInput(OpportunityHelpers.QueryContacts(db), OpportunityHelpers.LookupContact(db), placeholder: "Select Contact"))
            .Builder(e => e.Stage, e => e.ToSelectInput(new[]
            {
                new Option<string>("Prospecting", "Prospecting"),
                new Option<string>("Qualification", "Qualification"),
                new Option<string>("Proposal", "Proposal"),
                new Option<string>("Negotiation", "Negotiation"),
                new Option<string>("Closed Won", "Closed Won"),
                new Option<string>("Closed Lost", "Closed Lost")
            }, placeholder: "Select Stage"))
            .Builder(e => e.Probability, e => e.ToNumberInput().Min(0).Max(100))
            .ToSheet(isOpen, "Edit Opportunity");
    }
}

public static class OpportunityHelpers
{
    public static AsyncSelectQueryDelegate<int> QueryContacts(ApplicationDbContext db)
    {
        return async query =>
        {
            var searchTerm = string.IsNullOrWhiteSpace(query) ? "" : query;
            return (await db.Contacts
                    .Where(e => string.IsNullOrWhiteSpace(searchTerm) ||
                               e.FirstName.Contains(searchTerm) ||
                               e.LastName.Contains(searchTerm) ||
                               e.Company.Contains(searchTerm))
                    .Select(e => new { e.Id, DisplayName = $"{e.FirstName} {e.LastName} ({e.Company})" })
                    .Take(50)
                    .ToArrayAsync())
                .Select(e => new Option<int>(e.DisplayName, e.Id))
                .ToArray();
        };
    }

    public static AsyncSelectLookupDelegate<int> LookupContact(ApplicationDbContext db)
    {
        return async id =>
        {
            var contact = await db.Contacts.FindAsync(id);
            if (contact == null) return null;
            return new Option<int>($"{contact.FirstName} {contact.LastName} ({contact.Company})", contact.Id);
        };
    }
}
