using System.Diagnostics;


public class CustomerTenantBuilder {

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

  public static void CreateCustomerTenantWithImportedSalesModel(string WorkspaceName) {

    Console.WriteLine("Provision a new tenant with import-mode semantic model");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName);

    FabricItemCreateRequest modelCreateRequest =
      FabricItemDefinitionFactory.GetImportedSalesModelCreateRequest("Product Sales");

    var model = FabricUserApi.CreateItem(workspace.id, modelCreateRequest);

    Console.WriteLine(" - Preparing " + model.displayName + " semantic model");

    Console.WriteLine("   > Patching datasource credentials for semantic model");
    PowerBiUserApi.PatchAnonymousAccessWebCredentials(workspace.id, model.id);

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

    WebPageGenerator.GenerateReportPageUserOwnsData(workspace.id, report.id);

    WebPageGenerator.GenerateReportPageAppOwnsData(workspace.id, report.id);

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void UpdateSalesModel(string WorkspaceName) {

    Console.WriteLine("Updating item definition for semantic model named Product Sales");
    Console.WriteLine();

    FabricWorkspace workspace = FabricUserApi.GetWorkspaceByName(WorkspaceName);

    var model = FabricUserApi.GetSemanticModelByName(workspace.id, "Product Sales");

    var updateModelRequest = FabricItemDefinitionFactory.GetImportedSalesModelUpdateRequest();

    Console.Write(" > Calling UpdateItem API to update item definition for sementic model ");
    FabricUserApi.UpdateItemDefinition(workspace.id, model.id, updateModelRequest);

    Console.WriteLine("   - Sementic model item definition updated");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }


  public static void UpdateSalesReport(string WorkspaceName) {

    Console.WriteLine("Updating item definition for report named Product Sales");
    Console.WriteLine();   

    FabricWorkspace workspace = FabricUserApi.GetWorkspaceByName(WorkspaceName);

    var model = FabricUserApi.GetSemanticModelByName(workspace.id, "Product Sales");
    var report = FabricUserApi.GetReportByName(workspace.id, "Product Sales");

    var updateReportRequest = FabricItemDefinitionFactory.GetSalesReportUpdateThemeRequest(model.id, "Product Sales");

    Console.Write(" > Calling UpdateItem API to update item definition for report");

    FabricUserApi.UpdateItemDefinition(workspace.id, report.id, updateReportRequest);
    Console.WriteLine("   - Report item definition updated");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    WebPageGenerator.GenerateReportPageUserOwnsData(workspace.id, report.id);

    WebPageGenerator.GenerateReportPageAppOwnsData(workspace.id, report.id);

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

