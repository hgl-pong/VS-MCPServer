using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class InspectionTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public InspectionTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "debugger_evaluate", ReadOnly = true)]
    [Description("Evaluate an expression in the current debug context. The debugger must be in break mode (paused at a breakpoint).")]
    public async Task<string> EvaluateExpressionAsync(
        [Description("The expression to evaluate (e.g., 'myVariable', 'x + y', 'obj.Property').")] string expression)
    {
        var result = await _rpcClient.EvaluateExpressionAsync(expression);
        
        if (!result.IsValid || !string.IsNullOrEmpty(result.ErrorMessage))
        {
            return JsonSerializer.Serialize(new 
            { 
                success = false, 
                error = result.ErrorMessage ?? "Failed to evaluate expression. Ensure debugger is in break mode." 
            }, _jsonOptions);
        }
        
        return JsonSerializer.Serialize(new 
        { 
            success = true, 
            expression = result.Expression,
            value = result.Value, 
            type = result.Type 
        }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_locals", ReadOnly = true)]
    [Description("Get all local variables in the current stack frame. Only works when debugger is in break mode.")]
    public async Task<string> GetLocalsAsync()
    {
        var locals = await _rpcClient.GetLocalsAsync();
        return JsonSerializer.Serialize(locals, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_arguments", ReadOnly = true)]
    [Description("Get all arguments of the current method in the active stack frame. Only works when debugger is in break mode.")]
    public async Task<string> GetArgumentsAsync()
    {
        var arguments = await _rpcClient.GetArgumentsAsync();
        return JsonSerializer.Serialize(arguments, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_inspect_variable", ReadOnly = true)]
    [Description("Deep inspect a variable including its members (properties, fields). Returns nested structure up to specified depth.")]
    public async Task<string> InspectVariableAsync(
        [Description("The variable name to inspect")] string variableName,
        [Description("Depth of member inspection (default: 1)")] int depth = 1
    )
    {
        var result = await _rpcClient.InspectVariableAsync(variableName, depth);
        
        if (!result.IsValid)
        {
            return JsonSerializer.Serialize(new 
            { 
                success = false, 
                error = $"Failed to inspect variable '{variableName}'. Ensure debugger is in break mode and the variable exists in the current context." 
            }, _jsonOptions);
        }
        
        return JsonSerializer.Serialize(new 
        { 
            success = true,
            variable = result
        }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_set_variable")]
    [Description("Modify a variable's value during debugging. Only works when debugger is in break mode.")]
    public async Task<string> SetVariableValueAsync(
        [Description("The variable name to modify")] string variableName,
        [Description("The new value (as a string that will be parsed)")] string value
    )
    {
        var result = await _rpcClient.SetVariableValueAsync(variableName, value);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_get_watch", ReadOnly = true)]
    [Description("Get all watch expressions and their current values.")]
    public async Task<string> GetWatchExpressionsAsync()
    {
        var watches = await _rpcClient.GetWatchExpressionsAsync();
        return JsonSerializer.Serialize(watches, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_add_watch")]
    [Description("Add an expression to the watch list.")]
    public async Task<string> AddWatchExpressionAsync(
        [Description("The expression to watch")] string expression
    )
    {
        var result = await _rpcClient.AddWatchExpressionAsync(expression);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_remove_watch")]
    [Description("Remove an expression from the watch list.")]
    public async Task<string> RemoveWatchExpressionAsync(
        [Description("The expression to remove")] string expression
    )
    {
        var result = await _rpcClient.RemoveWatchExpressionAsync(expression);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "debugger_clear_watch")]
    [Description("Remove all expressions from the watch list.")]
    public async Task<string> ClearWatchExpressionsAsync()
    {
        var result = await _rpcClient.ClearWatchExpressionsAsync();
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }
}
