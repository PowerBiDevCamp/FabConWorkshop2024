
public class FabricPermissionScopes {

  // this provides all delegated permission scopes to current user
  public static readonly string[] Fabric_User_Impresonation = new string[] {
      "https://analysis.windows.net/powerbi/api/user_impersonation"
  };


  public const string resourceUri = "https://api.fabric.microsoft.com/";

  // delegated permission scopes used for user token acquisition
  public static readonly string[] TenantProvisioning = new string[] {
      "https://api.fabric.microsoft.com/Capacity.ReadWrite.All",
      "https://api.fabric.microsoft.com/Workspace.ReadWrite.All",
      "https://api.fabric.microsoft.com/Item.ReadWrite.All",
      "https://api.fabric.microsoft.com/Item.Read.All",
      "https://api.fabric.microsoft.com/Item.Execute.All",
      "https://api.fabric.microsoft.com/Content.Create",
      "https://api.fabric.microsoft.com/Dataset.ReadWrite.All ",
      "https://api.fabric.microsoft.com/Report.ReadWrite.All",
      "https://api.fabric.microsoft.com/Workspace.GitCommit.All",
    };



}



