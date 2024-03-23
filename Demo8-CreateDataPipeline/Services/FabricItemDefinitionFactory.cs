using System.Text;
using System.Text.Json;

public class FabricItemDefinitionFactory {

  public static FabricItemCreateRequest GetNotebookCreateRequest(string WorkspaceId, FabricItem Lakehouse, string DisplayName, string CodeContent) {

    CodeContent = CodeContent.Replace("{WORKSPACE_ID}", WorkspaceId)
                             .Replace("{LAKEHOUSE_ID}", Lakehouse.id)
                             .Replace("{LAKEHOUSE_NAME}", Lakehouse.displayName);

    FabricItemCreateRequest itemCreateRequest = new FabricItemCreateRequest {
      displayName = DisplayName,
      type = "Notebook",
      definition = new FabricItemDefinition {
        format = "ipynb",
        parts = new List<FabricItemDefinitionPart>() {
            new FabricItemDefinitionPart {
              path = "notebook-content.ipynb",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(CodeContent))
            }
          },
      }
    };

    return itemCreateRequest;
  }

  public static FabricItemCreateRequest GetDirectLakeSalesModelCreateRequest(string DisplayName, string SqlEndpointServer, string SqlEndpointDatabase) {

    string part1FileContent = Demo8_CreateDataPipeline.Properties.Resources.definition_pbidataset;

    string part2FileContent = Demo8_CreateDataPipeline.Properties.Resources.sales_model_DirectLake_bim
                                          .Replace("{SQL_ENDPOINT_SERVER}", SqlEndpointServer)
                                          .Replace("{SQL_ENDPOINT_DATABASE}", SqlEndpointDatabase);

    FabricItemCreateRequest itemCreateRequest = new FabricItemCreateRequest {
      displayName = DisplayName,
      type = "SemanticModel",
      definition = new FabricItemDefinition {
        parts = new List<FabricItemDefinitionPart>() {

            new FabricItemDefinitionPart {
              path = "definition.pbidataset",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(part1FileContent))
            },

            new FabricItemDefinitionPart {
              path = "model.bim",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(part2FileContent))
            }

          }
      }
    };

    return itemCreateRequest;
  }

  public static FabricItemCreateRequest GetSalesReportCreateRequest(string SemanticModelId, string DisplayName) {

    string part1FileContent = Demo8_CreateDataPipeline.Properties.Resources.definition_pbir.Replace("{SEMANTIC_MODEL_ID}", SemanticModelId);
    string part2FileContent = Demo8_CreateDataPipeline.Properties.Resources.sales_report_json;
    string part3FileContent = Demo8_CreateDataPipeline.Properties.Resources.CY24SU02_json;

    FabricItemCreateRequest itemCreateRequest = new FabricItemCreateRequest {
      displayName = DisplayName,
      type = "Report",
      definition = new FabricItemDefinition {
        parts = new List<FabricItemDefinitionPart>() {

            new FabricItemDefinitionPart {
              path = "definition.pbir",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(part1FileContent))
            },

            new FabricItemDefinitionPart {
              path = "report.json",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(part2FileContent))
            },

            new FabricItemDefinitionPart {
              path = "StaticResources/SharedResources/BaseThemes/CY24SU02.json",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(part3FileContent))
            }
          }
      }
    };

    return itemCreateRequest;
  }

  public static void DeleteAllTemplateFiles(string WorkspaceName) {
    string targetFolder = AppSettings.LocalTemplatesFolder + (string.IsNullOrEmpty(WorkspaceName) ? "" : WorkspaceName + @"\");
    if (Directory.Exists(targetFolder)) {
      DirectoryInfo di = new DirectoryInfo(targetFolder);
      foreach (FileInfo file in di.GetFiles()) { file.Delete(); }
      foreach (DirectoryInfo dir in di.GetDirectories()) { dir.Delete(true); }
    }
  }

  public static void WriteFile(string WorkspaceFolder, string ItemFolder, string FilePath, string FileContent, bool ConvertFromBase64 = true) {

    if (ConvertFromBase64) {
      byte[] bytes = Convert.FromBase64String(FileContent);
      FileContent = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }

    FilePath = FilePath.Replace("/", @"\");
    string folderPath = AppSettings.LocalTemplatesFolder + WorkspaceFolder + @"\" + ItemFolder;

    Directory.CreateDirectory(folderPath);

    string fullPath = folderPath + @"\" + FilePath;

    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

    File.WriteAllText(fullPath, FileContent);

  }

  private static string GetPartPath(string ItemFolderPath, string FilePath) {
    int ItemFolderPathOffset = ItemFolderPath.Length + 1;
    return FilePath.Substring(ItemFolderPathOffset).Replace("\\", "/");
  }

  public static List<FabricItemCreateRequest> GetItemCreateRequests(string WorkspaceFolder) {

    List<FabricItemCreateRequest> itemCreateRequests = new List<FabricItemCreateRequest>();

    string WorkspaceFolderPath = AppSettings.LocalTemplatesFolder + WorkspaceFolder;

    var metadataFiles = Directory.GetFiles(WorkspaceFolderPath, "item.metadata.json", SearchOption.AllDirectories);

    foreach (var metadataFile in metadataFiles) {
      string ItemFolder = Path.GetFileName(Path.GetDirectoryName(metadataFile));
      itemCreateRequests.Add(GetItemCreateRequest(WorkspaceFolder, ItemFolder));
    }

    return itemCreateRequests;
  }

  public static FabricItemCreateRequest GetItemCreateRequest(string WorkspaceFolder, string ItemFolder) {

    string ItemFolderPath = AppSettings.LocalTemplatesFolder + WorkspaceFolder + @"\" + ItemFolder;

    string metadataFilePath = ItemFolderPath + @"\item.metadata.json";
    string metadataFileContent = File.ReadAllText(metadataFilePath);
    FabricItem item = JsonSerializer.Deserialize<FabricItem>(metadataFileContent);

    FabricItemCreateRequest itemCreateRequest = new FabricItemCreateRequest {
      displayName = item.displayName,
      type = item.type,
      definition = new FabricItemDefinition {
        parts = new List<FabricItemDefinitionPart>()
      }
    };

    List<string> ignoredFilePaths = new List<string> {
        "\\.pbi\\",
        "item.metadata.json",
        "item.config.json",
        "lakehouse.metadata.json",
        "diagramLayout.json",
        "localSettings.json"
      };

    List<string> FilesInFolder = Directory.GetFiles(ItemFolderPath, "*", SearchOption.AllDirectories).ToList<string>();

    List<string> ItemDefinitionFiles = FilesInFolder.Where(filePath => !ignoredFilePaths.Any(ignoredFilePath => filePath.Contains(ignoredFilePath))).ToList();

    foreach (string ItemDefinitionFile in ItemDefinitionFiles) {

      string fileContentBase64 = Convert.ToBase64String(File.ReadAllBytes(ItemDefinitionFile));
      itemCreateRequest.definition.parts.Add(new FabricItemDefinitionPart {
        path = GetPartPath(ItemFolderPath, ItemDefinitionFile),
        payload = fileContentBase64,
        payloadType = "InlineBase64"
      });
    }

    return itemCreateRequest;
  }


  public static FabricItemCreateRequest GetDataPipelineCreateRequest(string WorkspaceId, string LakehouseId, string DisplayName, string CodeContent) {

    CodeContent = CodeContent.Replace("{CONNECTION_ID}", AppSettings.ConnectIdToAzureStorage)
                             .Replace("{WORKSPACE_ID}", WorkspaceId)
                             .Replace("{LAKEHOUSE_ID}", LakehouseId);

    FabricItemCreateRequest itemCreateRequest = new FabricItemCreateRequest {
      displayName = DisplayName,
      type = "DataPipeline",
      definition = new FabricItemDefinition {
        //format = "",
        parts = new List<FabricItemDefinitionPart>() {
            new FabricItemDefinitionPart {
              path = "pipeline-content.json",
              payloadType = "InlineBase64",
              payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(CodeContent))
            }
          },
      }
    };

    return itemCreateRequest;
  }



}