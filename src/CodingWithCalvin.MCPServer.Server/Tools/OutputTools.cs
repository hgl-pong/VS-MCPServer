using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class OutputTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public OutputTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
        _jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    [McpServerTool(Name = "output_get_build", ReadOnly = true)]
    [Description("Get the contents of the Visual Studio Build Output window.")]
    public async Task<string> GetBuildOutputAsync()
    {
        var output = await _rpcClient.GetBuildOutputAsync();
        return output;
    }

    [McpServerTool(Name = "output_get_debug", ReadOnly = true)]
    [Description("Get the contents of the Visual Studio Debug Output window.")]
    public async Task<string> GetDebugOutputAsync()
    {
        var output = await _rpcClient.GetDebugOutputAsync();
        return output;
    }

    [McpServerTool(Name = "output_write", ReadOnly = false)]
    [Description("Write a message to a Visual Studio Output window pane. Creates the pane if it doesn't exist.")]
    public async Task<string> WriteToOutputWindowAsync(
        [Description("The name of the output pane (e.g., 'Build', 'Debug', or custom name)")] string paneName,
        [Description("The message to write")] string message)
    {
        var success = await _rpcClient.WriteToOutputWindowAsync(paneName, message);
        return JsonSerializer.Serialize(new { success, paneName }, _jsonOptions);
    }
}
