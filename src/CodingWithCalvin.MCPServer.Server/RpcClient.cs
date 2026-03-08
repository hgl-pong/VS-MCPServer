using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using ModelContextProtocol.Server;
using StreamJsonRpc;

namespace CodingWithCalvin.MCPServer.Server;

public class RpcClient : IVisualStudioRpc, IServerRpc, IDisposable
{
    private readonly CancellationTokenSource _shutdownCts;
    private NamedPipeClientStream? _pipeClient;
    private JsonRpc? _jsonRpc;
    private IVisualStudioRpc? _proxy;
    private bool _disposed;
    private List<ToolInfo>? _cachedTools;

    public bool IsConnected => _pipeClient?.IsConnected ?? false;

    public RpcClient(CancellationTokenSource shutdownCts)
    {
        _shutdownCts = shutdownCts;
    }

    public async Task ConnectAsync(string pipeName, int timeoutMs = 10000)
    {
        _pipeClient = new NamedPipeClientStream(
            ".",
            pipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await _pipeClient.ConnectAsync(timeoutMs);

        _jsonRpc = JsonRpc.Attach(_pipeClient, this);
        _proxy = _jsonRpc.Attach<IVisualStudioRpc>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _jsonRpc?.Dispose();
        _pipeClient?.Dispose();
    }

    private IVisualStudioRpc Proxy => _proxy ?? throw new InvalidOperationException("Not connected to Visual Studio");

    public Task<List<ToolInfo>> GetAvailableToolsAsync()
    {
        if (_cachedTools != null)
        {
            return Task.FromResult(_cachedTools);
        }

        var tools = new List<ToolInfo>();
        var toolTypes = new[] { typeof(Tools.SolutionTools), typeof(Tools.DocumentTools), typeof(Tools.BuildTools), typeof(Tools.NavigationTools) };

        foreach (var toolType in toolTypes)
        {
            var category = toolType.Name.Replace("Tools", "");

            foreach (var method in toolType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var toolAttr = method.GetCustomAttribute<McpServerToolAttribute>();
                if (toolAttr == null)
                {
                    continue;
                }

                var descAttr = method.GetCustomAttribute<DescriptionAttribute>();

                tools.Add(new ToolInfo
                {
                    Name = toolAttr.Name ?? method.Name,
                    Description = descAttr?.Description ?? string.Empty,
                    Category = category
                });
            }
        }

        _cachedTools = tools;
        return Task.FromResult(tools);
    }

    public Task ShutdownAsync()
    {
        Console.Error.WriteLine("Shutdown requested via RPC");
        _shutdownCts.Cancel();
        return Task.CompletedTask;
    }

    public Task<SolutionInfo?> GetSolutionInfoAsync() => Proxy.GetSolutionInfoAsync();
    public Task<bool> OpenSolutionAsync(string path) => Proxy.OpenSolutionAsync(path);
    public Task CloseSolutionAsync(bool saveFirst) => Proxy.CloseSolutionAsync(saveFirst);
    public Task<List<ProjectInfo>> GetProjectsAsync() => Proxy.GetProjectsAsync();
    public Task<List<DocumentInfo>> GetOpenDocumentsAsync() => Proxy.GetOpenDocumentsAsync();
    public Task<DocumentInfo?> GetActiveDocumentAsync() => Proxy.GetActiveDocumentAsync();
    public Task<bool> OpenDocumentAsync(string path) => Proxy.OpenDocumentAsync(path);
    public Task<bool> CloseDocumentAsync(string path, bool save) => Proxy.CloseDocumentAsync(path, save);
    public Task<string?> ReadDocumentAsync(string path) => Proxy.ReadDocumentAsync(path);
    public Task<bool> WriteDocumentAsync(string path, string content) => Proxy.WriteDocumentAsync(path, content);
    public Task<SelectionInfo?> GetSelectionAsync() => Proxy.GetSelectionAsync();
    public Task<bool> SetSelectionAsync(string path, int startLine, int startColumn, int endLine, int endColumn)
        => Proxy.SetSelectionAsync(path, startLine, startColumn, endLine, endColumn);
    public Task<bool> InsertTextAsync(string text) => Proxy.InsertTextAsync(text);
    public Task<bool> ReplaceTextAsync(string oldText, string newText) => Proxy.ReplaceTextAsync(oldText, newText);
    public Task<bool> GoToLineAsync(int line) => Proxy.GoToLineAsync(line);
    public Task<List<FindResult>> FindAsync(string searchText, bool matchCase, bool wholeWord)
        => Proxy.FindAsync(searchText, matchCase, wholeWord);
    public Task<bool> BuildSolutionAsync() => Proxy.BuildSolutionAsync();
    public Task<bool> BuildProjectAsync(string projectName) => Proxy.BuildProjectAsync(projectName);
    public Task<bool> CleanSolutionAsync() => Proxy.CleanSolutionAsync();
    public Task<bool> CancelBuildAsync() => Proxy.CancelBuildAsync();
    public Task<BuildStatus> GetBuildStatusAsync() => Proxy.GetBuildStatusAsync();

    public Task<List<SymbolInfo>> GetDocumentSymbolsAsync(string path) => Proxy.GetDocumentSymbolsAsync(path);
    public Task<WorkspaceSymbolResult> SearchWorkspaceSymbolsAsync(string query, int maxResults = 100)
        => Proxy.SearchWorkspaceSymbolsAsync(query, maxResults);
    public Task<DefinitionResult> GoToDefinitionAsync(string path, int line, int column)
        => Proxy.GoToDefinitionAsync(path, line, column);
    public Task<ReferencesResult> FindReferencesAsync(string path, int line, int column, int maxResults = 100)
        => Proxy.FindReferencesAsync(path, line, column, maxResults);

    // Debugger Control
    public Task<DebugState> GetDebugStateAsync() => Proxy.GetDebugStateAsync();
    public Task<bool> StartDebuggingAsync() => Proxy.StartDebuggingAsync();
    public Task<bool> StopDebuggingAsync() => Proxy.StopDebuggingAsync();
    public Task<bool> ContinueDebuggingAsync() => Proxy.ContinueDebuggingAsync();
    public Task<bool> StepIntoAsync() => Proxy.StepIntoAsync();
    public Task<bool> StepOverAsync() => Proxy.StepOverAsync();
    public Task<bool> StepOutAsync() => Proxy.StepOutAsync();
    public Task<bool> RunToCursorAsync(string filePath, int line) => Proxy.RunToCursorAsync(filePath, line);

    // Breakpoints
    public Task<List<BreakpointInfo>> GetBreakpointsAsync() => Proxy.GetBreakpointsAsync();
    public Task<BreakpointInfo?> SetBreakpointAsync(SetBreakpointRequest request) => Proxy.SetBreakpointAsync(request);
    public Task<bool> RemoveBreakpointAsync(string filePath, int line) => Proxy.RemoveBreakpointAsync(filePath, line);
    public Task<bool> ToggleBreakpointAsync(string filePath, int line) => Proxy.ToggleBreakpointAsync(filePath, line);
    public Task<bool> SetBreakpointConditionAsync(string filePath, int line, string? condition, int hitCount, string hitCountType)
        => Proxy.SetBreakpointConditionAsync(filePath, line, condition, hitCount, hitCountType);

    // Variable Inspection
    public Task<EvaluationResult> EvaluateExpressionAsync(string expression) => Proxy.EvaluateExpressionAsync(expression);
    public Task<List<VariableInfo>> GetLocalsAsync() => Proxy.GetLocalsAsync();
    public Task<List<VariableInfo>> GetArgumentsAsync() => Proxy.GetArgumentsAsync();
    public Task<VariableInfo> InspectVariableAsync(string variableName, int depth = 1)
        => Proxy.InspectVariableAsync(variableName, depth);
    public Task<bool> SetVariableValueAsync(string variableName, string value)
        => Proxy.SetVariableValueAsync(variableName, value);

    // Watch Window
    public Task<List<WatchItem>> GetWatchExpressionsAsync() => Proxy.GetWatchExpressionsAsync();
    public Task<bool> AddWatchExpressionAsync(string expression) => Proxy.AddWatchExpressionAsync(expression);
    public Task<bool> RemoveWatchExpressionAsync(string expression) => Proxy.RemoveWatchExpressionAsync(expression);
    public Task<bool> ClearWatchExpressionsAsync() => Proxy.ClearWatchExpressionsAsync();

    // Call Stack & Threads
    public Task<List<StackFrameInfo>> GetCallStackAsync() => Proxy.GetCallStackAsync();
    public Task<bool> SetActiveStackFrameAsync(int frameIndex) => Proxy.SetActiveStackFrameAsync(frameIndex);
    public Task<List<ThreadInfo>> GetThreadsAsync() => Proxy.GetThreadsAsync();
    public Task<bool> SetActiveThreadAsync(int threadId) => Proxy.SetActiveThreadAsync(threadId);

    // Diagnostics
    public Task<List<DiagnosticInfo>> GetDiagnosticsAsync(string? filePath = null, string? severity = null)
        => Proxy.GetDiagnosticsAsync(filePath, severity);
    public Task<List<DiagnosticInfo>> GetErrorListAsync() => Proxy.GetErrorListAsync();
    public Task<CodeFixResult> ApplyCodeFixAsync(ApplyCodeFixRequest request) => Proxy.ApplyCodeFixAsync(request);
    public Task<List<CodeFixInfo>> GetCodeFixesAsync(string filePath, int line, int column)
        => Proxy.GetCodeFixesAsync(filePath, line, column);

    // Testing
    public Task<List<TestInfo>> DiscoverTestsAsync(string? projectName = null) => Proxy.DiscoverTestsAsync(projectName);
    public Task<TestRunSummary> RunAllTestsAsync(string? projectName = null) => Proxy.RunAllTestsAsync(projectName);
    public Task<TestRunSummary> RunTestsAsync(RunTestsRequest request) => Proxy.RunTestsAsync(request);
    public Task<TestRunSummary> GetTestResultsAsync() => Proxy.GetTestResultsAsync();
    public Task<bool> DebugTestAsync(string testName) => Proxy.DebugTestAsync(testName);

    // Refactoring
    public Task<List<string>> RenameSymbolAsync(string filePath, int line, int column, string newName)
        => Proxy.RenameSymbolAsync(filePath, line, column, newName);
    public Task<string?> ExtractMethodAsync(string filePath, int startLine, int startColumn, int endLine, int endColumn, string newMethodName)
        => Proxy.ExtractMethodAsync(filePath, startLine, startColumn, endLine, endColumn, newMethodName);
    public Task<string?> OrganizeUsingsAsync(string filePath, bool placeSystemFirst = true)
        => Proxy.OrganizeUsingsAsync(filePath, placeSystemFirst);

    // Output Windows
    public Task<string> GetBuildOutputAsync() => Proxy.GetBuildOutputAsync();
    public Task<string> GetDebugOutputAsync() => Proxy.GetDebugOutputAsync();
    public Task<bool> WriteToOutputWindowAsync(string paneName, string message)
        => Proxy.WriteToOutputWindowAsync(paneName, message);

    // Project Operations
    public Task<bool> AddFileToProjectAsync(string projectPath, string filePath)
        => Proxy.AddFileToProjectAsync(projectPath, filePath);
    public Task<bool> CreateProjectItemAsync(string projectPath, string itemTemplate, string itemName, string? folderPath = null)
        => Proxy.CreateProjectItemAsync(projectPath, itemTemplate, itemName, folderPath);
    public Task<bool> RemoveFileFromProjectAsync(string projectPath, string filePath)
        => Proxy.RemoveFileFromProjectAsync(projectPath, filePath);
    public Task<bool> AddProjectReferenceAsync(string projectPath, string referenceProjectPath)
        => Proxy.AddProjectReferenceAsync(projectPath, referenceProjectPath);
    public Task<bool> RemoveProjectReferenceAsync(string projectPath, string referenceProjectPath)
        => Proxy.RemoveProjectReferenceAsync(projectPath, referenceProjectPath);
    public Task<bool> AddProjectToSolutionAsync(string projectPath)
        => Proxy.AddProjectToSolutionAsync(projectPath);
    public Task<bool> RemoveProjectFromSolutionAsync(string projectPath)
        => Proxy.RemoveProjectFromSolutionAsync(projectPath);

    // General
    public Task<CommandResult> ExecuteCommandAsync(string commandName, string? args = null)
        => Proxy.ExecuteCommandAsync(commandName, args);
    public Task<IdeStatus> GetIdeStatusAsync() => Proxy.GetIdeStatusAsync();

    // NuGet
    public Task<List<ProjectPackage>> GetProjectPackagesAsync(string projectPath)
        => Proxy.GetProjectPackagesAsync(projectPath);
    public Task<NuGetSearchResult> SearchNuGetPackagesAsync(string searchTerm, int skip = 0, int take = 20)
        => Proxy.SearchNuGetPackagesAsync(searchTerm, skip, take);
    public Task<bool> InstallNuGetPackageAsync(string projectPath, string packageId, string? version = null)
        => Proxy.InstallNuGetPackageAsync(projectPath, packageId, version);
    public Task<bool> UpdateNuGetPackageAsync(string projectPath, string packageId, string? version = null)
        => Proxy.UpdateNuGetPackageAsync(projectPath, packageId, version);
    public Task<bool> UninstallNuGetPackageAsync(string projectPath, string packageId)
        => Proxy.UninstallNuGetPackageAsync(projectPath, packageId);

    // Build Extensions
    public Task<bool> RebuildSolutionAsync() => Proxy.RebuildSolutionAsync();
    public Task<List<BuildError>> GetBuildErrorsAsync() => Proxy.GetBuildErrorsAsync();

    // Find in Files
    public Task<List<FindInFilesResult>> FindInFilesAsync(string searchTerm, string? filePattern = null, string? folderPath = null, bool matchCase = false, bool matchWholeWord = false, bool useRegex = false)
        => Proxy.FindInFilesAsync(searchTerm, filePattern, folderPath, matchCase, matchWholeWord, useRegex);

    // Advanced Debugging
    public Task<bool> AttachToProcessAsync(int processId) => Proxy.AttachToProcessAsync(processId);
    public Task<List<ProcessInfo>> GetProcessesAsync() => Proxy.GetProcessesAsync();
    public Task<List<ModuleInfo>> GetModulesAsync() => Proxy.GetModulesAsync();
    public Task<MemoryReadResult> ReadMemoryAsync(ulong address, int size) => Proxy.ReadMemoryAsync(address, size);
    public Task<List<RegisterInfo>> GetRegistersAsync() => Proxy.GetRegistersAsync();

    // Diagnostics Extensions
    public Task<List<DiagnosticInfo>> GetXamlBindingErrorsAsync() => Proxy.GetXamlBindingErrorsAsync();
}
