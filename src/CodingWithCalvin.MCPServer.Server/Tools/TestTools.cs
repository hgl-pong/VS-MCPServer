using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;

namespace CodingWithCalvin.MCPServer.Server.Tools;

[McpServerToolType]
public class TestTools
{
    private readonly RpcClient _rpcClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    public TestTools(RpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    [McpServerTool(Name = "test_discover", ReadOnly = true)]
    [Description("Discover all tests in the solution or a specific project.")]
    public async Task<string> DiscoverTestsAsync(
        [Description("Optional project name to discover tests in. If omitted, discovers tests in all projects.")] string? projectName = null
    )
    {
        var tests = await _rpcClient.DiscoverTestsAsync(projectName);
        return JsonSerializer.Serialize(tests, _jsonOptions);
    }

    [McpServerTool(Name = "test_run_all")]
    [Description("Run all tests in the solution or a specific project.")]
    public async Task<string> RunAllTestsAsync(
        [Description("Optional project name to run tests in. If omitted, runs all tests.")] string? projectName = null
    )
    {
        var summary = await _rpcClient.RunAllTestsAsync(projectName);
        return JsonSerializer.Serialize(summary, _jsonOptions);
    }

    [McpServerTool(Name = "test_run_specific")]
    [Description("Run specific tests by name or filter pattern.")]
    public async Task<string> RunTestsAsync(
        [Description("Optional list of test names to run")] string[]? testNames = null,
        [Description("Optional filter pattern (e.g., 'ClassName.TestMethod*')")] string? filter = null,
        [Description("Optional project name")] string? projectName = null,
        [Description("Enable verbose output")] bool verbose = false
    )
    {
        var request = new RunTestsRequest
        {
            TestNames = testNames?.ToList(),
            Filter = filter,
            ProjectName = projectName,
            Verbose = verbose
        };

        var summary = await _rpcClient.RunTestsAsync(request);
        return JsonSerializer.Serialize(summary, _jsonOptions);
    }

    [McpServerTool(Name = "test_debug")]
    [Description("Run a specific test under the debugger.")]
    public async Task<string> DebugTestAsync(
        [Description("The test name or fully qualified name to debug")] string testName
    )
    {
        var result = await _rpcClient.DebugTestAsync(testName);
        return JsonSerializer.Serialize(new { success = result }, _jsonOptions);
    }

    [McpServerTool(Name = "test_results", ReadOnly = true)]
    [Description("Get the results of the last test run.")]
    public async Task<string> GetTestResultsAsync()
    {
        var summary = await _rpcClient.GetTestResultsAsync();
        return JsonSerializer.Serialize(summary, _jsonOptions);
    }
}
