
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

  public static void ExportItemDefinitionsFromWorkspace(string WorkspaceName) {

    Console.WriteLine("Exporting workspaces items from {0}", WorkspaceName);

    FabricItemDefinitionFactory.DeleteAllTemplateFiles(WorkspaceName);

    FabricWorkspace workspace = GetWorkspaceByName(WorkspaceName);
    string jsonResponse = ExecuteGetRequest("/workspaces/" + workspace.id + "/items");

    var items = JsonSerializer.Deserialize<FabricItemList>(jsonResponse).value;

    List<string> unsupportedItems = new List<string>() { FabricItemType.Lakehouse, FabricItemType.SQLEndpoint, FabricItemType.Dashboard, FabricItemType.PaginatedReport };
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

  public static FabricItem GetSemanticModelByName(string WorkspaceId, string ModelName) {

    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/items?type=SemanticModel");
    var items = JsonSerializer.Deserialize<FabricItemList>(jsonResponse).value;

    foreach (var model in items) {
      if (model.displayName == ModelName) {
        return model;
      }
    }

    return null;
  }

  public static void ConnectWorkspaceToGitRepository(string WorkspaceId, GitProviderConnection connectionRequest) {

    Console.Write(" - Connecting workspace to Azure Dev Ops");

    // (1) establish connection
    string postBody = JsonSerializer.Serialize(connectionRequest, jsonOptions);
    string jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/connect", postBody);
    Console.Write("   > GIT connection established between workspace and Azure Dev Ops");

    // (2) initialize connection
    var initRequest = new GitConnectionInitRequest {
      initializationStrategy = GitInitializationStrategy.PreferWorkspace
    };

    postBody = JsonSerializer.Serialize(initRequest, jsonOptions);
    jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/initializeConnection", postBody);

    var initResponse = JsonSerializer.Deserialize<GitInitializationResonse>(jsonResponse);

    if (initResponse.requiredAction == GitRequiredAction.CommitToGit) {
      // (2A) commit workspace changes to GIT
      Console.Write("   > Committing changes to GIT repository");
      var commitRequest = new CommitToGitRequest {
        comment = "Required commit after initializing connection",
        mode = "All",
        workspaceHead = initResponse.workspaceHead
      };
      postBody = JsonSerializer.Serialize(commitRequest, jsonOptions);
      jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/commitToGit", postBody);
      Console.WriteLine("   > Workspace changes committed to GIT");
    }

    if (initResponse.requiredAction == GitRequiredAction.UpdateFromGit) {
      // (2B) update workspace from source files in GIT
      Console.Write("   > Updating workspace from source files in GIT");
      var updateFromGitRequest = new UpdateFromGitRequest {
        remoteCommitHash = initResponse.remoteCommitHash,
        conflictResolution = new ConflictResolution {
          conflictResolutionPolicy = "PreferRemote",
          conflictResolutionType = "Workspace"
        },
        options = new ConflictResolutionOptions {
          allowOverrideItems = true
        }
      };
      postBody = JsonSerializer.Serialize(updateFromGitRequest, jsonOptions);
      jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/updateFromGit", postBody);
      Console.WriteLine("   > Workspace updated from source files in GIT");
    }

    Console.WriteLine("   > Workspace connection intialization complete");
    Console.WriteLine();

  }

  public static void DisconnectWorkspaceFromGitRepository(string WorkspaceId) {
    ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/disconnect");
  }

  public static GitProviderConnection GetWorkspaceGitConnection(string WorkspaceId) {
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/git/connection");
    return JsonSerializer.Deserialize<GitProviderConnection>(jsonResponse);
  }

  public static GitGetStatusResponse GetWorkspaceGitStatus(string WorkspaceId) {
    string jsonResponse = ExecuteGetRequest("/workspaces/" + WorkspaceId + "/git/status");
    var status = JsonSerializer.Deserialize<GitGetStatusResponse>(jsonResponse);
    return status;
  }

  public static void CommitWorkspaceToGit(string WorkspaceId) {
    Console.Write(" - Committing workspace changes to GIT");

    var gitStatus = GetWorkspaceGitStatus(WorkspaceId);

    var commitRequest = new CommitToGitRequest {
      comment = "Workspaces changes after semantic model refresh",
      mode = "All",
      workspaceHead = gitStatus.workspaceHead,
    };

    string postBody = JsonSerializer.Serialize(commitRequest, jsonOptions);
    string jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/commitToGit", postBody);
    Console.WriteLine("   > Workspace changes committed to GIT");
    Console.WriteLine();
  }

  public static void UpdateWorkspaceFromGit(string WorkspaceId) {

    Console.Write(" - Syncing updates to workspace from GIT");

    var gitStatus = GetWorkspaceGitStatus(WorkspaceId);

    var updateFromGitRequest = new UpdateFromGitRequest {
      remoteCommitHash = gitStatus.remoteCommitHash,
      workspaceHead = gitStatus.workspaceHead,
      conflictResolution = new ConflictResolution {
        conflictResolutionPolicy = "PreferWorkspace",
        conflictResolutionType = "Workspace"
      },
      options = new ConflictResolutionOptions {
        allowOverrideItems = true
      }
    };

    string postBody = JsonSerializer.Serialize(updateFromGitRequest, jsonOptions);

    string jsonResponse = ExecutePostRequest("/workspaces/" + WorkspaceId + "/git/updateFromGit", postBody);

    Console.WriteLine("   > Sync from GIT complete");
    Console.WriteLine();
  }

}

