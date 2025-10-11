using IvyOneBusinessOnePlatform.Apps;
using IvyOneBusinessOnePlatform.Services;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

// Configure services
var services = new ServiceCollection();
services.AddDatabase();

var serviceProvider = services.BuildServiceProvider();

// Initialize database
await serviceProvider.InitializeDatabaseAsync();

var server = new Server();
#if DEBUG
server.UseHotReload();
#endif
server.AddAppsFromAssembly();
server.AddConnectionsFromAssembly();
var chromeSettings = new ChromeSettings().DefaultApp<MainDashboardApp>().UseTabs(preventDuplicates: true);
server.UseChrome(chromeSettings);
await server.RunAsync();
