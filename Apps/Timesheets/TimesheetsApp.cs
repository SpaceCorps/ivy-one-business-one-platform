namespace IvyOneBusinessOnePlatform.Apps.Timesheets;

[App(icon: Icons.Clock, title: "Timesheets")]
public class TimesheetsApp : ViewBase
{
    public override object? Build()
    {
        var client = this.UseService<IClientProvider>();
        
        return new Card(
            Layout.Vertical()
                .Gap(16)
                .Padding(24)
                .Add("Time Tracking")
                .Add("Track work hours and manage employee timesheets")
                .Add(Layout.Horizontal()
                    .Gap(12)
                    .Add(new Button("Log Time", _ => client.Toast("Time logging coming soon!")))
                    .Add(new Button("View Timesheets", _ => client.Toast("Timesheet view coming soon!")))
                    .Add(new Button("Reports", _ => client.Toast("Time reports coming soon!")))
                )
        );
    }
}
