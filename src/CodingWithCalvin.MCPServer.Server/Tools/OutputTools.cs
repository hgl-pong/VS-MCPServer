using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class OutputTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public OutputTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "output_get_build", ReadOnly = true)]
    [Description("Get the contents of the Build output window.")]
    public async Task<string> GetBuildOutputAsync()
    {
        var output = await _rpcClient.GetBuildOutputAsync();
        return JsonSerializer.Serialize(new { output }, _jsonOptions);
    }

    [McpServerTool(Name = "output_get_debug", ReadOnly = true)]
    [Description("Get the contents of the Debug output window (including Debug.WriteLine output).")]
    public async Task<string> GetDebugOutputAsync()
    {
        var output = await _rpcClient.GetDebugOutputAsync();
        return JsonSerializer.Serialize(new { output }, _jsonOptions);
    }
}
