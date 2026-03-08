using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class GeneralTools
{
    private readonly RpcClient _rpcClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public GeneralTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "execute_command", ReadOnly = false)]
    [Description("Execute a Visual Studio command by name. Examples: 'File.SaveAll', 'Edit.Undo', 'View.SolutionExplorer', 'Debug.Start'. Use this to trigger any VS command that can be accessed via keyboard shortcuts or menus.")]
    public async Task<string> ExecuteCommandAsync(
        [Description("The command name to execute (e.g., 'File.SaveAll', 'Debug.Start')")] string commandName,
        [Description("Optional command arguments")] string? args = null)
    {
        var result = await _rpcClient.ExecuteCommandAsync(commandName, args);
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "get_ide_status", ReadOnly = true)]
    [Description("Get the current status of the Visual Studio IDE, including solution state, debugging state, and active document.")]
    public async Task<string> GetIdeStatusAsync()
    {
        var status = await _rpcClient.GetIdeStatusAsync();
        return JsonSerializer.Serialize(status, _jsonOptions);
    }
}
