using Microsoft.EntityFrameworkCore;
using IvyOneBusinessOnePlatform.Data;

namespace IvyOneBusinessOnePlatform.Apps.EmailMarketing;

public static class CampaignStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Completed = "Completed";
}

[App(icon: Icons.Mail, title: "Email Marketing", path: new[] { "Marketing & Communication" })]
public class EmailMarketingApp : ViewBase
{
    public override object? Build()
    {
        return this.UseBlades(() => new EmailMarketingRootBlade(), "Email Marketing", Size.Units(100));
    }
}

public class EmailMarketingRootBlade : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        var searchQuery = this.UseState("");
        var isNewCampaignOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        var query = context.Campaigns.Where(c => c.Type == "Email").AsQueryable();
        
        if (!string.IsNullOrEmpty(searchQuery.Value))
        {
            var searchPattern = $"%{searchQuery.Value}%";
            query = query.Where(c => 
                EF.Functions.Like(c.Name, searchPattern) ||
                EF.Functions.Like(c.Description, searchPattern) ||
                EF.Functions.Like(c.Status, searchPattern));
        }
        
        var campaigns = query.OrderByDescending(c => c.CreatedAt).ToList();
        
        var listItems = campaigns.Select(camp => new ListItem(
            title: camp.Name,
            subtitle: $"{camp.Status} - ${camp.Budget:N0} - {camp.TargetAudience}",
            icon: Icons.Mail,
            badge: camp.Status,
            onClick: _ => blades.Push(this, new CampaignDetailBlade(camp.Id, () => refreshToken.Refresh()), camp.Name)
        ));
        
        var mainContent = BladeHelper.WithHeader(
            Layout.Horizontal()
                .Gap(4)
                .Add(searchQuery.ToSearchInput().Placeholder("Search campaigns..."))
                .Add(new Button("New Campaign")
                    .Icon(Icons.Plus)
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => isNewCampaignOpen.Set(true))),
            campaigns.Count == 0 
                ? Text.Block("No email campaigns found!")
                : new List(listItems)
        );

        return isNewCampaignOpen.Value ? new Sheet(
            (Event<Sheet> _) => isNewCampaignOpen.Set(false),
            new CampaignFormSheet("Email", null, () => {
                isNewCampaignOpen.Set(false);
                refreshToken.Refresh();
            }),
            title: "New Email Campaign",
            description: "Create a new email marketing campaign"
        ).Width(Size.Fraction(1/3f)) : mainContent;
    }
}

public class CampaignDetailBlade(int campaignId, Action? onRefresh = null) : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        var blades = this.UseContext<IBladeController>();
        
        var initialCampaign = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
        
        if (initialCampaign == null)
        {
            return Layout.Vertical()
                .Gap(4)
                .Add(Text.H3("Campaign Not Found"))
                .Add(new Button("Go Back", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary));
        }
        
        var campaignData = this.UseState(initialCampaign);
        var isEditOpen = this.UseState(false);
        var refreshToken = this.UseRefreshToken();
        
        this.UseEffect(() =>
        {
            var updatedCampaign = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
            if (updatedCampaign != null)
            {
                campaignData.Set(updatedCampaign);
            }
        }, [refreshToken.ToTrigger()]);
        
        var statusBadge = new Badge(campaignData.Value.Status)
            .Variant(campaignData.Value.Status == CampaignStatus.Active ? BadgeVariant.Success :
                   campaignData.Value.Status == CampaignStatus.Completed ? BadgeVariant.Primary :
                   BadgeVariant.Secondary);

        var campaignDetails = new
        {
            Name = campaignData.Value.Name,
            Description = campaignData.Value.Description,
            Type = campaignData.Value.Type,
            Budget = $"${campaignData.Value.Budget:N0}",
            StartDate = campaignData.Value.StartDate.ToString("MMM dd, yyyy"),
            EndDate = campaignData.Value.EndDate?.ToString("MMM dd, yyyy"),
            TargetAudience = campaignData.Value.TargetAudience,
            Status = statusBadge
        };
        
        return Layout.Vertical()
            .Gap(4)
            .Add(Text.H3(campaignData.Value.Name))
            .Add(campaignDetails.ToDetails().RemoveEmpty().MultiLine(x => x.Description))
            .Add(Layout.Horizontal()
                .Gap(4)
                .Add(campaignData.Value.Status == CampaignStatus.Draft ? new Button("Activate Campaign", _ => {
                    var dbCampaign = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
                    if (dbCampaign != null)
                    {
                        dbCampaign.Status = CampaignStatus.Active;
                        dbCampaign.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Campaign {campaignData.Value.Name} activated!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Success)
                    .Icon(Icons.Play)
                    : null)
                .Add(campaignData.Value.Status == CampaignStatus.Active ? new Button("Pause Campaign", _ => {
                    var dbCampaign = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
                    if (dbCampaign != null)
                    {
                        dbCampaign.Status = CampaignStatus.Paused;
                        dbCampaign.UpdatedAt = DateTime.UtcNow;
                        context.SaveChanges();
                        client.Toast($"Campaign {campaignData.Value.Name} paused!");
                        refreshToken.Refresh();
                        onRefresh?.Invoke();
                    }
                })
                    .Variant(ButtonVariant.Warning)
                    .Icon(Icons.Pause)
                    : null)
                .Add(new Button("Edit Campaign")
                    .Variant(ButtonVariant.Outline)
                    .Icon(Icons.Pencil)
                    .HandleClick(_ => isEditOpen.Set(true)))
                .Add(new Button("Delete Campaign")
                    .Variant(ButtonVariant.Destructive)
                    .Icon(Icons.Trash)
                    .HandleClick(_ => {
                        try
                        {
                            var campaignToDelete = context.Campaigns.FirstOrDefault(c => c.Id == campaignId);
                            if (campaignToDelete != null)
                            {
                                context.Campaigns.Remove(campaignToDelete);
                                context.SaveChanges();
                                client.Toast($"Campaign {campaignData.Value.Name} deleted successfully!");
                                refreshToken.Refresh();
                                onRefresh?.Invoke();
                                blades.Pop();
                            }
                        }
                        catch (Exception ex)
                        {
                            client.Toast($"Error deleting campaign: {ex.Message}", "Error");
                        }
                    }))
                .Add(new Button("Cancel", _ => blades.Pop())
                    .Variant(ButtonVariant.Secondary)))
            .Add(isEditOpen.Value ? new Sheet(
                (Event<Sheet> _) => isEditOpen.Set(false),
                new CampaignFormSheet(campaignData.Value.Type, campaignId, () => {
                    isEditOpen.Set(false);
                    refreshToken.Refresh();
                    onRefresh?.Invoke();
                }),
                title: "Edit Campaign",
                description: $"Edit campaign {campaignData.Value.Name}"
            ).Width(Size.Fraction(1/3f)) : null);
    }
}

public class CampaignFormSheet(string campaignType, int? campaignId = null, Action? onClose = null) : ViewBase
{
    private Campaign CloneCampaign(Campaign source) => new Campaign
    {
        Id = source.Id,
        Name = source.Name,
        Description = source.Description,
        Type = source.Type,
        Status = source.Status,
        StartDate = source.StartDate,
        EndDate = source.EndDate,
        Budget = source.Budget,
        TargetAudience = source.TargetAudience,
        CreatedAt = source.CreatedAt,
        UpdatedAt = source.UpdatedAt
    };
    
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        var context = this.UseService<ApplicationDbContext>();
        
        var isEdit = campaignId.HasValue;
        var existingCampaign = isEdit ? context.Campaigns.FirstOrDefault(c => c.Id == campaignId!.Value) : null;
        
        var campaignForm = this.UseState(existingCampaign ?? new Campaign
        {
            Name = "",
            Description = "",
            Type = campaignType,
            Status = CampaignStatus.Draft,
            StartDate = DateTime.UtcNow,
            EndDate = null,
            Budget = 0.00m,
            TargetAudience = ""
        });
        
        var statusOptions = new[] { CampaignStatus.Draft, CampaignStatus.Active, CampaignStatus.Paused, CampaignStatus.Completed };
        
        return new FooterLayout(
            Layout.Horizontal().Gap(2)
                .Add(new Button("Save")
                    .Variant(ButtonVariant.Primary)
                    .HandleClick(_ => {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(campaignForm.Value.Name))
                            {
                                client.Toast("Campaign Name is required", "Validation Error");
                                return;
                            }
                            
                            if (campaignForm.Value.Budget < 0)
                            {
                                client.Toast("Budget cannot be negative", "Validation Error");
                                return;
                            }
                            
                            if (isEdit && existingCampaign != null)
                            {
                                existingCampaign.Name = campaignForm.Value.Name;
                                existingCampaign.Description = campaignForm.Value.Description;
                                existingCampaign.Status = campaignForm.Value.Status;
                                existingCampaign.StartDate = campaignForm.Value.StartDate;
                                existingCampaign.EndDate = campaignForm.Value.EndDate;
                                existingCampaign.Budget = campaignForm.Value.Budget;
                                existingCampaign.TargetAudience = campaignForm.Value.TargetAudience;
                                existingCampaign.UpdatedAt = DateTime.UtcNow;
                            }
                            else
                            {
                                var newCampaign = new Campaign
                                {
                                    Name = campaignForm.Value.Name,
                                    Description = campaignForm.Value.Description,
                                    Type = campaignType,
                                    Status = campaignForm.Value.Status,
                                    StartDate = campaignForm.Value.StartDate,
                                    EndDate = campaignForm.Value.EndDate,
                                    Budget = campaignForm.Value.Budget,
                                    TargetAudience = campaignForm.Value.TargetAudience,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                context.Campaigns.Add(newCampaign);
                            }
                            
                            context.SaveChanges();
                            client.Toast(isEdit ? "Campaign updated successfully!" : "Campaign created successfully!");
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
                        .Add(Text.Small("Campaign Information"))
                        .Add(Text.Small("Campaign Name"))
                        .Add(new TextInput(campaignForm.Value.Name, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.Name = e.Value;
                            campaignForm.Set(cloned);
                        }).Placeholder("Summer Sale 2025"))
                        .Add(Text.Small("Description"))
                        .Add(new TextInput(campaignForm.Value.Description, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.Description = e.Value;
                            campaignForm.Set(cloned);
                        }).Placeholder("Campaign description...").Variant(TextInputs.Textarea))
                        .Add(Text.Small("Target Audience"))
                        .Add(new TextInput(campaignForm.Value.TargetAudience, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.TargetAudience = e.Value;
                            campaignForm.Set(cloned);
                        }).Placeholder("All subscribers"))
                        .Add(Text.Small("Status"))
                        .Add(new SelectInput<string>(campaignForm.Value.Status, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.Status = e.Value;
                            campaignForm.Set(cloned);
                        }, statusOptions.ToOptions()))
                ).Title("Campaign Details"))
                
                .Add(new Card(
                    Layout.Vertical().Gap(3)
                        .Add(Text.Small("Timeline & Budget"))
                        .Add(Text.Small("Start Date"))
                        .Add(new DateTimeInput<DateTime>(campaignForm.Value.StartDate, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.StartDate = e.Value;
                            campaignForm.Set(cloned);
                        }))
                        .Add(Text.Small("End Date (Optional)"))
                        .Add(new DateTimeInput<DateTime?>(campaignForm.Value.EndDate, e => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.EndDate = e.Value;
                            campaignForm.Set(cloned);
                        }))
                        .Add(Text.Small("Budget ($)"))
                        .Add(new NumberInput<decimal>(campaignForm.Value.Budget, v => {
                            var cloned = CloneCampaign(campaignForm.Value);
                            cloned.Budget = v;
                            campaignForm.Set(cloned);
                        }).Placeholder("0.00"))
                ).Title("Schedule"))
        );
    }
}
