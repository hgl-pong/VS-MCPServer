using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class NuGetTools
{
    private readonly RpcClient _rpcClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public NuGetTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "nuget_list", ReadOnly = true)]
    [Description("List all NuGet packages installed in a project.")]
    public async Task<string> ListPackagesAsync(
        [Description("The path to the project file (.csproj)")] string projectPath)
    {
        var packages = await _rpcClient.GetProjectPackagesAsync(projectPath);
        return JsonSerializer.Serialize(packages, _jsonOptions);
    }

    [McpServerTool(Name = "nuget_search", ReadOnly = true)]
    [Description("Search for NuGet packages in the NuGet gallery.")]
    public async Task<string> SearchPackagesAsync(
        [Description("The search term")] string searchTerm,
        [Description("Number of results to skip for pagination (default: 0)")] int skip = 0,
        [Description("Number of results to return (default: 20)")] int take = 20)
    {
        var result = await _rpcClient.SearchNuGetPackagesAsync(searchTerm, skip, take);
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "nuget_install", ReadOnly = false)]
    [Description("Install a NuGet package to a project.")]
    public async Task<string> InstallPackageAsync(
        [Description("The path to the project file (.csproj)")] string projectPath,
        [Description("The package ID to install")] string packageId,
        [Description("Optional specific version to install (e.g., '1.2.3'). If not specified, installs the latest stable version.")] string? version = null)
    {
        var success = await _rpcClient.InstallNuGetPackageAsync(projectPath, packageId, version);
        return JsonSerializer.Serialize(new { success, packageId, version, projectPath }, _jsonOptions);
    }

    [McpServerTool(Name = "nuget_update", ReadOnly = false)]
    [Description("Update a NuGet package in a project to a newer version.")]
    public async Task<string> UpdatePackageAsync(
        [Description("The path to the project file (.csproj)")] string projectPath,
        [Description("The package ID to update")] string packageId,
        [Description("Optional specific version to update to. If not specified, updates to the latest stable version.")] string? version = null)
    {
        var success = await _rpcClient.UpdateNuGetPackageAsync(projectPath, packageId, version);
        return JsonSerializer.Serialize(new { success, packageId, version, projectPath }, _jsonOptions);
    }

    [McpServerTool(Name = "nuget_uninstall", ReadOnly = false)]
    [Description("Uninstall a NuGet package from a project.")]
    public async Task<string> UninstallPackageAsync(
        [Description("The path to the project file (.csproj)")] string projectPath,
        [Description("The package ID to uninstall")] string packageId)
    {
        var success = await _rpcClient.UninstallNuGetPackageAsync(projectPath, packageId);
        return JsonSerializer.Serialize(new { success, packageId, projectPath }, _jsonOptions);
    }
}
