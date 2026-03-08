## 1. Shared Models

- [x] 1.1 Create `CodingWithCalvin.MCPServer.Shared/Models/DebuggingModels.cs` with DebugState, BreakpointInfo, VariableInfo, StackFrameInfo, ThreadInfo, WatchItem DTOs
- [x] 1.2 Create `CodingWithCalvin.MCPServer.Shared/Models/DiagnosticModels.cs` with DiagnosticInfo, CodeFixInfo DTOs
- [x] 1.3 Create `CodingWithCalvin.MCPServer.Shared/Models/TestModels.cs` with TestInfo, TestResult DTOs

## 2. RPC Contracts

- [x] 2.1 Add debug control RPC methods to `RpcContracts.cs` (IVisualStudioRpc): GetDebugStateAsync, StartDebuggingAsync, StopDebuggingAsync, ContinueAsync, StepIntoAsync, StepOverAsync, StepOutAsync, RunToCursorAsync
- [x] 2.2 Add breakpoint RPC methods: GetBreakpointsAsync, SetBreakpointAsync, RemoveBreakpointAsync, ToggleBreakpointAsync, SetBreakpointConditionAsync
- [x] 2.3 Add inspection RPC methods: EvaluateExpressionAsync, GetLocalsAsync, GetArgumentsAsync, InspectVariableAsync, SetVariableAsync, GetWatchAsync, AddWatchAsync, RemoveWatchAsync, ClearWatchAsync
- [x] 2.4 Add thread/stack RPC methods: GetCallStackAsync, SetStackFrameAsync, GetThreadsAsync, SetThreadAsync
- [x] 2.5 Add diagnostic RPC methods: GetDiagnosticsAsync, GetErrorListAsync, ApplyCodeFixAsync
- [x] 2.6 Add test RPC methods: DiscoverTestsAsync, RunAllTestsAsync, RunSpecificTestsAsync, DebugTestAsync, GetTestResultsAsync
- [x] 2.7 Add refactor RPC methods: RenameSymbolAsync, ExtractMethodAsync, OrganizeUsingsAsync
- [x] 2.8 Add output RPC methods: GetBuildOutputAsync, GetDebugOutputAsync
- [x] 2.9 Add project RPC methods: AddFileToProjectAsync, CreateProjectItemAsync

## 3. VisualStudioService Interface

- [x] 3.1 Add debug control method signatures to `IVisualStudioService.cs`
- [x] 3.2 Add breakpoint method signatures to `IVisualStudioService.cs`
- [x] 3.3 Add inspection method signatures to `IVisualStudioService.cs`
- [x] 3.4 Add thread/stack method signatures to `IVisualStudioService.cs`
- [x] 3.5 Add diagnostic method signatures to `IVisualStudioService.cs`
- [x] 3.6 Add test method signatures to `IVisualStudioService.cs`
- [x] 3.7 Add refactor method signatures to `IVisualStudioService.cs`
- [x] 3.8 Add output method signatures to `IVisualStudioService.cs`
- [x] 3.9 Add project method signatures to `IVisualStudioService.cs`

## 4. VisualStudioService Implementation - Debug Control

- [x] 4.1 Implement GetDebugStateAsync using dte.Debugger.CurrentMode
- [x] 4.2 Implement StartDebuggingAsync using dte.Debugger.Go()
- [x] 4.3 Implement StopDebuggingAsync using dte.Debugger.Stop()
- [x] 4.4 Implement ContinueAsync (same as StartDebuggingAsync when in break mode)
- [x] 4.5 Implement StepIntoAsync using dte.Debugger.StepInto()
- [x] 4.6 Implement StepOverAsync using dte.Debugger.StepOver()
- [x] 4.7 Implement StepOutAsync using dte.Debugger.StepOut()
- [x] 4.8 Implement RunToCursorAsync using cursor position and execution control

## 5. VisualStudioService Implementation - Breakpoints

- [x] 5.1 Implement GetBreakpointsAsync iterating dte.Debugger.Breakpoints
- [x] 5.2 Implement SetBreakpointAsync using dte.Debugger.Breakpoints.Add()
- [x] 5.3 Implement RemoveBreakpointAsync using Breakpoint.Delete()
- [x] 5.4 Implement ToggleBreakpointAsync using Breakpoint.Enabled property
- [x] 5.5 Implement SetBreakpointConditionAsync modifying Breakpoint.Condition and HitCountTarget

## 6. VisualStudioService Implementation - Inspection

- [x] 6.1 Implement EvaluateExpressionAsync using dte.Debugger.GetExpression()
- [x] 6.2 Implement GetLocalsAsync using CurrentThread.StackFrames[0].Locals
- [x] 6.3 Implement GetArgumentsAsync using CurrentThread.StackFrames[0].Arguments
- [x] 6.4 Implement InspectVariableAsync with recursive member inspection
- [x] 6.5 Implement SetVariableAsync using expression evaluation and assignment
- [x] 6.6 Add _watchExpressions list field to VisualStudioService
- [x] 6.7 Implement GetWatchAsync evaluating all watch expressions
- [x] 6.8 Implement AddWatchAsync adding to _watchExpressions
- [x] 6.9 Implement RemoveWatchAsync removing from _watchExpressions
- [x] 6.10 Implement ClearWatchAsync clearing _watchExpressions

## 7. VisualStudioService Implementation - Threads & Stack

- [x] 7.1 Implement GetCallStackAsync using CurrentThread.StackFrames
- [x] 7.2 Implement SetStackFrameAsync tracking active frame index
- [x] 7.3 Implement GetThreadsAsync using CurrentProcess.Threads
- [x] 7.4 Implement SetThreadAsync switching dte.Debugger.CurrentThread

## 8. VisualStudioService Implementation - Diagnostics

- [x] 8.1 Implement GetDiagnosticsAsync using ErrorListProvider or DTE.ErrorItems
- [x] 8.2 Implement GetErrorListAsync using dte.ToolWindows.ErrorList
- [x] 8.3 Implement ApplyCodeFixAsync (research Roslyn CodeAction access first)

## 9. VisualStudioService Implementation - Testing

- [x] 9.1 Implement DiscoverTestsAsync using dotnet test --list-tests shell execution
- [x] 9.2 Implement RunAllTestsAsync using dotnet test shell execution
- [x] 9.3 Implement RunSpecificTestsAsync using dotnet test --filter
- [x] 9.4 Implement DebugTestAsync with VSTEST_HOST_DEBUG environment variable
- [x] 9.5 Implement GetTestResultsAsync parsing test results

## 10. VisualStudioService Implementation - Refactoring

- [x] 10.1 Implement RenameSymbolAsync using dte.Find.FindReplace or Roslyn Renamer
- [x] 10.2 Implement ExtractMethodAsync using VS code model or text manipulation
- [x] 10.3 Implement OrganizeUsingsAsync using text manipulation or Roslyn

## 11. VisualStudioService Implementation - Output

- [x] 11.1 Implement GetBuildOutputAsync accessing Build output pane
- [x] 11.2 Implement GetDebugOutputAsync accessing Debug output pane

## 12. VisualStudioService Implementation - Project

- [x] 12.1 Implement AddFileToProjectAsync using ProjectItems.AddFromFile
- [x] 12.2 Implement CreateProjectItemAsync using ProjectItems.AddFromTemplate

## 13. RpcClient Proxy Methods

- [x] 13.1 Add all new RPC method proxies to RpcClient.cs
- [x] 13.2 Register new tool types in GetAvailableToolsAsync() toolTypes array

## 14. MCP Server Tools - Debug Control

- [x] 14.1 Create `CodingWithCalvin.MCPServer.Server/Tools/DebugControlTools.cs` with [McpServerToolType]
- [x] 14.2 Implement debugger_state tool
- [x] 14.3 Implement debugger_start tool
- [x] 14.4 Implement debugger_stop tool
- [x] 14.5 Implement debugger_continue tool
- [x] 14.6 Implement debugger_step_into tool
- [x] 14.7 Implement debugger_step_over tool
- [x] 14.8 Implement debugger_step_out tool
- [x] 14.9 Implement debugger_run_to_cursor tool

## 15. MCP Server Tools - Breakpoints

- [x] 15.1 Create `CodingWithCalvin.MCPServer.Server/Tools/BreakpointTools.cs`
- [x] 15.2 Implement breakpoint_list tool
- [x] 15.3 Implement breakpoint_set tool
- [x] 15.4 Implement breakpoint_remove tool
- [x] 15.5 Implement breakpoint_toggle tool
- [x] 15.6 Implement breakpoint_set_condition tool

## 16. MCP Server Tools - Inspection

- [x] 16.1 Create `CodingWithCalvin.MCPServer.Server/Tools/InspectionTools.cs`
- [x] 16.2 Implement debugger_evaluate tool
- [x] 16.3 Implement debugger_get_locals tool
- [x] 16.4 Implement debugger_get_arguments tool
- [x] 16.5 Implement debugger_inspect_variable tool
- [x] 16.6 Implement debugger_set_variable tool
- [x] 16.7 Implement debugger_get_watch tool
- [x] 16.8 Implement debugger_add_watch tool
- [x] 16.9 Implement debugger_remove_watch tool
- [x] 16.10 Implement debugger_clear_watch tool

## 17. MCP Server Tools - Threads & Stack

- [x] 17.1 Create `CodingWithCalvin.MCPServer.Server/Tools/ThreadStackTools.cs`
- [x] 17.2 Implement debugger_call_stack tool
- [x] 17.3 Implement debugger_set_frame tool
- [x] 17.4 Implement debugger_threads tool
- [x] 17.5 Implement debugger_set_thread tool

## 18. MCP Server Tools - Diagnostics

- [x] 18.1 Create `CodingWithCalvin.MCPServer.Server/Tools/DiagnosticTools.cs`
- [x] 18.2 Implement diagnostics_get tool
- [x] 18.3 Implement error_list_get tool
- [x] 18.4 Implement code_fix_apply tool

## 19. MCP Server Tools - Testing

- [x] 19.1 Create `CodingWithCalvin.MCPServer.Server/Tools/TestTools.cs`
- [x] 19.2 Implement test_discover tool
- [x] 19.3 Implement test_run_all tool
- [x] 19.4 Implement test_run_specific tool
- [x] 19.5 Implement test_debug tool
- [x] 19.6 Implement test_results tool

## 20. MCP Server Tools - Refactoring

- [x] 20.1 Create `CodingWithCalvin.MCPServer.Server/Tools/RefactorTools.cs`
- [x] 20.2 Implement refactor_rename tool
- [x] 20.3 Implement refactor_extract_method tool
- [x] 20.4 Implement refactor_organize_usings tool

## 21. MCP Server Tools - Output & Project

- [x] 21.1 Create `CodingWithCalvin.MCPServer.Server/Tools/OutputTools.cs`
- [x] 21.2 Implement output_get_build tool
- [x] 21.3 Implement output_get_debug tool
- [x] 21.4 Create `CodingWithCalvin.MCPServer.Server/Tools/ProjectOperationTools.cs`
- [x] 21.5 Implement project_add_file tool
- [x] 21.6 Implement project_create_item tool

## 22. DI Registration

- [x] 22.1 Register all new tool classes in MCPServer.Server DI container

## 23. Testing & Validation

- [x] 23.1 Build solution and verify no compilation errors
- [ ] 23.2 Test debug control tools with sample project
- [ ] 23.3 Test breakpoint tools with sample project
- [ ] 23.4 Test inspection tools with sample project
- [ ] 23.5 Test all 41 new tools for basic functionality
