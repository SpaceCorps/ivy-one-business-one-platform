namespace IvyOneBusinessOnePlatform.Apps.HR;

[App(icon: Icons.User, title: "HR")]
public class HRApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Human Resources")
                .Add("Manage employees, payroll, benefits, and HR processes")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Employee Records", _ => client.Toast("Employee management coming soon!")))
                    .Add(new Button("Payroll System", _ => client.Toast("Payroll processing coming soon!")))
                    .Add(new Button("Benefits Portal", _ => client.Toast("Benefits management coming soon!")))
                )
        );
    }
}
