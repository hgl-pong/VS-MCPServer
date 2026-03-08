using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class SearchTools
{
    private readonly RpcClient _rpcClient;
    private static readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public SearchTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "find_in_files", ReadOnly = true)]
    [Description("Search for text across all files in the solution or a specific folder. Supports file patterns, case sensitivity, whole word matching, and regular expressions.")]
    public async Task<string> FindInFilesAsync(
        [Description("The text or pattern to search for")] string searchTerm,
        [Description("Optional file pattern to limit search (e.g., '*.cs', '*.xaml')")] string? filePattern = null,
        [Description("Optional folder path to limit search scope")] string? folderPath = null,
        [Description("Match case (default: false)")] bool matchCase = false,
        [Description("Match whole word only (default: false)")] bool matchWholeWord = false,
        [Description("Use regular expression (default: false)")] bool useRegex = false)
    {
        var results = await _rpcClient.FindInFilesAsync(searchTerm, filePattern, folderPath, matchCase, matchWholeWord, useRegex);
        return JsonSerializer.Serialize(results, _jsonOptions);
    }
}
