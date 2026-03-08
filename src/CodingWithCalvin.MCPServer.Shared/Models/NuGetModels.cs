using System.Collections.Generic;

namespace CodingWithCalvin.MCPServer.Shared.Models;

public class NuGetPackage
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Authors { get; set; }
    public int? DownloadCount { get; set; }
}

public class NuGetSearchResult
{
    public List<NuGetPackage> Packages { get; set; } = [];
    public int TotalCount { get; set; }
}

public class ProjectPackage
{
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? TargetFramework { get; set; }
}
