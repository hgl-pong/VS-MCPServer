using System.Collections.Generic;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;

namespace CodingWithCalvin.MCPServer.Shared;

// Forward declarations for models that will be in DebuggingModels.cs, DiagnosticModels.cs, TestModels.cs
// These are resolved at compile time

/// <summary>
/// RPC interface for Visual Studio operations.
/// Implemented by VS extension, called by MCP server process.
/// </summary>
public interface IVisualStudioRpc
{
    Task<SolutionInfo?> GetSolutionInfoAsync();
    Task<bool> OpenSolutionAsync(string path);
    Task CloseSolutionAsync(bool saveFirst);
    Task<List<ProjectInfo>> GetProjectsAsync();

    Task<List<DocumentInfo>> GetOpenDocumentsAsync();
    Task<DocumentInfo?> GetActiveDocumentAsync();
    Task<bool> OpenDocumentAsync(string path);
    Task<bool> CloseDocumentAsync(string path, bool save);
    Task<string?> ReadDocumentAsync(string path);
    Task<bool> WriteDocumentAsync(string path, string content);
    Task<SelectionInfo?> GetSelectionAsync();
    Task<bool> SetSelectionAsync(string path, int startLine, int startColumn, int endLine, int endColumn);

    Task<bool> InsertTextAsync(string text);
    Task<bool> ReplaceTextAsync(string oldText, string newText);
    Task<bool> GoToLineAsync(int line);
    Task<List<FindResult>> FindAsync(string searchText, bool matchCase, bool wholeWord);

    Task<bool> BuildSolutionAsync();
    Task<bool> BuildProjectAsync(string projectName);
    Task<bool> CleanSolutionAsync();
    Task<bool> CancelBuildAsync();
    Task<BuildStatus> GetBuildStatusAsync();

    Task<List<SymbolInfo>> GetDocumentSymbolsAsync(string path);
    Task<WorkspaceSymbolResult> SearchWorkspaceSymbolsAsync(string query, int maxResults = 100);
    Task<DefinitionResult> GoToDefinitionAsync(string path, int line, int column);
    Task<ReferencesResult> FindReferencesAsync(string path, int line, int column, int maxResults = 100);

    // Debugger Control
    Task<DebugState> GetDebugStateAsync();
    Task<bool> StartDebuggingAsync();
    Task<bool> StopDebuggingAsync();
    Task<bool> ContinueDebuggingAsync();
    Task<bool> StepIntoAsync();
    Task<bool> StepOverAsync();
    Task<bool> StepOutAsync();
    Task<bool> RunToCursorAsync(string filePath, int line);

    // Breakpoints
    Task<List<BreakpointInfo>> GetBreakpointsAsync();
    Task<BreakpointInfo?> SetBreakpointAsync(SetBreakpointRequest request);
    Task<bool> RemoveBreakpointAsync(string filePath, int line);
    Task<bool> ToggleBreakpointAsync(string filePath, int line);
    Task<bool> SetBreakpointConditionAsync(string filePath, int line, string? condition, int hitCount, string hitCountType);

    // Variable Inspection
    Task<EvaluationResult> EvaluateExpressionAsync(string expression);
    Task<List<VariableInfo>> GetLocalsAsync();
    Task<List<VariableInfo>> GetArgumentsAsync();
    Task<VariableInfo> InspectVariableAsync(string variableName, int depth = 1);
    Task<bool> SetVariableValueAsync(string variableName, string value);

    // Watch Window
    Task<List<WatchItem>> GetWatchExpressionsAsync();
    Task<bool> AddWatchExpressionAsync(string expression);
    Task<bool> RemoveWatchExpressionAsync(string expression);
    Task<bool> ClearWatchExpressionsAsync();

    // Call Stack & Threads
    Task<List<StackFrameInfo>> GetCallStackAsync();
    Task<bool> SetActiveStackFrameAsync(int frameIndex);
    Task<List<ThreadInfo>> GetThreadsAsync();
    Task<bool> SetActiveThreadAsync(int threadId);

    // Diagnostics
    Task<List<DiagnosticInfo>> GetDiagnosticsAsync(string? filePath = null, string? severity = null);
    Task<List<DiagnosticInfo>> GetErrorListAsync();
    Task<CodeFixResult> ApplyCodeFixAsync(ApplyCodeFixRequest request);
    Task<List<CodeFixInfo>> GetCodeFixesAsync(string filePath, int line, int column);

    // Testing
    Task<List<TestInfo>> DiscoverTestsAsync(string? projectName = null);
    Task<TestRunSummary> RunAllTestsAsync(string? projectName = null);
    Task<TestRunSummary> RunTestsAsync(RunTestsRequest request);
    Task<TestRunSummary> GetTestResultsAsync();
    Task<bool> DebugTestAsync(string testName);

    // Refactoring
    Task<List<string>> RenameSymbolAsync(string filePath, int line, int column, string newName);
    Task<string?> ExtractMethodAsync(string filePath, int startLine, int startColumn, int endLine, int endColumn, string newMethodName);
    Task<string?> OrganizeUsingsAsync(string filePath, bool placeSystemFirst = true);

    // Output Windows
    Task<string> GetBuildOutputAsync();
    Task<string> GetDebugOutputAsync();
    Task<bool> WriteToOutputWindowAsync(string paneName, string message);

    // Project Operations
    Task<bool> AddFileToProjectAsync(string projectPath, string filePath);
    Task<bool> CreateProjectItemAsync(string projectPath, string itemTemplate, string itemName, string? folderPath = null);
    Task<bool> RemoveFileFromProjectAsync(string projectPath, string filePath);
    Task<bool> AddProjectReferenceAsync(string projectPath, string referenceProjectPath);
    Task<bool> RemoveProjectReferenceAsync(string projectPath, string referenceProjectPath);
    Task<bool> AddProjectToSolutionAsync(string projectPath);
    Task<bool> RemoveProjectFromSolutionAsync(string projectPath);

    // General
    Task<CommandResult> ExecuteCommandAsync(string commandName, string? args = null);
    Task<IdeStatus> GetIdeStatusAsync();

    // NuGet
    Task<List<ProjectPackage>> GetProjectPackagesAsync(string projectPath);
    Task<NuGetSearchResult> SearchNuGetPackagesAsync(string searchTerm, int skip = 0, int take = 20);
    Task<bool> InstallNuGetPackageAsync(string projectPath, string packageId, string? version = null);
    Task<bool> UpdateNuGetPackageAsync(string projectPath, string packageId, string? version = null);
    Task<bool> UninstallNuGetPackageAsync(string projectPath, string packageId);

    // Build Extensions
    Task<bool> RebuildSolutionAsync();
    Task<List<BuildError>> GetBuildErrorsAsync();

    // Find in Files
    Task<List<FindInFilesResult>> FindInFilesAsync(string searchTerm, string? filePattern = null, string? folderPath = null, bool matchCase = false, bool matchWholeWord = false, bool useRegex = false);

    // Advanced Debugging
    Task<bool> AttachToProcessAsync(int processId);
    Task<List<ProcessInfo>> GetProcessesAsync();
    Task<List<ModuleInfo>> GetModulesAsync();
    Task<MemoryReadResult> ReadMemoryAsync(ulong address, int size);
    Task<List<RegisterInfo>> GetRegistersAsync();

    // Diagnostics Extensions
    Task<List<DiagnosticInfo>> GetXamlBindingErrorsAsync();
}

/// <summary>
/// RPC interface for MCP Server operations.
/// Implemented by MCP server process, called by VS extension.
/// </summary>
public interface IServerRpc
{
    Task<List<ToolInfo>> GetAvailableToolsAsync();
    Task ShutdownAsync();
}
