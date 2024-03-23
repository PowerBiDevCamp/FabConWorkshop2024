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

    Console.Write("\"Press ENTER to continue");
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

  public static void CreateAzureDevOpsProject(string WorkspaceName) {

    Console.WriteLine("Creating a new project in Azure Dev Ops namaed {0}", WorkspaceName);
    Console.WriteLine();


    // create new project in Azure Dev Ops
    AdoProjectManager.CreateProject(WorkspaceName);

    Console.WriteLine();
    Console.WriteLine("Press ENTER to continue");
    Console.ReadLine();

  }

  public static void CreateCustomerTenantWithGitIntegration(string WorkspaceName) {

    Console.WriteLine("Provision a new Fabric tenant with GIT Integration");
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

    // create new project in Azure Dev Ops
    AdoProjectManager.CreateProject(WorkspaceName, workspace);

    var providerConnection = new GitProviderConnection {
      gitProviderDetails = new GitProviderDetails {
        gitProviderType = "AzureDevOps",
        organizationName = "DevCampDevOps",
        projectName = WorkspaceName,
        repositoryName = WorkspaceName,
        branchName = "main",
        directoryName = "/"
      }
    };

    FabricUserApi.ConnectWorkspaceToGitRepository(workspace.id, providerConnection);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void CreateCustomerTenantWithSourceFilesFromGIT(string WorkspaceName) {

    Console.WriteLine("Provision a new Fabric customer tenant and initialize from GIT");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName);

    AdoProjectManager.CreateProjectWithImportModeSourceFile(WorkspaceName, workspace);

    var providerConnection = new GitProviderConnection {
      gitProviderDetails = new GitProviderDetails {
        gitProviderType = "AzureDevOps",
        organizationName = "DevCampDevOps",
        projectName = WorkspaceName,
        repositoryName = WorkspaceName,
        branchName = "main",
        directoryName = "/"
      }
    };

    FabricUserApi.ConnectWorkspaceToGitRepository(workspace.id, providerConnection);

    Console.WriteLine(" - Preparing semantic model by importing data");

    var model = FabricUserApi.GetSemanticModelByName(workspace.id, "Product Sales");

    Console.WriteLine("   > Patching datasource credentials for semantic model");
    PowerBiUserApi.PatchAnonymousAccessWebCredentials(workspace.id, model.id);

    Console.Write("   > Refreshing semantic model");
    PowerBiUserApi.RefreshDataset(workspace.id, model.id);
    Console.WriteLine();

    FabricUserApi.CommitWorkspaceToGit(workspace.id);
    FabricUserApi.UpdateWorkspaceFromGit(workspace.id);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);


  }

  private static void OpenWorkspaceInBrowser(string WorkspaceId) {

    string url = "https://app.powerbi.com/groups/" + WorkspaceId;

    var process = new Process();
    process.StartInfo = new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
    process.StartInfo.Arguments = url + " --profile-directory=\"Profile 1\" ";
    process.Start();

  }


  // delete after use
  public static void CreateCustomerTenantWithGitIntegrationListing(string WorkspaceName) {

    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName);

    FabricItemCreateRequest modelCreateRequest = FabricItemDefinitionFactory.GetImportedSalesModelCreateRequest("Product Sales");
    var model = FabricUserApi.CreateItem(workspace.id, modelCreateRequest);
    PowerBiUserApi.PatchAnonymousAccessWebCredentials(workspace.id, model.id);

    FabricItemCreateRequest createRequestReport = FabricItemDefinitionFactory.GetSalesReportCreateRequest(model.id, "Product Sales");
    var report = FabricUserApi.CreateItem(workspace.id, createRequestReport);

    // create new project in Azure Dev Ops
    AdoProjectManager.CreateProject(WorkspaceName, workspace);

    var providerConnection = new GitProviderConnection {
      gitProviderDetails = new GitProviderDetails {
        gitProviderType = "AzureDevOps",
        organizationName = "DevCampDevOps",
        projectName = WorkspaceName,
        repositoryName = WorkspaceName,
        branchName = "main",
        directoryName = "/"
      }
    };

    FabricUserApi.ConnectWorkspaceToGitRepository(workspace.id, providerConnection);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void CreateCustomerTenantWithSourceFilesFromGITListing(string WorkspaceName) {

    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName);

    AdoProjectManager.CreateProjectWithImportModeSourceFile(WorkspaceName, workspace);

    var providerConnection = new GitProviderConnection {
      gitProviderDetails = new GitProviderDetails {
        gitProviderType = "AzureDevOps",
        organizationName = "DevCampDevOps",
        projectName = WorkspaceName,
        repositoryName = WorkspaceName,
        branchName = "main",
        directoryName = "/"
      }
    };

    FabricUserApi.ConnectWorkspaceToGitRepository(workspace.id, providerConnection);

    var model = FabricUserApi.GetSemanticModelByName(workspace.id, "Product Sales");
    PowerBiUserApi.PatchAnonymousAccessWebCredentials(workspace.id, model.id);
    PowerBiUserApi.RefreshDataset(workspace.id, model.id);

    FabricUserApi.CommitWorkspaceToGit(workspace.id);
    FabricUserApi.UpdateWorkspaceFromGit(workspace.id);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);


  }


}

