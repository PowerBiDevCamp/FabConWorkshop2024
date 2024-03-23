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

  // this is used for next 3 methods
  private const string DemoWorkspaceName = "A workspace with examples of shortcuts";

  public static void CreateCustomerTenantWithEmptyLakehouse() {

    string WorkspaceName = DemoWorkspaceName;
    string LakehouseName = "staging";

    Console.WriteLine("Provision new customer tenant to demo creating shortcuts by hand");

    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);

    FabricItem lakehouseItem = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void GetShortcutForLakehouse() {
    string WorkspaceName = DemoWorkspaceName;
    string LakehouseName = "staging";
    var workspace = FabricUserApi.GetWorkspaceByName(WorkspaceName);
    var lakehouse = FabricUserApi.GetLakehouseByName(workspace.id, LakehouseName);
    FabricUserApi.GetLakehouseShortcuts(workspace.id, lakehouse.id);
  }

  public static void CreateCustomerTenantWithShortcutExamples() {

    string WorkspaceName = DemoWorkspaceName;

    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);
    FabricItem lakehouseStaging = FabricUserApi.CreateLakehouse(workspace.id, "staging");
    FabricItem lakehouseSales = FabricUserApi.CreateLakehouse(workspace.id, "sales");

    FabricWorkspace targetworkspace = FabricUserApi.GetWorkspaceByName("Demo4 - CreateLakehouse");
    FabricItem targetlakehouse = FabricUserApi.GetLakehouseByName(targetworkspace.id, "sales");

    // (1) Create OneLake Folder Shortcut
    var createFolderShortcutRequest = new LakehouseShortcutCreateRequest {
      path = "/Files",
      name = "bronze_landing_layer",
      target = new LakehouseShortcutTarget {
        oneLake = new OneLakeShortcutTarget {
          workspaceId = targetworkspace.id,
          itemId = targetlakehouse.id,
          path = "Files/bronze_landing_layer"
        }
      }
    };

    FabricUserApi.CreateLakehouseShortcut(workspace.id, lakehouseStaging.id, createFolderShortcutRequest);

    // (2) Create ADLS Gen2 Folder Shortcut
    var createAdlsGen2FolderShortcutRequest = new LakehouseShortcutCreateRequest {
      name = "sales-data",
      path = "Files",
      target = new LakehouseShortcutTarget {
        adlsGen2 = new AdlsGen2ShortcutTarget {
          connectionId = AppSettings.ConnectIdToAzureStorage,
          location = AppSettings.AzureStorageUrl,
          subpath = "/sales-data"
        }
      }
    };

    FabricUserApi.CreateLakehouseShortcut(workspace.id, lakehouseStaging.id, createAdlsGen2FolderShortcutRequest);

    // (3) Create OneLake Table Shortcuts
    List<string> targetTables = new List<string>() { "calendar", "customers", "products", "sales" };

    foreach (var targetTable in targetTables) {

      var createTableShortcutRequest = new LakehouseShortcutCreateRequest {
        path = "Tables",
        name = targetTable,
        target = new LakehouseShortcutTarget {
          oneLake = new OneLakeShortcutTarget {
            workspaceId = targetworkspace.id,
            itemId = targetlakehouse.id,
            path = "Tables/" + targetTable
          }
        }
      };

      FabricUserApi.CreateLakehouseShortcut(workspace.id, lakehouseSales.id, createTableShortcutRequest);

    }

    Console.WriteLine();
    Console.Write(" - Refreshing sales lakehouse table schema");
    var sqlEndPoint = FabricUserApi.GetSqlEndpointForLakehouse(workspace.id, lakehouseSales.id);
    FabricUserApi.RefreshLakehouseSchema(sqlEndPoint.database);

    Console.WriteLine();
    Console.WriteLine();
    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

    OpenWorkspaceInBrowser(workspace.id);

  }

  public static void CreateCustomerTenantWithWarehouse(string WorkspaceName) {

    string LakehouseName = "staging";

    Console.WriteLine("Provision new customer tenant with shortcut to ALDS container");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);
    FabricItem lakehouseItem = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);
    FabricLakehouse lakehouse = FabricUserApi.GetLakehouse(workspace.id, lakehouseItem.id);

    LakehouseShortcutCreateRequest shortcutCreateRequest = new LakehouseShortcutCreateRequest {
      name = "sales-data",
      path = "Files",
      target = new LakehouseShortcutTarget {
        adlsGen2 = new AdlsGen2ShortcutTarget {
          connectionId = AppSettings.ConnectIdToAzureStorage,
          location = AppSettings.AzureStorageUrl,
          subpath = "/sales-data"
        }
      }
    };

    FabricUserApi.CreateLakehouseShortcut(workspace.id, lakehouse.id, shortcutCreateRequest);
    Console.WriteLine();

    FabricUserApi.LoadLakehouseTableFromCsv(workspace.id, lakehouse.id, "Files/sales-data/customers.csv", "customers");
    FabricUserApi.LoadLakehouseTableFromCsv(workspace.id, lakehouse.id, "Files/sales-data/invoices.csv", "invoices");
    FabricUserApi.LoadLakehouseTableFromCsv(workspace.id, lakehouse.id, "Files/sales-data/invoice_details.csv", "invoice_details");
    FabricUserApi.LoadLakehouseTableFromCsv(workspace.id, lakehouse.id, "Files/sales-data/products.csv", "products");

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
    sqlWriter.ExecuteSql(Demo07_CreateShortcut.Properties.Resources.CreateSproc_CreateAllTables_sql);
    Console.WriteLine("   > Executing stored procedure to create all tables");
    sqlWriter.ExecuteSql("EXEC create_all_tables");

    sqlWriter.ExecuteSql(Demo07_CreateShortcut.Properties.Resources.CreateSproc_RefreshProducts_sql);
    sqlWriter.ExecuteSql(Demo07_CreateShortcut.Properties.Resources.CreateSproc_RefreshCustomers_sql);
    sqlWriter.ExecuteSql(Demo07_CreateShortcut.Properties.Resources.CreateSproc_RefreshSales_sql);
    sqlWriter.ExecuteSql(Demo07_CreateShortcut.Properties.Resources.CreateSproc_RefreshCalendar_sql);

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

    // uncomment next two lines to test Power BI embedding
    // WebPageGenerator.GenerateReportPageUserOwnsData(workspace.id, report.id);
    // WebPageGenerator.GenerateReportPageAppOwnsData(workspace.id, report.id);

    OpenWorkspaceInBrowser(workspace.id);

    Console.WriteLine();
    Console.WriteLine("Customer tenant provisioning complete");
    Console.WriteLine();

    Console.Write("Press ENTER to open workspace in the browser");
    Console.ReadLine();

  }
  
  public static void CreateCustomerTenantAndLoadTaxiData() {

    string WorkspaceName = "Taxi Trips";
    string LakehouseName = "taxi_data";

    Console.WriteLine("Provision new customer tenant with shortcut with taxi data");
    FabricWorkspace workspace = FabricUserApi.CreateWorkspace(WorkspaceName, AppSettings.PremiumCapacityId);
    FabricItem lakehouseItem = FabricUserApi.CreateLakehouse(workspace.id, LakehouseName);
    FabricLakehouse lakehouse = FabricUserApi.GetLakehouse(workspace.id, lakehouseItem.id);

    LakehouseShortcutCreateRequest shortcutCreateRequest = new LakehouseShortcutCreateRequest {
      name = "taxi-data",
      path = "Files",
      target = new LakehouseShortcutTarget {
        adlsGen2 = new AdlsGen2ShortcutTarget {
          connectionId = AppSettings.ConnectIdToAzureStorage,
          location = AppSettings.AzureStorageUrl,
          subpath = "/taxi-data"
        }
      }
    };

    FabricUserApi.CreateLakehouseShortcut(workspace.id, lakehouse.id, shortcutCreateRequest);
    Console.WriteLine();

    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2017.parquet", "taxi_trips", false);
    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2018.parquet", "taxi_trips", true);
    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2019.parquet", "taxi_trips", true);
    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2020.parquet", "taxi_trips", true);
    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2021.parquet", "taxi_trips", true);
    FabricUserApi.LoadLakehouseTableFromParquet(workspace.id, lakehouse.id, "Files/taxi-data/taxi-2022.parquet", "taxi_trips", true);

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

}

