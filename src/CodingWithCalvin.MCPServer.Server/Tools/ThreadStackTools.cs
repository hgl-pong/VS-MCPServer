using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class ThreadStackTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public ThreadStackTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "debugger_call_stack", ReadOnly = true)]
    [Description("Get the current call stack with frame information including method names and file locations.")]
    public async Task<string> GetCallStackAsync()
    {
        var frames = await _rpcClient.GetCallStackAsync();
        return JsonSerializer.Serialize(frames, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_set_frame")]
    [Description("Change the active stack frame for variable inspection. Use this to inspect variables in different frames.")]
    public async Task<string> SetActiveStackFrameAsync(
        [Description("The frame index (0 = current frame, 1 = caller, etc.)")] int frameIndex
    )
    {
        var result = await _rpcClient.SetActiveStackFrameAsync(frameIndex);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_threads", ReadOnly = true)]
    [Description("List all threads in the current debugged process with their IDs, names, and current locations.")]
    public async Task<string> GetThreadsAsync()
    {
        var threads = await _rpcClient.GetThreadsAsync();
        return JsonSerializer.Serialize(threads, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_set_thread")]
    [Description("Switch the active thread for inspection.")]
    public async Task<string> SetActiveThreadAsync(
        [Description("The thread ID to switch to")] int threadId
    )
    {
        var result = await _rpcClient.SetActiveThreadAsync(threadId);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }
}
