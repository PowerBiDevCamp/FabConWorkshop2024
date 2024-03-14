
public class AppSettings {

  public const string FabricUserApiBaseUrl = "https://api.fabric.microsoft.com/v1";

  // use App Id for PowerShell Azure Auth - it works in every Fabric tenant
  public const string ApplicationId = "1950a258-227b-4e31-a9cf-717495945fc2";
  public const string RedirectUri = "http://localhost";

  // add Capacity Id for Premium capacity
   public const string PremiumCapacityId = "ADD_ID_FOR_CAPACITY_HERE";

  public const string LocalWebPageFolder = @"..\..\..\WebPages\";
  public const string LocalTemplatesFolder = @"..\..\..\ItemDefinitionExports\";
  public const string LocalDataFilesFolder = @"..\..\..\DataFiles\";



}
