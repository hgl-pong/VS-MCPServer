using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class SolutionExplorerTools
{
    private readonly RpcClient _rpcClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public SolutionExplorerTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "solution_add_project", ReadOnly = false)]
    [Description("Add an existing project to the current solution.")]
    public async Task<string> AddProjectToSolutionAsync(
        [Description("The full path to the project file (.csproj) to add")] string projectPath)
    {
        var success = await _rpcClient.AddProjectToSolutionAsync(projectPath);
        return JsonSerializer.Serialize(new { success, projectPath }, _jsonOptions);
    }

    [McpServerTool(Name = "solution_remove_project", ReadOnly = false)]
    [Description("Remove a project from the current solution.")]
    public async Task<string> RemoveProjectFromSolutionAsync(
        [Description("The full path to the project file (.csproj) to remove")] string projectPath)
    {
        var success = await _rpcClient.RemoveProjectFromSolutionAsync(projectPath);
        return JsonSerializer.Serialize(new { success, projectPath }, _jsonOptions);
    }

    [McpServerTool(Name = "project_remove_file", ReadOnly = false)]
    [Description("Remove a file from a project.")]
    public async Task<string> RemoveFileFromProjectAsync(
        [Description("The full path to the project file (.csproj)")] string projectPath,
        [Description("The full path to the file to remove")] string filePath)
    {
        var success = await _rpcClient.RemoveFileFromProjectAsync(projectPath, filePath);
        return JsonSerializer.Serialize(new { success, projectPath, filePath }, _jsonOptions);
    }

    [McpServerTool(Name = "project_add_reference", ReadOnly = false)]
    [Description("Add a project-to-project reference.")]
    public async Task<string> AddProjectReferenceAsync(
        [Description("The full path to the project file (.csproj) that will have the reference")] string projectPath,
        [Description("The full path to the project file (.csproj) to reference")] string referenceProjectPath)
    {
        var success = await _rpcClient.AddProjectReferenceAsync(projectPath, referenceProjectPath);
        return JsonSerializer.Serialize(new { success, projectPath, referenceProjectPath }, _jsonOptions);
    }

    [McpServerTool(Name = "project_remove_reference", ReadOnly = false)]
    [Description("Remove a project-to-project reference.")]
    public async Task<string> RemoveProjectReferenceAsync(
        [Description("The full path to the project file (.csproj) that has the reference")] string projectPath,
        [Description("The full path to the referenced project file (.csproj)")] string referenceProjectPath)
    {
        var success = await _rpcClient.RemoveProjectReferenceAsync(projectPath, referenceProjectPath);
        return JsonSerializer.Serialize(new { success, projectPath, referenceProjectPath }, _jsonOptions);
    }
}
