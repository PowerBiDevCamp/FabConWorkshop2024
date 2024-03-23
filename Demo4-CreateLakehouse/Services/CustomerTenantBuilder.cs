using System.Diagnostics;


public class CustomerTenantBuilder {

  public static void ViewWorkspaces() {

    Console.WriteLine("View workspaces accessible to current user");
    Console.WriteLine();

    var workspcaes = FabricUserApi.GetWorkspaces();

    Console.WriteLine(" > Workspaces List");
    foreach (var workspace in workspcaes) {
      Console.WriteLine("   - {0} ({1})", workspace.displayName, workspace.id);
    }

    Console.WriteLine();

    Console.Write("Press ENTER to continue");
    Console.ReadLine();

  }

  public static void ViewCapacities() {

    Console.WriteLine("View capacities accessible to current user");
    Console.WriteLine();

    var capacities = FabricUserApi.GetCapacities();

    Console.WriteLine(" > Capacities List");
    foreach (var capacity in capacities) {
      Console.WriteLine("   - [{0}] {1} ({2})", capacity.sku, capacity.displayName, capacity.id);
    }

    Console.WriteLine();

    Console.Write("Press ENTER to continue");
    Console.ReadLine();

  }

  public static void CreateCustomerTenant(string WorkspaceName) {

    Console.WriteLine("Provision a new Fabric customer tenant");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);

    Console.WriteLine();
    Console.WriteLine("Mission complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void CreateCustomerTenantForNotebookDemo(string WorkspaceName) {

    string LakehouseName = "sales";

    Console.WriteLine("Provision new tenant with Lakehouse and DirectLake semantic model");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);

    FabricItem lakehouse = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);

    string displayName = "Create Lakehouse Tables";
    string codeContent = Demo4_CreateLakehouse.Properties.Resources.CreateLakehouseTables_ipynb;
    var notebookCreateRequest = FabricItemDefinitionFactory.GetNotebookCreateRequest(workspace.id, lakehouse, displayName, codeContent);

    var notebook = FabricUserApi.CreateItem(workspace.id, notebookCreateRequest);  

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();
  
    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void CreateCustomerTenantWithLakehouse(string WorkspaceName) {

    string LakehouseName = "sales";

    Console.WriteLine("Provision new tenant with Lakehouse and DirectLake semantic model");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);

    FabricItem lakehouse = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);

    string displayName = "Create Lakehouse Tables";
    string codeContent = Demo4_CreateLakehouse.Properties.Resources.CreateLakehouseTables_ipynb;
    var notebookCreateRequest = FabricItemDefinitionFactory.GetNotebookCreateRequest(workspace.id, lakehouse, displayName, codeContent);

    var notebook = FabricUserApi.CreateItem(workspace.id, notebookCreateRequest);

    FabricUserApi.RunNotebook(workspace.id, notebook);

    Console.Write(" - Getting SQL endpoint connection information");
    var sqlEndpoint = FabricUserApi.GetSqlEndpointForLakehouse(workspace.id, lakehouse.id);

    Console.WriteLine("   > Server: " + sqlEndpoint.server);
    Console.WriteLine("   > Database: " + sqlEndpoint.database);
    Console.WriteLine();

    var modelCreateRequest =
      FabricItemDefinitionFactory.GetDirectLakeSalesModelCreateRequest("Product Sales", sqlEndpoint.server, sqlEndpoint.database);

    var model = FabricUserApi.CreateItem(workspace.id, modelCreateRequest);

    Console.WriteLine(" - Preparing " + model.displayName + " semantic model");

    Console.WriteLine("   > Patching datasource credentials for semantic model");
    PowerBiUserApi.PatchDirectLakeDatasetCredentials(workspace.id, model.id);

    // workaround for regression bug - call internal API call to refresh lakehouse schema
    // this code should be removed as soon as bug is fixed
    PowerBiUserApi.RefreshSqlEndointSchema(sqlEndpoint.database);

    Console.Write("   > Refreshing semantic model");
    PowerBiUserApi.RefreshDataset(workspace.id, model.id);
    Console.WriteLine();

    FabricItemCreateRequest createRequestReport =
      FabricItemDefinitionFactory.GetSalesReportCreateRequest(model.id, "Product Sales");

    var report = FabricUserApi.CreateItem(workspace.id, createRequestReport);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    // uncomment next two lines to test Power BI embedding
    // WebPageGenerator.GenerateReportPageUserOwnsData(workspace.id, report.id);
    // WebPageGenerator.GenerateReportPageAppOwnsData(workspace.id, report.id);

    OpenWorkspaceInBrowser(workspace.id);

  }

  private static void OpenWorkspaceInBrowser(string WorkspaceId) {

    string url = "https://app.powerbi.com/groups/" + WorkspaceId;

    var process = new Process();
    process.StartInfo = new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
    process.StartInfo.Arguments = url + " --profile-directory=\"Profile 1\" ";
    process.Start();

  }

}

