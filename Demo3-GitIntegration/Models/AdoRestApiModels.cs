
public class ADOProjectList {
  public int count { get; set; }
  public List<ADOProject> value { get; set; }
}

public class ADOProject {
  public string id { get; set; }
  public string name { get; set; }
  public string url { get; set; }
  public string state { get; set; }
  public int revision { get; set; }
  public string visibility { get; set; }
  public DateTime lastUpdateTime { get; set; }
}

public class ADOProjectCreateRequest {
  public string name { get; set; }
  public string description { get; set; }
  public int visibility { get; set; }
  public ADOCapabilities capabilities { get; set; }
}

public class ADORepositoryCreateRequest {
  public string name { get; set; }
  public ADOProject project { get; set; }
}

public class ADOCapabilities {
  public ADOVersioncontrol versioncontrol { get; set; }
  public ADOProcessTemplate processTemplate { get; set; }
}

public class ADOVersioncontrol {
  public string sourceControlType { get; set; }
}

public class ADOProcessTemplate {
  public string templateTypeId { get; set; }
}

public class ADOProjectOperation {
  public string id { get; set; }
  public string status { get; set; }
  public string url { get; set; }
}

public class ADORepositoryList {
  public List<ADORepository> value { get; set; }
  public int count { get; set; }
}

public class ADORepository {
  public string id { get; set; }
  public string name { get; set; }
  public string url { get; set; }
  public ADOProject project { get; set; }
  public int size { get; set; }
  public string remoteUrl { get; set; }
  public string sshUrl { get; set; }
  public string webUrl { get; set; }
  public bool isDisabled { get; set; }
  public bool isInMaintenance { get; set; }
}

public class ADOPushRequest {
  public List<ADOCommit> commits { get; set; }
  public List<ADORef> refUpdates { get; set; }
  public ADORepository repository { get; set; }
}

public class ADOCommit {
  public string comment { get; set; }
  public List<ADOChange> changes { get; set; }
}

public class ADOChange {
  public string changeType { get; set; }
  public ADOItem item { get; set; }
  public ADONewContent newContent { get; set; }
  public ADONewContentTemplate newContentTemplate { get; set; }
}

public class ADOItem {
  public string path { get; set; }
}

public class ADONewContent {
  public string content { get; set; }
  public string contentType { get; set; }
}

public class ADONewContentTemplate {
  public string name { get; set; }
  public string type { get; set; }
}

public class ADOPushResponse {
  public List<ADOCommit> commits { get; set; }
  public List<ADORef> refUpdates { get; set; }
  public ADORepository repository { get; set; }
  public ADOPushedBy pushedBy { get; set; }
  public int pushId { get; set; }
  public DateTime date { get; set; }
  public string url { get; set; }
  public Links _links { get; set; }
}

public class ADOPushedBy {
  public string displayName { get; set; }
  public string url { get; set; }
  public Links _links { get; set; }
  public string id { get; set; }
  public string uniqueName { get; set; }
  public string imageUrl { get; set; }
  public string descriptor { get; set; }
}

public class ADOPusher {
  public string href { get; set; }
}

public class ADORefsList {
  public List<ADORef> value { get; set; }
  public int count { get; set; }
}

public class ADORef {
  public string name { get; set; }
  public string objectId { get; set; }
  public string oldObjectId { get; set; }
  public string newObjectId { get; set; }
  public string url { get; set; }
  public Creator creator { get; set; }
}

public class Avatar {
  public string href { get; set; }
}

public class Creator {
  public string displayName { get; set; }
  public string url { get; set; }
  public Links _links { get; set; }
  public string id { get; set; }
  public string uniqueName { get; set; }
  public string imageUrl { get; set; }
  public string descriptor { get; set; }
}

public class Links {
  public Avatar avatar { get; set; }
}