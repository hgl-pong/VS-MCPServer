using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class BreakpointTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public BreakpointTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "breakpoint_list", ReadOnly = true)]
    [Description("List all breakpoints in the solution with their locations, conditions, and enabled status.")]
    public async Task<string> GetBreakpointsAsync()
    {
        var breakpoints = await _rpcClient.GetBreakpointsAsync();
        return JsonSerializer.Serialize(breakpoints, _jsonOptions);
    }

    [McpServerTool(Name = "breakpoint_set")]
    [Description("Set a breakpoint at the specified file and line. Optionally set condition and hit count.")]
    public async Task<string> SetBreakpointAsync(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number for the breakpoint")] int line,
        [Description("Optional column number (default: 1)")] int column = 1,
        [Description("Optional condition expression (e.g., 'x > 10')")] string? condition = null,
        [Description("Optional hit count target")] int hitCount = 0,
        [Description("Hit count type: 'always', 'equal', 'greater', or 'multiple'")] string hitCountType = "always"
    )
    {
        var request = new SetBreakpointRequest
        {
            FilePath = filePath,
            Line = line,
            Column = column,
            Condition = condition,
            HitCount = hitCount,
            HitCountType = hitCountType
        };

        var result = await _rpcClient.SetBreakpointAsync(request);
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "breakpoint_remove")]
    [Description("Remove a breakpoint at the specified file and line.")]
    public async Task<string> RemoveBreakpointAsync(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number of the breakpoint")] int line
    )
    {
        var result = await _rpcClient.RemoveBreakpointAsync(filePath, line);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "breakpoint_toggle")]
    [Description("Enable or disable a breakpoint at the specified file and line without removing it.")]
    public async Task<string> ToggleBreakpointAsync(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number of the breakpoint")] int line
    )
    {
        var result = await _rpcClient.ToggleBreakpointAsync(filePath, line);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "breakpoint_set_condition")]
    [Description("Set or modify the condition and hit count of an existing breakpoint.")]
    public async Task<string> SetBreakpointConditionAsync(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number of the breakpoint")] int line,
        [Description("Optional condition expression (empty string to clear)")] string? condition = null,
        [Description("Optional hit count target")] int hitCount = 0,
        [Description("Hit count type: 'always', 'equal', 'greater', or 'multiple'")] string hitCountType = "always"
    )
    {
        var result = await _rpcClient.SetBreakpointConditionAsync(filePath, line, condition, hitCount, hitCountType);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }
}
