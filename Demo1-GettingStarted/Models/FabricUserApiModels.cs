// Classes used to serialize to JSON and to deserialize from JSON

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