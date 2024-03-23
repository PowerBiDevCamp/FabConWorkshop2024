using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Text.Json;


public class AdoProjectManager {

  #region "Utility constants and methods for executing HTTP requests"

  private const string AdoProjectTemplateId = "b8a3a935-7e91-48b8-a94c-606d37c3e9f2";
  private const string AdoApiVersion = "api-version=7.1";

  private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
  };

  private static readonly string[] AdoPermissionScopes = new string[] {
      "499b84ac-1321-427f-aa17-267ca6975798/user_impersonation"
    };

  private static string AccessToken = AzureAdTokenManager.GetAccessToken(AdoPermissionScopes);

  private static string ExecuteGetRequest(string endpoint) {

    string restUri = AppSettings.AzureDevOpsApiBaseUrl + endpoint;

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

    string restUri = AppSettings.AzureDevOpsApiBaseUrl + endpoint + "?" + AdoApiVersion;

    HttpContent body = new StringContent(postBody);
    body.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/json");

    HttpClient client = new HttpClient();
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + AccessToken);

    HttpResponseMessage response = client.PostAsync(restUri, body).Result;

    return response.Content.ReadAsStringAsync().Result;

  }

  private static string ExecutePatchRequest(string endpoint, string postBody = "") {

    string restUri = AppSettings.AzureDevOpsApiBaseUrl + endpoint + "?" + AdoApiVersion;

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

    string restUri = AppSettings.AzureDevOpsApiBaseUrl + endpoint + "?" + AdoApiVersion;

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

  #endregion

  public static List<ADOProject> GetProjects() {
    string jsonResponse = ExecuteGetRequest("/_apis/projects");
    return JsonSerializer.Deserialize<ADOProjectList>(jsonResponse).value;
  }

  public static void DisplayProjects() {
    string jsonResponse = ExecuteGetRequest("/_apis/projects");
    var projects = JsonSerializer.Deserialize<ADOProjectList>(jsonResponse).value;

    Console.WriteLine("ADO Project List");
    foreach (var project in projects) {
      Console.WriteLine(" * " + project.name);
    }
    Console.WriteLine();
  }

  public static ADOProject GetProject(string Name) {

    var projects = GetProjects();

    foreach (var project in projects) {
      if (project.name == Name) {
        return project;
      }
    }

    return null;
  }

  public static void CreateProject(string Name) {

    Console.WriteLine(" - Creating new project in Azure Dev Ops");

    var existingProject = GetProject(Name);

    if (existingProject != null) {
      DeleteProject(existingProject.id);
    }

    var createRequest = new ADOProjectCreateRequest {
      name = Name,
      description = "ADO Project used to demonstrate Fabric GIT Integration",
      visibility = 0,
      capabilities = new ADOCapabilities {
        versioncontrol = new ADOVersioncontrol {
          sourceControlType = "Git"
        },
        processTemplate = new ADOProcessTemplate {
          templateTypeId = AdoProjectTemplateId
        }
      }
    };

    string postBody = JsonSerializer.Serialize(createRequest, jsonOptions);

    string jsonResponse = ExecutePostRequest("/_apis/projects", postBody);

    var createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);

    while (createOperation.status != "succeeded") {
      Thread.Sleep(3000);
      jsonResponse = ExecuteGetRequest("/_apis/operations/" + createOperation.id);
      createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
    }

    Console.WriteLine("   > Project successfully created");

    // get main respository for new project
    jsonResponse = ExecuteGetRequest("/" + Name + "/_apis/git/repositories");
    var mainRepository = JsonSerializer.Deserialize<ADORepositoryList>(jsonResponse).value[0];

    // constructl URL for push requests to main repository
    string pushUrl = "/" + Name + "/_apis/git/repositories/" + mainRepository.id + "/pushes";

    // submit initial push request with ReadMe.md
    ADOPushRequest pushRequest = GetInitialPushRequestWithReadMe(Name);
    string pushPostBody = JsonSerializer.Serialize(pushRequest, jsonOptions);
    jsonResponse = ExecutePostRequest(pushUrl, pushPostBody);
    Console.WriteLine("   > Project intialized with ReadMe.md");

  }

  public static void CreateProject(string Name, FabricWorkspace Workspace) {

    Console.WriteLine(" - Creating new project in Azure Dev Ops");

    var existingProject = GetProject(Name);

    if (existingProject != null) {
      DeleteProject(existingProject.id);
    }

    var createRequest = new ADOProjectCreateRequest {
      name = Name,
      description = "ADO Project used to demonstrate Fabric GIT Integration",
      visibility = 0,
      capabilities = new ADOCapabilities {
        versioncontrol = new ADOVersioncontrol {
          sourceControlType = "Git"
        },
        processTemplate = new ADOProcessTemplate {
          templateTypeId = AdoProjectTemplateId
        }
      }
    };

    string postBody = JsonSerializer.Serialize(createRequest, jsonOptions);

    string jsonResponse = ExecutePostRequest("/_apis/projects", postBody);

    var createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);

    while (createOperation.status != "succeeded") {
      Thread.Sleep(3000);
      jsonResponse = ExecuteGetRequest("/_apis/operations/" + createOperation.id);
      createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
    }

    Console.WriteLine("   > Project successfully created");

    // get main respository for new project
    jsonResponse = ExecuteGetRequest("/" + Name + "/_apis/git/repositories");
    var mainRepository = JsonSerializer.Deserialize<ADORepositoryList>(jsonResponse).value[0];

    // constructl URL for push requests to main repository
    string pushUrl = "/" + Name + "/_apis/git/repositories/" + mainRepository.id + "/pushes";

    // submit initial push request with ReadMe.md
    ADOPushRequest pushRequest = GetInitialPushRequestWithReadMe(Name, Workspace);
    string pushPostBody = JsonSerializer.Serialize(pushRequest, jsonOptions);
    jsonResponse = ExecutePostRequest(pushUrl, pushPostBody);
    Console.WriteLine("   > Project intialized with ReadMe.md");

    // capture push response
    var pushResponse = JsonSerializer.Deserialize<ADOPushResponse>(jsonResponse, jsonOptions);

    // get new object Id from push response
    string lastObjectId = pushResponse.refUpdates[0].newObjectId;

    Console.WriteLine();


  }

  public static void CreateProjectWithImportModeSourceFile(string Name, FabricWorkspace Workspace) {

    Console.WriteLine(" - Creating new project in Azure Dev Ops");

    var existingProject = GetProject(Name);

    if (existingProject != null) {
      DeleteProject(existingProject.id);
    }

    var createRequest = new ADOProjectCreateRequest {
      name = Name,
      description = "ADO Project used to demonstrate Fabric GIT Integration",
      visibility = 0,
      capabilities = new ADOCapabilities {
        versioncontrol = new ADOVersioncontrol {
          sourceControlType = "Git"
        },
        processTemplate = new ADOProcessTemplate {
          templateTypeId = AdoProjectTemplateId
        }
      }
    };

    string postBody = JsonSerializer.Serialize(createRequest, jsonOptions);

    string jsonResponse = ExecutePostRequest("/_apis/projects", postBody);

    var createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);

    while (createOperation.status != "succeeded") {
      Thread.Sleep(3000);
      jsonResponse = ExecuteGetRequest("/_apis/operations/" + createOperation.id);
      createOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
    }

    // get main respository for new project
    jsonResponse = ExecuteGetRequest("/" + Name + "/_apis/git/repositories");
    var mainRepository = JsonSerializer.Deserialize<ADORepositoryList>(jsonResponse).value[0];

    // constructl URL for push requests to main repository
    string pushUrl = "/" + Name + "/_apis/git/repositories/" + mainRepository.id + "/pushes";

    // submit initial push request with ReadMe.md
    ADOPushRequest pushRequest = GetInitialPushRequestWithReadMe(Name, Workspace);
    string pushPostBody = JsonSerializer.Serialize(pushRequest, jsonOptions);
    jsonResponse = ExecutePostRequest(pushUrl, pushPostBody);
    Console.WriteLine("   > Uploading and committing ReadMe.md to intialize ADO repository");

    // capture push response
    var pushResponse = JsonSerializer.Deserialize<ADOPushResponse>(jsonResponse, jsonOptions);

    // get new object Id from push response
    string lastObjectId = pushResponse.refUpdates[0].newObjectId;

    Console.WriteLine("   > Uploading and committing PBIP sources files ADO repository");

    pushRequest = GetPushRequestWithImportModeSourceFiles(lastObjectId);
    pushPostBody = JsonSerializer.Serialize(pushRequest, jsonOptions);
    ExecutePostRequest(pushUrl, pushPostBody);

    Console.WriteLine("   > Azure Dev Ops project initialization complete");
    Console.WriteLine();
  }

  private static string GetPartPath(string ItemFolderPath, string FilePath) {
    int ItemFolderPathOffset = ItemFolderPath.Length; // + 1;
    return FilePath.Substring(ItemFolderPathOffset).Replace("\\", "/");
  }

  public static ADOPushRequest GetInitialPushRequestWithReadMe(string Name) {

    // update markdown content for ReadMe.md
    string ReadMeContent = "# ADO Project used for GIT Integration with Fabric Workspace";

    return new ADOPushRequest {
      refUpdates = new List<ADORef> {
          new ADORef {
            name = "refs/heads/main",
            oldObjectId = "0000000000000000000000000000000000000000"
          }
        },
      commits = new List<ADOCommit> {
          new ADOCommit {
            comment = "Initial commit with ReadMe.md",
            changes = new List<ADOChange> {
              new ADOChange {
                changeType = "add",
                item = new ADOItem {
                  path = "/README.md"
                },
                newContent = new ADONewContent {
                  content = ReadMeContent,
                  contentType = "rawtext"
                }
              }
            }
          }
        }
    };
  }

  public static ADOPushRequest GetInitialPushRequestWithReadMe(string Name, FabricWorkspace Workspace) {

    // update markdown content for ReadMe.md
    string ReadMeContent = Demo3_GitIntegration.Properties.Resources.AdoReadMe_md
                                           .Replace("{WORKSPACE_NAME}", Name)
                                           .Replace("{WORKSPACE_ID}", Workspace.id);

    return new ADOPushRequest {
      refUpdates = new List<ADORef> {
          new ADORef {
            name = "refs/heads/main",
            oldObjectId = "0000000000000000000000000000000000000000"
          }
        },
      commits = new List<ADOCommit> {
          new ADOCommit {
            comment = "Initial commit with ReadMe.md",
            changes = new List<ADOChange> {
              new ADOChange {
                changeType = "add",
                item = new ADOItem {
                  path = "/README.md"
                },
                newContent = new ADONewContent {
                  content = ReadMeContent,
                  contentType = "rawtext"
                }
              }
            }
          }
        }
    };
  }

  public static ADOPushRequest GetPushRequestWithImportModeSourceFiles(string LastObjectId) {

    string folderPath = AppSettings.LocalGitIntegrationSource;
    List<string> ignoredFilePaths = new List<string> { "localSettings.json", @"\.pbi\" };
    List<string> filesInFolder = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).ToList<string>();
    List<string> contentFiles = filesInFolder.Where(filePath => !ignoredFilePaths.Any(ignoredFilePath => filePath.Contains(ignoredFilePath))).ToList();

    var changes = new List<ADOChange>();
    foreach (string contentFile in contentFiles) {
      var fullFilePath = Path.GetFullPath(contentFile);
      var fileName = Path.GetFileName(contentFile);
      var filePathPart = GetPartPath(folderPath, contentFile);
      changes.Add(new ADOChange {
        changeType = "add",
        item = new ADOItem {
          path = "/" + filePathPart
        },
        newContent = new ADONewContent {
          content = Convert.ToBase64String(File.ReadAllBytes(contentFile)),
          contentType = "base64encoded"
        }
      });
    }

    return new ADOPushRequest {
      refUpdates = new List<ADORef> {
          new ADORef {
            name = "refs/heads/main",
            oldObjectId = LastObjectId
          }
        },
      commits = new List<ADOCommit> {
          new ADOCommit {
            comment = "Second commit with a bunch of files",
            changes = changes
          }
        }
    };

  }

  public static ADOPushRequest GetPushRequestWithDirectLakeModeSourceFiles(string LastObjectId) {

    string folderPath = AppSettings.LocalGitIntegrationSource + "DirectLakeMode";
    List<string> ignoredFilePaths = new List<string> {
        "localSettings.json", "\\.pbi\\"
      };

    List<string> filesInFolder = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories).ToList<string>();
    List<string> contentFiles = filesInFolder.Where(filePath => !ignoredFilePaths.Any(ignoredFilePath => filePath.Contains(ignoredFilePath))).ToList();

    var changes = new List<ADOChange>();
    foreach (string contentFile in contentFiles) {
      var fullFilePath = Path.GetFullPath(contentFile);
      var fileName = Path.GetFileName(contentFile);
      var filePathPart = GetPartPath(folderPath, contentFile);
      changes.Add(new ADOChange {
        changeType = "add",
        item = new ADOItem {
          path = "/" + filePathPart
        },
        newContent = new ADONewContent {
          content = Convert.ToBase64String(File.ReadAllBytes(contentFile)),
          contentType = "base64encoded"
        }
      });
    }

    return new ADOPushRequest {
      refUpdates = new List<ADORef> {
          new ADORef {
            name = "refs/heads/main",
            oldObjectId = LastObjectId
          }
        },
      commits = new List<ADOCommit> {
          new ADOCommit {
            comment = "Second commit with a bunch of files",
            changes = changes
          }
        }
    };

  }

  // delete action methods - be careful with these
  public static void DeleteProject(string ProjectId) {
    string jsonResponse = ExecuteDeleteRequest("/_apis/projects/" + ProjectId);
    var deleteOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
    while (deleteOperation.status != "succeeded") {
      Thread.Sleep(2000);
      jsonResponse = ExecuteGetRequest("/_apis/operations/" + deleteOperation.id);
      deleteOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
    }
  }

  public static void DeleteAllProjects_Danger_ThisFunctionCanHurtYou() {

    var projects = GetProjects();

    foreach (var project in projects) {

      Console.WriteLine("deleting project " + project.name);
      string jsonResponse = ExecuteDeleteRequest("/_apis/projects/" + project.id);

      var deleteOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);

      while (deleteOperation.status != "succeeded") {
        Thread.Sleep(2000);
        jsonResponse = ExecuteGetRequest("/_apis/operations/" + deleteOperation.id);
        deleteOperation = JsonSerializer.Deserialize<ADOProjectOperation>(jsonResponse);
      }

      Console.WriteLine("Project successfully deleted");

    }
  }

}