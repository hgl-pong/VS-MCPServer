using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class DiagnosticTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public DiagnosticTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "diagnostics_get", ReadOnly = true)]
    [Description("Get Roslyn diagnostics (errors, warnings, suggestions) for a file or the entire solution.")]
    public async Task<string> GetDiagnosticsAsync(
        [Description("Optional file path to get diagnostics for. If omitted, returns diagnostics for the entire solution.")] string? filePath = null,
        [Description("Optional severity filter: 'error', 'warning', or 'info'")] string? severity = null
    )
    {
        var diagnostics = await _rpcClient.GetDiagnosticsAsync(filePath, severity);
        return JsonSerializer.Serialize(diagnostics, _jsonOptions);
    }

    [McpServerTool(Name = "error_list_get", ReadOnly = true)]
    [Description("Get the contents of the Visual Studio Error List window.")]
    public async Task<string> GetErrorListAsync()
    {
        var errors = await _rpcClient.GetErrorListAsync();
        return JsonSerializer.Serialize(errors, _jsonOptions);
    }

    [McpServerTool(Name = "code_fix_apply")]
    [Description("Apply a suggested code fix for a diagnostic. Use preview=true to see what would change without applying.")]
    public async Task<string> ApplyCodeFixAsync(
        [Description("The file path")] string filePath,
        [Description("The line number")] int line,
        [Description("The column number")] int column,
        [Description("The diagnostic ID to fix")] string diagnosticId,
        [Description("Optional specific fix ID if multiple fixes available")] string? fixId = null,
        [Description("If true, preview changes without applying")] bool preview = false
    )
    {
        var request = new ApplyCodeFixRequest
        {
            FilePath = filePath,
            Line = line,
            Column = column,
            DiagnosticId = diagnosticId,
            FixId = fixId,
            Preview = preview
        };

        var result = await _rpcClient.ApplyCodeFixAsync(request);
        return JsonSerializer.Serialize(result, _jsonOptions);
    }
}
