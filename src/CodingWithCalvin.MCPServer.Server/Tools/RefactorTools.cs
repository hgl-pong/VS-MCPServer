using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class RefactorTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public RefactorTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "refactor_rename")]
    [Description("Rename a symbol (variable, method, class, etc.) across the entire solution.")]
    public async Task<string> RenameSymbolAsync(
        [Description("The file path containing the symbol")] string filePath,
        [Description("The line number of the symbol")] int line,
        [Description("The column number of the symbol")] int column,
        [Description("The new name for the symbol")] string newName
    )
    {
        var changedFiles = await _rpcClient.RenameSymbolAsync(filePath, line, column, newName);
        return JsonSerializer.Serialize(new { changedFiles }, _jsonOptions);
    }

    [McpServerTool(Name = "refactor_extract_method")]
    [Description("Extract selected code into a new method.")]
    public async Task<string> ExtractMethodAsync(
        [Description("The file path")] string filePath,
        [Description("Start line of selection")] int startLine,
        [Description("Start column of selection")] int startColumn,
        [Description("End line of selection")] int endLine,
        [Description("End column of selection")] int endColumn,
        [Description("Name for the new method")] string newMethodName
    )
    {
        var result = await _rpcClient.ExtractMethodAsync(filePath, startLine, startColumn, endLine, endColumn, newMethodName);
        return JsonSerializer.Serialize(new { content = result }, _jsonOptions);
    }

    [McpServerTool(Name = "refactor_organize_usings")]
    [Description("Sort and remove unused using directives in a C# file.")]
    public async Task<string> OrganizeUsingsAsync(
        [Description("The file path")] string filePath,
        [Description("Place System.* usings first (default: true)")] bool placeSystemFirst = true
    )
    {
        var result = await _rpcClient.OrganizeUsingsAsync(filePath, placeSystemFirst);
        return JsonSerializer.Serialize(new { content = result }, _jsonOptions);
    }
}
