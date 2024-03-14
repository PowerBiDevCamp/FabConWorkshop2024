﻿
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

}
