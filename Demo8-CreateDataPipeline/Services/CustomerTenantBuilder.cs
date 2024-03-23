using Microsoft.PowerBI.Api.Models;
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

    Console.Write("Press ENTER to open workspace in the browser");
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

    Console.Write("Press ENTER to open workspace in the browser");
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

  public static void CreateCustomerTenantWithDataPipeline(string WorkspaceName) {

    Console.WriteLine("Provision new customer tenant with shortcut to ALDS container");

    string LakehouseName = "staging";
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);
    FabricItem lakehouseItem = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);
    FabricLakehouse lakehouse = FabricUserApi.GetLakehouse(workspace.id, lakehouseItem.id);

    string DataPipeLineName = "Copy Data Files Pipeline";
    string codeContent =  Demo8_CreateDataPipeline.Properties.Resources.CopySalesData_json;

    FabricItemCreateRequest createDataPipelineRequest = 
      FabricItemDefinitionFactory.GetDataPipelineCreateRequest(workspace.id, lakehouse.id, DataPipeLineName, codeContent);

    var pipeline = FabricUserApi.CreateItem(workspace.id, createDataPipelineRequest);

    FabricUserApi.RunDataPipeline(workspace.id, pipeline);

    // Call GetSqlEndpointForLakehouse to pause until SQL endpoint is ready
    var sqlEndPoint = FabricUserApi.GetSqlEndpointForLakehouse(workspace.id, lakehouse.id);

    // this call is required to deal with regression bug 
    FabricUserApi.RefreshLakehouseSchema(sqlEndPoint.database);

    // create warehouse
    string WarehouseName = "sales";

    FabricItem warehouseItem = FabricUserApi.CreateWarehouse(workspace.id, WarehouseName);

    Console.Write(" - Getting SQL Endpoint connection info for warehouse");
    string connectionInfo = FabricUserApi.GetWarehouseConnection(workspace.id, warehouseItem.id);
    Console.WriteLine();
    Console.WriteLine("   > Server: " + connectionInfo);
    Console.WriteLine();

    Console.WriteLine(" - Connecting to Warehouse SQL endpoint to execute SQL statements");

    var sqlWriter = new SqlConnectionWriter(connectionInfo, WarehouseName);

    Console.WriteLine("   > Executing SQL statement to create stored procedures to create all tables");
    sqlWriter.ExecuteSql(Demo8_CreateDataPipeline.Properties.Resources.CreateSproc_CreateAllTables_sql);
    Console.WriteLine("   > Executing stored procedure to create all tables");
    sqlWriter.ExecuteSql("EXEC create_all_tables");

    sqlWriter.ExecuteSql(Demo8_CreateDataPipeline.Properties.Resources.CreateSproc_RefreshProducts_sql);
    sqlWriter.ExecuteSql(Demo8_CreateDataPipeline.Properties.Resources.CreateSproc_RefreshCustomers_sql);
    sqlWriter.ExecuteSql(Demo8_CreateDataPipeline.Properties.Resources.CreateSproc_RefreshSales_sql);
    sqlWriter.ExecuteSql(Demo8_CreateDataPipeline.Properties.Resources.CreateSproc_RefreshCalendar_sql);

    Console.WriteLine("   > Executing stored procedure to refresh products table");
    sqlWriter.ExecuteSql("EXEC refresh_products");

    Console.WriteLine("   > Executing stored procedure to refresh customers table");
    sqlWriter.ExecuteSql("EXEC refresh_customers");

    Console.WriteLine("   > Executing stored procedure to refresh sales table");
    sqlWriter.ExecuteSql("EXEC refresh_sales");

    Console.WriteLine("   > Executing stored procedure to refresh calendar table");
    sqlWriter.ExecuteSql("EXEC refresh_calendar");

    Console.WriteLine();

    var modelCreateRequest =
      FabricItemDefinitionFactory.GetDirectLakeSalesModelCreateRequest("Product Sales", connectionInfo, WarehouseName);

    var model = FabricUserApi.CreateItem(workspace.id, modelCreateRequest);

    Console.WriteLine(" - Preparing " + model.displayName + " semantic model");

    Console.WriteLine("   > Patching datasource credentials for semantic model");
    PowerBiUserApi.PatchDirectLakeDatasetCredentials(workspace.id, model.id);

    // workaround for regression bug - call internal API call to refresh lakehouse schema
    PowerBiUserApi.RefreshSqlEndointSchema(sqlEndPoint.database);

    Console.Write("   > Refreshing semantic model");
    PowerBiUserApi.RefreshDataset(workspace.id, model.id);
    Console.WriteLine();

    FabricItemCreateRequest createRequestReport =
      FabricItemDefinitionFactory.GetSalesReportCreateRequest(model.id, "Product Sales");

    var report = FabricUserApi.CreateItem(workspace.id, createRequestReport);

    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    WebPageGenerator.GenerateReportPageUserOwnsData(workspace.id, report.id);
    WebPageGenerator.GenerateReportPageAppOwnsData(workspace.id, report.id);

    OpenWorkspaceInBrowser(workspace.id);

 
   

  }

  private static void OpenWorkspaceInBrowser(string WorkspaceId) {

    string url = "https://dxt.powerbi.com/groups/" + WorkspaceId;

    var process = new Process();
    process.StartInfo = new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe");
    process.StartInfo.Arguments = url + " --profile-directory=\"Profile 2\" ";
    process.Start();

  }

}

