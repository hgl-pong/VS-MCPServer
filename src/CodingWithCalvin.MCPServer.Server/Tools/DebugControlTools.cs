using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class DebugControlTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public DebugControlTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "debugger_state", ReadOnly = true)]
    [Description("Get the current debugger state including mode (Design/Break/Run), process info, and thread info.")]
    public async Task<string> GetDebugStateAsync()
    {
        var state = await _rpcClient.GetDebugStateAsync();
        return JsonSerializer.Serialize(state, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_start")]
    [Description("Start debugging the current solution's startup project (F5).")]
    public async Task<string> StartDebuggingAsync()
    {
        var result = await _rpcClient.StartDebuggingAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_stop")]
    [Description("Stop the current debugging session (Shift+F5).")]
    public async Task<string> StopDebuggingAsync()
    {
        var result = await _rpcClient.StopDebuggingAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_continue")]
    [Description("Continue execution from a breakpoint (F5). Only works when debugger is in break mode.")]
    public async Task<string> ContinueDebuggingAsync()
    {
        var result = await _rpcClient.ContinueDebuggingAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_step_into")]
    [Description("Step into the next statement, entering function calls (F11). Only works when debugger is in break mode.")]
    public async Task<string> StepIntoAsync()
    {
        var result = await _rpcClient.StepIntoAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_step_over")]
    [Description("Step over the next statement without entering function calls (F10). Only works when debugger is in break mode.")]
    public async Task<string> StepOverAsync()
    {
        var result = await _rpcClient.StepOverAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_step_out")]
    [Description("Step out of the current function (Shift+F11). Only works when debugger is in break mode.")]
    public async Task<string> StepOutAsync()
    {
        var result = await _rpcClient.StepOutAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_run_to_cursor")]
    [Description("Run until reaching the specified file and line (Ctrl+F10). Only works when debugger is in break mode.")]
    public async Task<string> RunToCursorAsync(
        [Description("The absolute path to the file")] string filePath,
        [Description("The line number to run to")] int line
    )
    {
        var result = await _rpcClient.RunToCursorAsync(filePath, line);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }
}
