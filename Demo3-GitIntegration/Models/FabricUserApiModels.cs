﻿// Classes used to serialize to JSON and to deserialize from JSON

public class FabricWorkspaceList {
  public List<FabricWorkspace> value { get; set; }
}

public class FabricWorkspace {
  public string id { get; set; }
  public string displayName { get; set; }
  public string description { get; set; }
  public string type { get; set; }
}

public class FabricWorkspaceCreateRequest {
  public string displayName { get; set; }
  public string capacityId { get; set; }
  public string description { get; set; }
}

public class FabricWorkspaceUpdateRequest {
  public string displayName { get; set; }
  public string description { get; set; }
}

public class FabricCapacity {
  public string id { get; set; }
  public string displayName { get; set; }
  public string sku { get; set; }
  public string region { get; set; }
  public string state { get; set; }
}

public class FabricCapacityList {
  public List<FabricCapacity> value { get; set; }
}

public class FabricWorkspaceRoleAssignmentList {
  public List<FabricWorkspaceRoleAssignment> value { get; set; }
}

public class FabricWorkspaceRoleAssignment {
  public Principal principal { get; set; }
  public string role { get; set; }
}

public class WorkspaceRole {
  public const string Admin = "Admin";
  public const string Contributor = "Contributor";
  public const string Member = "Member";
  public const string Viewer = "Viewer";
}

public class Principal {
  public string id { get; set; }
  public string displayName { get; set; }
  public string type { get; set; }
  public UserDetails userDetails { get; set; }
  public GroupDetails groupDetails { get; set; }
  public ServicePrincipalDetails servicePrincipalDetails { get; set; }
  public ServicePrincipalProfileDetails servicePrincipalProfileDetails { get; set; }
}

public class PrincipalType {
  public const string User = "User";
  public const string Group = "Group";
  public const string ServicePrincipal = "ServicePrincipal";
  public const string ServicePrincipalProfile = "ServicePrincipalProfile";
}

public class UserDetails {
  public string userPrincipalName { get; set; }
}

public class GroupDetails {
  public string groupType { get; set; }
}

public class ServicePrincipalDetails {
  public string aadAppId { get; set; }
}

public class ServicePrincipalProfileDetails {
  public ParentPrincipal parentPrincipal { get; set; }
}

public class ParentPrincipal {
  public string id { get; set; }
  public string type { get; set; }
}

public class FabricOperation {
  public string status { get; set; }
  public DateTime createdTimeUtc { get; set; }
  public DateTime lastUpdatedTimeUtc { get; set; }
  public object percentComplete { get; set; }
  public FabricErrorResponse error { get; set; }
}

public class FabricErrorResponse {
  public string errorCode { get; set; }
  public string message { get; set; }
  public string requestId { get; set; }
  public object moreDetails { get; set; }
  public object relatedResource { get; set; }
}


public class FabricItemList {
  public List<FabricItem> value { get; set; }
}

public class FabricItem {
  public string id { get; set; }
  public string type { get; set; }
  public string displayName { get; set; }
  public string description { get; set; }
  public string workspaceId { get; set; }
}

public class FabricItemUpdateRequest {
  public string displayName { get; set; }
  public string description { get; set; }
}

public class FabricItemType {
  public const string SemanticModel = "SemanticModel";
  public const string Report = "Report";
  public const string PaginatedReport = "PaginatedReport";
  public const string Dashboard = "Dashboard";
  public const string Datamart = "Datamart";
  public const string Lakehouse = "Lakehouse";
  public const string SQLEndpoint = "SQLEndpoint";
  public const string Notebook = "Notebook";
  public const string SparkJobDefinition = "SparkJobDefinition";
  public const string MLModel = "MLModel";
  public const string MLExperiment = "MLExperiment";
  public const string Warehouse = "Warehouse";
  public const string MirroredWarehouse = "MirroredWarehouse";
  public const string DataPipeline = "DataPipeline";
  public const string KQLDatabase = "KQLDatabase";
  public const string KQLDataConnection = "KQLDataConnection";
  public const string KQLQueryset = "KQLQueryset";
  public const string Eventstream = "Eventstream";

  // create collection of all possible types for testing
  public static readonly List<string> AllTypes = new List<string> {
      SemanticModel, Report, PaginatedReport, Dashboard, Datamart,
      Lakehouse, SQLEndpoint, Notebook, SparkJobDefinition, MLModel, MLExperiment,
      Warehouse, MirroredWarehouse, DataPipeline,
      KQLDatabase, KQLDataConnection, KQLQueryset, Eventstream
    };

}

public class FabricItemCreateRequest {
  public string type { get; set; }
  public string displayName { get; set; }
  public string description { get; set; }
  public FabricItemDefinition definition { get; set; }
}

public class FabricItemUpdateDefinitionRequest {
  public FabricItemDefinition definition { get; set; }
}

public class FabricItemDefinition {
  public string format { get; set; }
  public List<FabricItemDefinitionPart> parts { get; set; }
}

public class FabricItemDefinitionFormat {
  public const string ipynb = "ipynb";
  public const string SparkJobDefinitionV1 = "SparkJobDefinitionV1";
}

public class FabricItemDefinitionPart {
  public string path { get; set; }
  public string payload { get; set; }
  public string payloadType { get; set; }
}

public class FabricItemDefinitionResponse {
  public FabricItemDefinition definition { get; set; }
}

public class GitProviderConnection {
  public GitProviderDetails gitProviderDetails { get; set; }
  public GitSyncDetails gitSyncDetails { get; set; }
  public string gitConnectionState { get; set; }
}

public class GitProviderDetails {
  public string organizationName { get; set; }
  public string projectName { get; set; }
  public string gitProviderType { get; set; }
  public string repositoryName { get; set; }
  public string branchName { get; set; }
  public string directoryName { get; set; }
}

public class GitSyncDetails {
  public string head { get; set; }
  public DateTime? lastSyncTime { get; set; }
}

public class GitConnectionInitRequest {
  public string initializationStrategy { get; set; }
}

public class GitInitializationStrategy {
  public const string None = "None";
  public const string PreferRemote = "PreferRemote";
  public const string PreferWorkspace = "PreferWorkspace";
}

public class GitInitializationResonse {
  public string requiredAction { get; set; }
  public string workspaceHead { get; set; }
  public string remoteCommitHash { get; set; }
}

public class GitRequiredAction {
  public const string None = "None";
  public const string CommitToGit = "CommitToGit";
  public const string UpdateFromGit = "UpdateFromGit";
}

public class CommitToGitRequest {
  public string mode { get; set; }
  public string workspaceHead { get; set; }
  public string comment { get; set; }
}

public class GitGetStatusResponse {
  public string workspaceHead { get; set; }
  public string remoteCommitHash { get; set; }
  public List<GitChange> changes { get; set; }
}

public class GitChange {
  public ItemMetadata itemMetadata { get; set; }
  public string workspaceChange { get; set; }
  public string conflictType { get; set; }
  public string remoteChange { get; set; }
}

public class ItemMetadata {
  public ItemIdentifier itemIdentifier { get; set; }
  public string itemType { get; set; }
  public string displayName { get; set; }
}

public class ItemIdentifier {
  public string objectId { get; set; }
  public string logicalId { get; set; }
}

public class UpdateFromGitRequest {
  public string workspaceHead { get; set; }
  public string remoteCommitHash { get; set; }
  public ConflictResolution conflictResolution { get; set; }
  public ConflictResolutionOptions options { get; set; }
}

public class ConflictResolution {
  public string conflictResolutionType { get; set; }
  public string conflictResolutionPolicy { get; set; }
}

public class ConflictResolutionOptions {
  public bool allowOverrideItems { get; set; }
}
