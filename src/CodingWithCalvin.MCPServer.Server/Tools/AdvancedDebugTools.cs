using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class AdvancedDebugTools
{
    private readonly RpcClient _rpcClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public AdvancedDebugTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "debugger_attach", ReadOnly = false)]
    [Description("Attach the debugger to a running process.")]
    public async Task<string> AttachToProcessAsync(
        [Description("The process ID to attach to")] int processId)
    {
        var success = await _rpcClient.AttachToProcessAsync(processId);
        return JsonSerializer.Serialize(new { success, processId }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_processes", ReadOnly = true)]
    [Description("Get a list of all local processes that can be attached to.")]
    public async Task<string> GetProcessesAsync()
    {
        var processes = await _rpcClient.GetProcessesAsync();
        return JsonSerializer.Serialize(processes, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_modules", ReadOnly = true)]
    [Description("Get a list of loaded modules in the current debug process.")]
    public async Task<string> GetModulesAsync()
    {
        var modules = await _rpcClient.GetModulesAsync();
        return JsonSerializer.Serialize(modules, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_read_memory", ReadOnly = true)]
    [Description("Read memory at a specific address during debugging.")]
    public async Task<string> ReadMemoryAsync(
        [Description("The memory address to read from")] ulong address,
        [Description("The number of bytes to read")] int size)
    {
        var result = await _rpcClient.ReadMemoryAsync(address, size);
        return JsonSerializer.Serialize(result, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_registers", ReadOnly = true)]
    [Description("Get the current register values during debugging.")]
    public async Task<string> GetRegistersAsync()
    {
        var registers = await _rpcClient.GetRegistersAsync();
        return JsonSerializer.Serialize(registers, _jsonOptions);
    }
}
