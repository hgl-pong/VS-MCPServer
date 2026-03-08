using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared;
using CodingWithCalvin.MCPServer.Shared.Models;
using StreamJsonRpc;

namespace CodingWithCalvin.MCPServer.Services;

[Export(typeof(IRpcServer))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class RpcServer : IRpcServer, IVisualStudioRpc
{
    private readonly IVisualStudioService _vsService;
    private NamedPipeServerStream? _pipeServer;
    private JsonRpc? _jsonRpc;
    private IServerRpc? _serverProxy;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private bool _disposed;

    public string PipeName { get; private set; } = string.Empty;
    public bool IsListening { get; private set; }
    public bool IsConnected => _serverProxy != null;

    [ImportingConstructor]
    public RpcServer(IVisualStudioService vsService)
    {
        _vsService = vsService;
    }

    public async Task StartAsync(string pipeName)
    {
        if (IsListening)
        {
            return;
        }

        PipeName = pipeName;
        _cts = new CancellationTokenSource();
        IsListening = true;

        _listenerTask = Task.Run(() => ListenAsync(_cts.Token));
        await Task.CompletedTask;
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                _pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                await _pipeServer.WaitForConnectionAsync(cancellationToken);

                _jsonRpc = JsonRpc.Attach(_pipeServer, this);
                _serverProxy = _jsonRpc.Attach<IServerRpc>();
                await _jsonRpc.Completion;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception)
            {
                // Connection lost, restart listening
                await Task.Delay(100, cancellationToken);
            }
            finally
            {
                _serverProxy = null;
                _jsonRpc?.Dispose();
                _jsonRpc = null;
                _pipeServer?.Dispose();
                _pipeServer = null;
            }
        }
    }

    public async Task StopAsync()
    {
        if (!IsListening)
        {
            return;
        }

        IsListening = false;
        _cts?.Cancel();

        // Dispose JsonRpc to break out of the Completion await
        _jsonRpc?.Dispose();
        _pipeServer?.Dispose();

        if (_listenerTask != null)
        {
            try
            {
                // Use a timeout to prevent hanging forever
                var timeoutTask = Task.Delay(2000);
                var completedTask = await Task.WhenAny(_listenerTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    // Listener didn't stop in time, just continue
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            catch
            {
                // Ignore other exceptions during shutdown
            }
        }

        _cts?.Dispose();
        _cts = null;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        StopAsync().GetAwaiter().GetResult();
    }

    public async Task<List<ToolInfo>> GetAvailableToolsAsync()
    {
        if (_serverProxy == null)
        {
            return new List<ToolInfo>();
        }

        return await _serverProxy.GetAvailableToolsAsync();
    }

    public async Task RequestShutdownAsync()
    {
        if (_serverProxy != null)
        {
            try
            {
                await _serverProxy.ShutdownAsync();
            }
            catch
            {
            }
        }
    }

    public Task<SolutionInfo?> GetSolutionInfoAsync() => _vsService.GetSolutionInfoAsync();
    public Task<bool> OpenSolutionAsync(string path) => _vsService.OpenSolutionAsync(path);
    public Task CloseSolutionAsync(bool saveFirst) => _vsService.CloseSolutionAsync(saveFirst);
    public Task<List<ProjectInfo>> GetProjectsAsync() => _vsService.GetProjectsAsync();
    public Task<List<DocumentInfo>> GetOpenDocumentsAsync() => _vsService.GetOpenDocumentsAsync();
    public Task<DocumentInfo?> GetActiveDocumentAsync() => _vsService.GetActiveDocumentAsync();
    public Task<bool> OpenDocumentAsync(string path) => _vsService.OpenDocumentAsync(path);
    public Task<bool> CloseDocumentAsync(string path, bool save) => _vsService.CloseDocumentAsync(path, save);
    public Task<string?> ReadDocumentAsync(string path) => _vsService.ReadDocumentAsync(path);
    public Task<bool> WriteDocumentAsync(string path, string content) => _vsService.WriteDocumentAsync(path, content);
    public Task<SelectionInfo?> GetSelectionAsync() => _vsService.GetSelectionAsync();
    public Task<bool> SetSelectionAsync(string path, int startLine, int startColumn, int endLine, int endColumn)
        => _vsService.SetSelectionAsync(path, startLine, startColumn, endLine, endColumn);
    public Task<bool> InsertTextAsync(string text) => _vsService.InsertTextAsync(text);
    public Task<bool> ReplaceTextAsync(string oldText, string newText) => _vsService.ReplaceTextAsync(oldText, newText);
    public Task<bool> GoToLineAsync(int line) => _vsService.GoToLineAsync(line);
    public Task<List<FindResult>> FindAsync(string searchText, bool matchCase, bool wholeWord)
        => _vsService.FindAsync(searchText, matchCase, wholeWord);
    public Task<bool> BuildSolutionAsync() => _vsService.BuildSolutionAsync();
    public Task<bool> BuildProjectAsync(string projectName) => _vsService.BuildProjectAsync(projectName);
    public Task<bool> CleanSolutionAsync() => _vsService.CleanSolutionAsync();
    public Task<bool> CancelBuildAsync() => _vsService.CancelBuildAsync();
    public Task<BuildStatus> GetBuildStatusAsync() => _vsService.GetBuildStatusAsync();

    public Task<List<SymbolInfo>> GetDocumentSymbolsAsync(string path) => _vsService.GetDocumentSymbolsAsync(path);
    public Task<WorkspaceSymbolResult> SearchWorkspaceSymbolsAsync(string query, int maxResults = 100)
        => _vsService.SearchWorkspaceSymbolsAsync(query, maxResults);
    public Task<DefinitionResult> GoToDefinitionAsync(string path, int line, int column)
        => _vsService.GoToDefinitionAsync(path, line, column);
    public Task<ReferencesResult> FindReferencesAsync(string path, int line, int column, int maxResults = 100)
        => _vsService.FindReferencesAsync(path, line, column, maxResults);

    // Debugger Control
    public Task<DebugState> GetDebugStateAsync() => _vsService.GetDebugStateAsync();
    public Task<bool> StartDebuggingAsync() => _vsService.StartDebuggingAsync();
    public Task<bool> StopDebuggingAsync() => _vsService.StopDebuggingAsync();
    public Task<bool> ContinueDebuggingAsync() => _vsService.ContinueDebuggingAsync();
    public Task<bool> StepIntoAsync() => _vsService.StepIntoAsync();
    public Task<bool> StepOverAsync() => _vsService.StepOverAsync();
    public Task<bool> StepOutAsync() => _vsService.StepOutAsync();
    public Task<bool> RunToCursorAsync(string filePath, int line) => _vsService.RunToCursorAsync(filePath, line);

    // Breakpoints
    public Task<List<BreakpointInfo>> GetBreakpointsAsync() => _vsService.GetBreakpointsAsync();
    public Task<BreakpointInfo?> SetBreakpointAsync(SetBreakpointRequest request) => _vsService.SetBreakpointAsync(request);
    public Task<bool> RemoveBreakpointAsync(string filePath, int line) => _vsService.RemoveBreakpointAsync(filePath, line);
    public Task<bool> ToggleBreakpointAsync(string filePath, int line) => _vsService.ToggleBreakpointAsync(filePath, line);
    public Task<bool> SetBreakpointConditionAsync(string filePath, int line, string? condition, int hitCount, string hitCountType)
        => _vsService.SetBreakpointConditionAsync(filePath, line, condition, hitCount, hitCountType);

    // Variable Inspection
    public Task<EvaluationResult> EvaluateExpressionAsync(string expression) => _vsService.EvaluateExpressionAsync(expression);
    public Task<List<VariableInfo>> GetLocalsAsync() => _vsService.GetLocalsAsync();
    public Task<List<VariableInfo>> GetArgumentsAsync() => _vsService.GetArgumentsAsync();
    public Task<VariableInfo> InspectVariableAsync(string variableName, int depth = 1)
        => _vsService.InspectVariableAsync(variableName, depth);
    public Task<bool> SetVariableValueAsync(string variableName, string value)
        => _vsService.SetVariableValueAsync(variableName, value);

    // Watch Window
    public Task<List<WatchItem>> GetWatchExpressionsAsync() => _vsService.GetWatchExpressionsAsync();
    public Task<bool> AddWatchExpressionAsync(string expression) => _vsService.AddWatchExpressionAsync(expression);
    public Task<bool> RemoveWatchExpressionAsync(string expression) => _vsService.RemoveWatchExpressionAsync(expression);
    public Task<bool> ClearWatchExpressionsAsync() => _vsService.ClearWatchExpressionsAsync();

    // Call Stack & Threads
    public Task<List<StackFrameInfo>> GetCallStackAsync() => _vsService.GetCallStackAsync();
    public Task<bool> SetActiveStackFrameAsync(int frameIndex) => _vsService.SetActiveStackFrameAsync(frameIndex);
    public Task<List<ThreadInfo>> GetThreadsAsync() => _vsService.GetThreadsAsync();
    public Task<bool> SetActiveThreadAsync(int threadId) => _vsService.SetActiveThreadAsync(threadId);

    // Diagnostics
    public Task<List<DiagnosticInfo>> GetDiagnosticsAsync(string? filePath = null, string? severity = null)
        => _vsService.GetDiagnosticsAsync(filePath, severity);
    public Task<List<DiagnosticInfo>> GetErrorListAsync() => _vsService.GetErrorListAsync();
    public Task<CodeFixResult> ApplyCodeFixAsync(ApplyCodeFixRequest request) => _vsService.ApplyCodeFixAsync(request);
    public Task<List<CodeFixInfo>> GetCodeFixesAsync(string filePath, int line, int column)
        => _vsService.GetCodeFixesAsync(filePath, line, column);

    // Testing
    public Task<List<TestInfo>> DiscoverTestsAsync(string? projectName = null) => _vsService.DiscoverTestsAsync(projectName);
    public Task<TestRunSummary> RunAllTestsAsync(string? projectName = null) => _vsService.RunAllTestsAsync(projectName);
    public Task<TestRunSummary> RunTestsAsync(RunTestsRequest request) => _vsService.RunTestsAsync(request);
    public Task<TestRunSummary> GetTestResultsAsync() => _vsService.GetTestResultsAsync();
    public Task<bool> DebugTestAsync(string testName) => _vsService.DebugTestAsync(testName);

    // Refactoring
    public Task<List<string>> RenameSymbolAsync(string filePath, int line, int column, string newName)
        => _vsService.RenameSymbolAsync(filePath, line, column, newName);
    public Task<string?> ExtractMethodAsync(string filePath, int startLine, int startColumn, int endLine, int endColumn, string newMethodName)
        => _vsService.ExtractMethodAsync(filePath, startLine, startColumn, endLine, endColumn, newMethodName);
    public Task<string?> OrganizeUsingsAsync(string filePath, bool placeSystemFirst = true)
        => _vsService.OrganizeUsingsAsync(filePath, placeSystemFirst);

    // Output Windows
    public Task<string> GetBuildOutputAsync() => _vsService.GetBuildOutputAsync();
    public Task<string> GetDebugOutputAsync() => _vsService.GetDebugOutputAsync();
    public Task<bool> WriteToOutputWindowAsync(string paneName, string message)
        => _vsService.WriteToOutputWindowAsync(paneName, message);

    // Project Operations
    public Task<bool> AddFileToProjectAsync(string projectPath, string filePath)
        => _vsService.AddFileToProjectAsync(projectPath, filePath);
    public Task<bool> CreateProjectItemAsync(string projectPath, string itemTemplate, string itemName, string? folderPath = null)
        => _vsService.CreateProjectItemAsync(projectPath, itemTemplate, itemName, folderPath);
    public Task<bool> RemoveFileFromProjectAsync(string projectPath, string filePath)
        => _vsService.RemoveFileFromProjectAsync(projectPath, filePath);
    public Task<bool> AddProjectReferenceAsync(string projectPath, string referenceProjectPath)
        => _vsService.AddProjectReferenceAsync(projectPath, referenceProjectPath);
    public Task<bool> RemoveProjectReferenceAsync(string projectPath, string referenceProjectPath)
        => _vsService.RemoveProjectReferenceAsync(projectPath, referenceProjectPath);
    public Task<bool> AddProjectToSolutionAsync(string projectPath)
        => _vsService.AddProjectToSolutionAsync(projectPath);
    public Task<bool> RemoveProjectFromSolutionAsync(string projectPath)
        => _vsService.RemoveProjectFromSolutionAsync(projectPath);

    // General
    public Task<CommandResult> ExecuteCommandAsync(string commandName, string? args = null)
        => _vsService.ExecuteCommandAsync(commandName, args);
    public Task<IdeStatus> GetIdeStatusAsync() => _vsService.GetIdeStatusAsync();

    // NuGet
    public Task<List<ProjectPackage>> GetProjectPackagesAsync(string projectPath)
        => _vsService.GetProjectPackagesAsync(projectPath);
    public Task<NuGetSearchResult> SearchNuGetPackagesAsync(string searchTerm, int skip = 0, int take = 20)
        => _vsService.SearchNuGetPackagesAsync(searchTerm, skip, take);
    public Task<bool> InstallNuGetPackageAsync(string projectPath, string packageId, string? version = null)
        => _vsService.InstallNuGetPackageAsync(projectPath, packageId, version);
    public Task<bool> UpdateNuGetPackageAsync(string projectPath, string packageId, string? version = null)
        => _vsService.UpdateNuGetPackageAsync(projectPath, packageId, version);
    public Task<bool> UninstallNuGetPackageAsync(string projectPath, string packageId)
        => _vsService.UninstallNuGetPackageAsync(projectPath, packageId);

    // Build Extensions
    public Task<bool> RebuildSolutionAsync() => _vsService.RebuildSolutionAsync();
    public Task<List<BuildError>> GetBuildErrorsAsync() => _vsService.GetBuildErrorsAsync();

    // Find in Files
    public Task<List<FindInFilesResult>> FindInFilesAsync(string searchTerm, string? filePattern = null, string? folderPath = null, bool matchCase = false, bool matchWholeWord = false, bool useRegex = false)
        => _vsService.FindInFilesAsync(searchTerm, filePattern, folderPath, matchCase, matchWholeWord, useRegex);

    // Advanced Debugging
    public Task<bool> AttachToProcessAsync(int processId) => _vsService.AttachToProcessAsync(processId);
    public Task<List<ProcessInfo>> GetProcessesAsync() => _vsService.GetProcessesAsync();
    public Task<List<ModuleInfo>> GetModulesAsync() => _vsService.GetModulesAsync();
    public Task<MemoryReadResult> ReadMemoryAsync(ulong address, int size) => _vsService.ReadMemoryAsync(address, size);
    public Task<List<RegisterInfo>> GetRegistersAsync() => _vsService.GetRegistersAsync();

    // Diagnostics Extensions
    public Task<List<DiagnosticInfo>> GetXamlBindingErrorsAsync() => _vsService.GetXamlBindingErrorsAsync();
}
