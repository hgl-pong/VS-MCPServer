using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class ProjectOperationTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public ProjectOperationTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "project_add_file")]
    [Description("Add an existing file to a project.")]
    public async Task<string> AddFileToProjectAsync(
        [Description("The project file path (.csproj)")] string projectPath,
        [Description("The file path to add")] string filePath
    )
    {
        var result = await _rpcClient.AddFileToProjectAsync(projectPath, filePath);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "project_create_item")]
    [Description("Create a new project item (class, interface, etc.) from a template.")]
    public async Task<string> CreateProjectItemAsync(
        [Description("The project file path (.csproj)")] string projectPath,
        [Description("The item template name (e.g., 'Class', 'Interface')")] string itemTemplate,
        [Description("The name for the new item")] string itemName,
        [Description("Optional folder path within the project")] string? folderPath = null
    )
    {
        var result = await _rpcClient.CreateProjectItemAsync(projectPath, itemTemplate, itemName, folderPath);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }
}
