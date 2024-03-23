using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

public class FabricUserApi {

  #region "Utility methods for executing HTTP requests"

  private static string AccessToken = AzureAdTokenManager.GetAccessToken(FabricPermissionScopes.Fabric_User_Impresonation);

  private static string ExecuteGetRequest(string endpoint) {

    string restUri = AppSettings.FabricUserApiBaseUrl + endpoint;

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
    client.DefaultRequestHeaders.Add("Accept", "application/json");

    HttpResponseMessage response = client.GetAsync(restUri).Result;

    if (response.IsSuccessStatusCode) {
      return response.Content.ReadAsStringAsync().Result;
    }
    else {
      throw new ApplicationException("ERROR executing HTTP GET request " + response.StatusCode);
    }
  }

  private static string ExecutePostRequest(string endpoint, string postBody = "") {

    string restUri = AppSettings.FabricUserApiBaseUrl + endpoint;

    HttpContent body = new StringContent(postBody);
    body.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

    HttpResponseMessage response = client.PostAsync(restUri, body).Result;

    // switch to handle responses with different status codes
    switch (response.StatusCode) {

      // handle case when sync call succeeds with OK (200) or CREATED (201)
      case HttpStatusCode.OK:
      case HttpStatusCode.Created:
      case HttpStatusCode.NoContent:
        Console.WriteLine();
        // return result to caller
        return response.Content.ReadAsStringAsync().Result;

      // handle case where call started async operation with ACCEPTED (202)
      case HttpStatusCode.Accepted:
        Console.Write(".");

        // get headers in response with URL for operation status and retry interval
        string operationUrl = response.Headers.GetValues("Location").First();
        //int retryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());
        int retryAfter = 10; // hard-coded during testing - use what's above instead 

        // execute GET request with operation url until it returns OK (200)
        string jsonOperation;
        FabricOperation operation;

        do {
          Thread.Sleep(retryAfter * 1000);  // wait for retry interval 
          Console.Write(".");
          response = client.GetAsync(operationUrl).Result;
          jsonOperation = response.Content.ReadAsStringAsync().Result;
          operation = JsonSerializer.Deserialize<FabricOperation>(jsonOperation);

        } while (operation.status != "Succeeded" &&
                 operation.status != "Failed" &&
                 operation.status != "Completed");

        if (response.StatusCode == HttpStatusCode.OK) {
          // handle 2 cases where operation completed successfully
          if (!response.Headers.Contains("Location")) {
            // (1) handle case where operation has no result
            Console.WriteLine();
            return string.Empty;
          }
          else {
            Console.Write(".");
            // (2) handle case where operation has result by retrieving it
            response = client.GetAsync(operationUrl + "/result").Result;
            Console.WriteLine();
            return response.Content.ReadAsStringAsync().Result;
          }
        }
        else {
          // handle case where operation experienced error
          jsonOperation = response.Content.ReadAsStringAsync().Result;
          operation = JsonSerializer.Deserialize<FabricOperation>(jsonOperation);
          string errorMessage = operation.error.errorCode + " - " + operation.error.message;
          throw new ApplicationException(errorMessage);
        }

      default: // handle exeception where HTTP status code indicates failure
        Console.WriteLine();
        throw new ApplicationException("ERROR executing HTTP POST request " + response.StatusCode);
    }

  }

  private static string ExecutePostRequestForLoadTable(string endpoint, string postBody = "") {

    string restUri = AppSettings.FabricUserApiBaseUrl + endpoint;

    HttpContent body = new StringContent(postBody);
    body.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

    HttpResponseMessage response = client.PostAsync(restUri, body).Result;

    // switch to handle responses with different status codes
    switch (response.StatusCode) {

      // handle case when sync call succeeds with OK (200) or CREATED (201)
      case HttpStatusCode.OK:
      case HttpStatusCode.Created:
        Console.WriteLine();
        // return result to caller
        return response.Content.ReadAsStringAsync().Result;

      // handle case where call started async operation with ACCEPTED (202)
      case HttpStatusCode.Accepted:
        Console.Write(".");

        // get headers in response with URL for operation status and retry interval
        string operationUrl = response.Headers.GetValues("Location").First();
        int retryAfter = int.Parse(response.Headers.GetValues("Retry-After").First());

        // execute GET request with operation url until it returns OK (200)
        string jsonOperation;
        FabricTableOperation operation;

        do {
          Thread.Sleep(retryAfter * 1000);  // wait for retry interval 
          Console.Write(".");
          response = client.GetAsync(operationUrl).Result;
          jsonOperation = response.Content.ReadAsStringAsync().Result;
          operation = JsonSerializer.Deserialize<FabricTableOperation>(jsonOperation);

        } while (operation.Status != 3 &&
                 operation.Error == null);

        if (operation.Error != null) {
          Console.WriteLine();
          Console.WriteLine();
          Console.WriteLine("Error");
          Console.WriteLine(" - Message: " + operation.Error.message);
          Console.WriteLine(" - Error Code: " + operation.Error.errorCode);
          Console.WriteLine();
          return "";
        }

        else {
          if (response.StatusCode == HttpStatusCode.OK) {
            // handle 2 cases where operation completed successfully
            if (!response.Headers.Contains("Location")) {
              // (1) handle case where operation has no result
              Console.WriteLine();
              return string.Empty;
            }
            else {
              Console.Write(".");
              // (2) handle case where operation has result by retrieving it
              response = client.GetAsync(operationUrl + "/result").Result;
              Console.WriteLine();
              return response.Content.ReadAsStringAsync().Result;
            }
          }
          else {
            // handle case where operation experienced error
            jsonOperation = response.Content.ReadAsStringAsync().Result;
            operation = JsonSerializer.Deserialize<FabricTableOperation>(jsonOperation);
            string errorMessage = operation.Error.errorCode + " - " + operation.Error.message;
            throw new ApplicationException(errorMessage);
          }
        }



      default: // handle exeception where HTTP status code indicates failure
        Console.WriteLine();
        throw new ApplicationException("ERROR executing HTTP POST request " + response.StatusCode);
    }

  }

  private static string ExecutePatchRequest(string endpoint, string postBody = "") {

    string restUri = AppSettings.FabricUserApiBaseUrl + endpoint;

    HttpContent body = new StringContent(postBody);
    body.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

    HttpResponseMessage response = client.PatchAsync(restUri, body).Result;

    if (response.IsSuccessStatusCode) {
      return response.Content.ReadAsStringAsync().Result;
    }
    else {
      throw new ApplicationException("ERROR executing HTTP PATCH request " + response.StatusCode);
    }
  }

  private static string ExecuteDeleteRequest(string endpoint) {
    string restUri = AppSettings.FabricUserApiBaseUrl + endpoint;

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);
    HttpResponseMessage response = client.DeleteAsync(restUri).Result;

    if (response.IsSuccessStatusCode) {
      return response.Content.ReadAsStringAsync().Result;
    }
    else {
      throw new ApplicationException("ERROR executing HTTP DELETE request " + response.StatusCode);
    }
  }

  private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
  };

  #endregion

  public static List<FabricWorkspace> GetWorkspaces() {
    string jsonResponse = ExecuteGetRequest("/workspaces");
    return JsonSerializer.Deserialize<FabricWorkspaceList>(jsonResponse).value;
  }

  public static List<FabricCapacity> GetCapacities() {
    string jsonResponse = ExecuteGetRequest("/capacities");
    return JsonSerializer.Deserialize<FabricCapacityList>(jsonResponse).value;
  }

  public static FabricWorkspace GetWorkspaceByName(string WorkspaceName) {

    string jsonResponse = ExecuteGetRequest("/workspaces");
    var workspaces = JsonSerializer.Deserialize<FabricWorkspaceList>(jsonResponse).value;

    foreach (FabricWorkspace workspace in workspaces) {
      if (workspace.displayName.Equals(WorkspaceName)) {
        return workspace;
      }
    }

    return null;
  }

  public static FabricWorkspace CreateWorkspace(string WorkspaceName, string CapacityId = AppSettings.PremiumCapacityId, string Description = null) {

    FabricWorkspace workspace = GetWorkspaceByName(WorkspaceName);

    Console.WriteLine();
    Console.Write(" - Creating workspace named " + WorkspaceName);

    // delete workspace with same name if it exists
    if (workspace != null) {
      ExecuteDeleteRequest("/workspaces/" + workspace.id);
      workspace = null;
    }

    var workspaceCreateRequest = new FabricWorkspaceCreateRequest {
      displayName = WorkspaceName,
      description = Description,
      capacityId = CapacityId
    };

    string requestBody = JsonSerializer.Serialize(workspaceCreateRequest, jsonOptions);

    string jsonResponse = ExecutePostRequest("/workspaces", requestBody);

    workspace = JsonSerializer.Deserialize<FabricWorkspace>(jsonResponse);

    Console.WriteLine("   > Workspace created with id of {0}", workspace.id);
    Console.WriteLine("   > Workspace associated with capacity id of {0}", workspaceCreateRequest.capacityId);
    Console.WriteLine();

    return workspace;
  }

  public static FabricWorkspace UpdateWorkspace(string WorkspaceId, string WorkspaceName, string Description = null) {

    var workspaceUpdateRequest = new FabricWorkspaceUpdateRequest {
      displayName = WorkspaceName,
      description = Description
    };

    string requestBody = JsonSerializer.Serialize(workspaceUpdateRequest, jsonOptions);

    string jsonResponse = ExecutePatchRequest("/workspaces/" + WorkspaceId, requestBody);

    FabricWorkspace workspace = JsonSerializer.Deserialize<FabricWorkspace>(jsonResponse);

    Console.WriteLine("   > Workspace uodated wtith new name of " + workspace.displayName);
    Console.WriteLine();

    return workspace;
  }

  public static void AssignWorkspaceToCapacity(string WorkspaceId, string CapacityId) {

    Console.WriteLine(" - Assigning workspace to capacity with id of " + CapacityId);

    string restUrl = "/workspaces/" + WorkspaceId + "/assignToCapacity";

    string postBody = "{ \"capacityId\": \"" + CapacityId + "\" }";

    // this call returns async 202 ACCEPTED
    string jsonResponse = ExecutePostRequest(restUrl, postBody);

  }

  public static void AddWorkspaceUser(string WorkspaceId, string UserId, string RoleAssignment) {
    Console.WriteLine(" - Adding workspace role assignment to user with id of " + UserId);

    string restUrl = "/workspaces/" + WorkspaceId + "/roleAssignments";

    FabricWorkspaceRoleAssignment roleAssignment =
      new FabricWorkspaceRoleAssignment {
        role = RoleAssignment,
        principal = new Principal {
          id = UserId,
          type = PrincipalType.User
        },
      };


    string postBody = JsonSerializer.Serialize(roleAssignment, jsonOptions);

    ExecutePostRequest(restUrl, postBody);

  }

  public static void AddWorkspaceGroup(string WorkspaceId, string GroupId, string RoleAssignment) {
    Console.WriteLine(" - Adding workspace role assignment to AAD Group with id of " + GroupId);

    string restUrl = "/workspaces/" + WorkspaceId + "/roleAssignments";

    FabricWorkspaceRoleAssignment roleAssignment =
      new FabricWorkspaceRoleAssignment {
        role = RoleAssignment,
        principal = new Principal {
          id = GroupId,
          type = PrincipalType.Group
        },
      };

    string postBody = JsonSerializer.Serialize(roleAssignment, jsonOptions);

    ExecutePostRequest(restUrl, postBody);

  }

  public static void AddWorkspaceServicePrincipal(string WorkspaceId, string ServicePrincipalObjectId, string RoleAssignment) {
    Console.WriteLine(" - Adding workspace role assignment to service principal with id of " + ServicePrincipalObjectId);

    string restUrl = "/workspaces/" + WorkspaceId + "/roleAssignments";

    FabricWorkspaceRoleAssignment roleAssignment =
      new FabricWorkspaceRoleAssignment {
        role = RoleAssignment,
        principal = new Principal {
          id = ServicePrincipalObjectId,
          type = PrincipalType.ServicePrincipal
        },
      };

    string postBody = JsonSerializer.Serialize(roleAssignment, jsonOptions);

    ExecutePostRequest(restUrl, postBody);

  }

  public static void ViewWorkspaceRoleAssignments(string WorkspaceId) {

    string restUrl = "/workspaces/" + WorkspaceId + "/roleAssignments";
    string jsonResponse = ExecuteGetRequest(restUrl);

    Console.WriteLine(" - Workspace membership list");
    var roleAssignments = JsonSerializer.Deserialize<FabricWorkspaceRoleAssignmentList>(jsonResponse).value;
    foreach (var roleAssignment in roleAssignments) {
      Console.WriteLine("    > " +
                        roleAssignment.principal.displayName +
                        " (" + roleAssignment.principal.type + ") " +
                        "added in role of " + roleAssignment.role);
    }

  }

  public static FabricItem CreateLakehouse(string WorkspaceId, string LakehouseName) {

    // Item create request for lakehouse des not include item definition
    FabricItemCreateRequest createRequestLakehouse = new FabricItemCreateRequest {
      displayName = LakehouseName,
      type = "Lakehouse"
    };

    // create lakehouse
    FabricItem lakehouse = CreateItem(WorkspaceId, createRequestLakehouse);

    return lakehouse;
  }

  public static FabricLakehouse GetLakehouse(string WorkspaceId, string LakehousId) {
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/lakehouses/" + LakehousId);
    FabricLakehouse lakehouse = JsonSerializer.Deserialize<FabricLakehouse>(jsonResponse);
    return lakehouse;
  }

  public static FabricLakehouse GetLakehouseByName(string WorkspaceId, string LakehouseName) {

    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/items?type=Lakehouse");
    var items = JsonSerializer.Deserialize<FabricItemList>(jsonResponse).value;

    foreach (var lakehouse in items) {
      if (lakehouse.displayName == LakehouseName) {
        return GetLakehouse(WorkspaceId, lakehouse.id);
      }
    }

    return null;
  }

  public static FabricSqlEndpoint GetSqlEndpointForLakehouse(string WorkspaceId, string LakehouseId) {

    var lakehouse = GetLakehouse(WorkspaceId, LakehouseId);

    while (lakehouse.properties.sqlEndpointProperties.provisioningStatus != "Success") {
      lakehouse = GetLakehouse(WorkspaceId, LakehouseId);
      Thread.Sleep(12000);
      Console.Write(".");
    }
    Console.WriteLine();

    return new FabricSqlEndpoint {
      server = lakehouse.properties.sqlEndpointProperties.connectionString,
      database = lakehouse.properties.sqlEndpointProperties.id
    };

  }

  public static void ExportItemDefinitionsFromWorkspace(string WorkspaceName) {

    Console.WriteLine("Exporting workspaces items from {0}", WorkspaceName);

    FabricItemDefinitionFactory.DeleteAllTemplateFiles(WorkspaceName);

    FabricWorkspace workspace = GetWorkspaceByName(WorkspaceName);
    string jsonResponse = ExecuteGetRequest("/workspaces/" + workspace.id + "/items");

    var items = JsonSerializer.Deserialize<FabricItemList>(jsonResponse).value;

    // list of items types that do not support getItemDefinition
    List<string> unsupportedItems = new List<string>() {
      FabricItemType.Lakehouse, FabricItemType.SQLEndpoint, FabricItemType.Warehouse ,
      FabricItemType.Dashboard, FabricItemType.PaginatedReport
    };

    foreach (var item in items) {
      if (!unsupportedItems.Contains(item.type)) {
        try {
          string restUrl = "/workspaces/" + workspace.id + "/items/" + item.id + "/getDefinition";
          if (item.type == FabricItemType.Notebook) {
            restUrl += "?format=ipynb";
          }
          string jsonResponseItem = ExecutePostRequest(restUrl);
          FabricItemDefinitionResponse definitionResponse = JsonSerializer.Deserialize<FabricItemDefinitionResponse>(jsonResponseItem);
          FabricItemDefinition definition = definitionResponse.definition;
          string targetFolder = item.displayName + "." + item.type;
          Console.Write(" > Exporting " + targetFolder);
          foreach (var part in definition.parts) {
            FabricItemDefinitionFactory.WriteFile(WorkspaceName, targetFolder, part.path, part.payload);
          }

          var ItemMetadata = JsonSerializer.Serialize(new {
            type = item.type,
            displayName = item.displayName
          }, jsonOptions);

          FabricItemDefinitionFactory.WriteFile(WorkspaceName, targetFolder, "item.metadata.json", ItemMetadata, false);

        }
        catch (Exception ex) {
          Console.WriteLine(" *** Error exporting " + item.type + " named " + item.displayName);
          Console.WriteLine(" *** " + ex.Message);

        }
        // slow up calls so it doesn't trigger throttleing for more than 10+ calls per minute
        Thread.Sleep(7000);
      }
    }

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("Exporting process completed");
    Console.WriteLine();

  }

  public static FabricItem CreateItem(string WorkspaceId, FabricItemCreateRequest ItemCreateRequest) {

    string displayMessage = string.Format(" - Creating {0} named {1}", ItemCreateRequest.type, ItemCreateRequest.displayName);
    Console.Write(displayMessage);


    string postBody = JsonSerializer.Serialize(ItemCreateRequest, jsonOptions);
    string jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/items", postBody);
    FabricItem newItem = JsonSerializer.Deserialize<FabricItem>(jsonResponse);

    Console.WriteLine("   > " + newItem.type + " created with Id " + newItem.id);
    Console.WriteLine();

    // return new item object to caller
    return newItem;
  }

  public static FabricItem UpdateItem(string WorkspaceId, string ItemId, string ItemName, string Description = null) {
    // NOTE: UpdateItem API does not work in initial Public preview release 

    var itemUpdateRequest = new FabricItemUpdateRequest {
      displayName = ItemName,
      description = Description
    };

    string requestBody = JsonSerializer.Serialize(itemUpdateRequest, jsonOptions);

    string jsonResponse = ExecutePatchRequest("/workspaces/" + WorkspaceId + "/items/" + ItemId, requestBody);

    FabricItem item = JsonSerializer.Deserialize<FabricItem>(jsonResponse);

    Console.WriteLine("   > Item uodated wtith new name of " + item.displayName);
    Console.WriteLine();

    return item;

  }

  public static FabricItemDefinitionResponse GetItemDefinitions(string WorkspaceId, string ItemId) {
    string jsonResponseItem = ExecutePostRequest("/workspaces/" + WorkspaceId + "/items/" + ItemId + "/getDefinition");
    return JsonSerializer.Deserialize<FabricItemDefinitionResponse>(jsonResponseItem);
  }

  public static void UpdateItemDefinition(string WorkspaceId, string ItemId, FabricItemUpdateDefinitionRequest ItemUpdateDefinitionRequest) {
    string postBody = JsonSerializer.Serialize(ItemUpdateDefinitionRequest, jsonOptions);
    ExecutePostRequest("/workspaces/" + WorkspaceId + "/items/" + ItemId + "/updateDefinition", postBody);
  }

  public static void RunNotebook(string WorkspaceId, FabricItem Item) {

    Console.Write(" - Running notebook named " + Item.displayName);

    string restUrl = "/workspaces/" + WorkspaceId + "/items/" + Item.id + "/jobs/instances?jobType=RunNotebook";

    string jsonResponse = ExecutePostRequest(restUrl, "{ \"executionData\": {} }");

    Console.WriteLine("   > Notebook execution completed");
    Console.WriteLine();
  }

  public static FabricItem CreateWarehouse(string WorkspaceId, string WarehouseName) {

    // Item create request for lakehouse des not include item definition
    FabricItemCreateRequest createRequestWarehouse = new FabricItemCreateRequest {
      displayName = WarehouseName,
      type = "Warehouse"
    };

    // create warehouse
    FabricItem warehouse = CreateItem(WorkspaceId, createRequestWarehouse);

    return warehouse;
  }

  public static FabricWarehouse GetWareHouseByName(string WorkspaceId, string WarehouseName) {

    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/items?type=Warehouse");
    var items = JsonSerializer.Deserialize<FabricItemList>(jsonResponse).value;

    foreach (var warehouse in items) {
      if (warehouse.displayName == WarehouseName) {
        return GetWarehouse(WorkspaceId, warehouse.id);
      }
    }

    return null;
  }

  public static FabricWarehouse GetWarehouse(string WorkspaceId, string WarehouseId) {
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/warehouses/" + WarehouseId);
    FabricWarehouse lakehouse = JsonSerializer.Deserialize<FabricWarehouse>(jsonResponse);
    return lakehouse;
  }

  public static string GetWarehouseConnection(string WorkspaceId, string WarehouseId) {

    string connectionInfo = "";

    while (connectionInfo.Equals("")) {

      try {
        var warehouse = GetWarehouse(WorkspaceId, WarehouseId);
        connectionInfo = warehouse.properties.connectionInfo;
        return connectionInfo;
      }
      catch (Exception ex) {
        // handle NotFound error
        if (ex.Message.Contains("NotFound")) {
          // do nothing - keep looping
          Console.Write(".");
        }
        else {
          throw;
        }
      }

      Thread.Sleep(6000);
    }
    return "";
  }

  public static void LoadLakehouseTableFromCsv(string WorkspaceId, string LakehouseId, string SourceFile, string TableName) {

    Console.Write(" - Loading table named {0} from {1}", TableName, SourceFile);

    string restUrl = "/workspaces/" + WorkspaceId + "/lakehouses/" + LakehouseId + "/tables/" + TableName + "/load";

    var tableLoadRequest = new FabricTableLoadRequest {
      pathType = "File",
      mode = "Overwrite",
      recursive = false,
      relativePath = SourceFile,
      formatOptions = new FabricTableLoadRequestFormatOptions {
        format = "Csv",
        delimiter = ",",
        header = true
      }
    };

    string postBody = JsonSerializer.Serialize(tableLoadRequest, jsonOptions);
    string jsonResponse = ExecutePostRequestForLoadTable(restUrl, postBody);

  }

  public static void LoadLakehouseTableFromParquet(string WorkspaceId, string LakehouseId, string SourceFile, string TableName, bool Append = false) {

    Console.Write(" - Loading table named {0} from {1}", TableName, SourceFile);

    string restUrl = "/workspaces/" + WorkspaceId + "/lakehouses/" + LakehouseId + "/tables/" + TableName + "/load";

    var tableLoadRequest = new FabricTableLoadRequest {
      pathType = "File",
      mode = Append ? "Append" : "Overwrite",
      recursive = false,
      relativePath = SourceFile,
      formatOptions = new FabricTableLoadRequestFormatOptions {
        format = "Parquet"
      }
    };

    string postBody = JsonSerializer.Serialize(tableLoadRequest, jsonOptions);
    string jsonResponse = ExecutePostRequestForLoadTable(restUrl, postBody);

  }

  public static LakehouseShortcut CreateLakehouseShortcut(string WorkspaceId, string LakehouseId, 
                                                          LakehouseShortcutCreateRequest shortcutCreateRequest) {

    Console.Write(string.Format(" - Creating shortcut named {0}", shortcutCreateRequest.name));    
    string postBody = JsonSerializer.Serialize(shortcutCreateRequest, jsonOptions);
    string jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/items/" + LakehouseId + "/shortcuts", postBody);
    return JsonSerializer.Deserialize<LakehouseShortcut>(jsonResponse);
  }

  public static List<LakehouseTable> GetLakehouseTables(string WorkspaceId, string LakehousId) {
    // needs to be updated to inspect continuation token
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/lakehouses/" + LakehousId + "/tables");
    return (JsonSerializer.Deserialize<LakehouseListTableResult>(jsonResponse)).data;
  }

  public static void GetLakehouseShortcuts(string WorkspaceId, string LakehouseId) {
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/items/" + LakehouseId + "/shortcuts");
    Console.WriteLine(jsonResponse);
  }

  // internal API call - not for public usage
  public static void RefreshLakehouseSchema(string LakehouseId) {

    string restUri = $"https://api.powerbi.com/v1.0/myorg/lhdatamarts/{LakehouseId}";
    string postBody = "{datamartVersion: 5, commands: [{$type: \"MetadataRefreshCommand\"}]}";
    HttpContent body = new StringContent(postBody);
    body.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

    HttpResponseMessage response = client.PostAsync(restUri, body).Result;
  }


}

