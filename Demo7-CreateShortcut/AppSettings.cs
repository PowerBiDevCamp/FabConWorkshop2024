
public class AppSettings {

  public const string FabricUserApiBaseUrl = "https://api.fabric.microsoft.com/v1";
  public const string PowerBiRestApiBaseUrl = "https://api.powerbi.com";

  // use App Id for PowerShell Azure Auth - it works in every Fabric tenant
  public const string ApplicationId = "1950a258-227b-4e31-a9cf-717495945fc2";
  public const string RedirectUri = "http://localhost";

  // add Capacity Id for Premium capacity
  public const string PremiumCapacityId = "ADD_ID_FOR_CAPACITY_HERE";

  // Connection Id must be hard-code until Fabric CRUD API for Connections is released
  public const string AzureStorageUrl = "https://YOUR_STORAGE_ACCOUNT.dfs.core.windows.net";
  public const string ConnectIdToAzureStorage = "ADD_ID_FOR_CONNECTIO_HERE";

  public const string LocalWebPageFolder = @"..\..\..\WebPages\";
  public const string LocalTemplatesFolder = @"..\..\..\ItemDefinitionExports\";

}
