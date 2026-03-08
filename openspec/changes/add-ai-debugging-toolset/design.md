## Context

VSMCP is a VS 2022/2026 extension that exposes VS features as MCP tools via HTTP/SSE. The architecture consists of:

```
MCP Client (Claude) → MCPServer.Server → Named Pipe → VS Extension → VS APIs (EnvDTE)
```

Current tools cover: Solution, Project, Document, Editor, Build, Navigation (27 tools).

This design adds 41 new tools for AI-assisted debugging, diagnostics, testing, and refactoring.

### Constraints
- Must use EnvDTE/EnvDTE80 for VS API access (no direct VS SDK for debugger)
- All VS API calls require `ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync()`
- Must follow existing MCP tool patterns (`[McpServerToolType]`, `[McpServerTool]`)
- Named pipe JSON-RPC for server-extension communication
- .NET Framework 4.8 for VSIX, .NET 10.0 for Server

## Goals / Non-Goals

**Goals:**
- Enable AI to independently debug code (set breakpoints, step, inspect)
- Provide full variable inspection (locals, watch, evaluate)
- Support call stack and thread navigation
- Expose VS diagnostics and code fixes
- Enable test discovery and execution
- Support common refactoring operations

**Non-Goals:**
- Git/version control tools (out of scope)
- NuGet package management (out of scope)
- Memory inspection (EnvDTE limitation)
- Disassembly view (EnvDTE limitation)
- Attach/detach to processes (security concern)

## Decisions

### D1: Debugger API Access via EnvDTE.Debugger

**Decision:** Use `EnvDTE80.DTE2.Debugger` for all debug operations.

**Rationale:**
- EnvDTE is the stable, documented automation model
- `dte.Debugger` provides: Breakpoints, CurrentMode, CurrentProcess, CurrentThread
- Process/Thread/StackFrame/Expression evaluation all available
- Alternative (IVsDebugger) requires more complex native interop

**Key APIs:**
```csharp
// Debug control
dte.Debugger.Go()           // Start/Continue
dte.Debugger.StepInto()     // F11
dte.Debugger.StepOver()     // F10
dte.Debugger.StepOut()      // Shift+F11
dte.Debugger.Stop()         // Stop debugging
dte.Debugger.CurrentMode    // dbgDebugMode enum

// Breakpoints
dte.Debugger.Breakpoints.Add(File, Line, Condition, HitCount)
dte.Debugger.Breakpoints.Item(index).Delete()
dte.Debugger.Breakpoints.Item(index).Enabled = true/false

// Inspection
dte.Debugger.GetExpression("variableName")
dte.Debugger.ExecuteStatement("code")
frame.Locals / frame.Arguments

// Threads/Stack
dte.Debugger.CurrentProcess.Threads
dte.Debugger.CurrentThread.StackFrames
```

### D2: Tool Organization by Domain

**Decision:** Create separate tool files per functional domain.

**Files:**
| File | Tools | Count |
|------|-------|-------|
| `DebugControlTools.cs` | Session control, stepping | 8 |
| `BreakpointTools.cs` | Breakpoint CRUD | 5 |
| `InspectionTools.cs` | Variables, watch, evaluate | 9 |
| `ThreadStackTools.cs` | Call stack, threads | 4 |
| `DiagnosticTools.cs` | Errors, diagnostics, fixes | 3 |
| `TestTools.cs` | Test discovery and execution | 5 |
| `RefactorTools.cs` | Rename, extract, organize | 3 |
| `OutputTools.cs` | Build/debug output | 2 |
| `ProjectTools.cs` | File operations | 2 |

**Rationale:** Keeps tools organized, follows existing pattern, enables parallel development.

### D3: Data Models in Shared Project

**Decision:** Create new model files in `CodingWithCalvin.MCPServer.Shared/Models/`.

**Files:**
- `DebuggingModels.cs` - DebugState, Breakpoint, Variable, StackFrame, Thread
- `DiagnosticModels.cs` - Diagnostic, CodeFix
- `TestModels.cs` - TestInfo, TestResult

**Rationale:** Shared models ensure type safety across RPC boundary, consistent with existing pattern.

### D4: Watch Window Simulation

**Decision:** Implement watch functionality via expression evaluation, not actual VS Watch window.

**Rationale:**
- VS Watch window API is not directly accessible via EnvDTE
- We maintain a list of watch expressions and evaluate them on demand
- `debugger_get_watch` returns current values of all watched expressions
- Watch list stored in VisualStudioService as `List<string>`

**Implementation:**
```csharp
private List<string> _watchExpressions = new();

public Task<List<WatchItem>> GetWatchAsync() {
    var results = new List<WatchItem>();
    foreach (var expr in _watchExpressions) {
        var value = _dte.Debugger.GetExpression(expr);
        results.Add(new WatchItem(expr, value.Value, value.IsValidValue));
    }
    return results;
}
```

### D5: Test Discovery via VS Test Explorer

**Decision:** Use `IVsTestService` or reflection-based discovery, execution via `dotnet test`.

**Rationale:**
- EnvDTE does not provide test APIs
- Options:
  1. **VS Test Service** (IVsTestService) - Native VS integration
  2. **Shell execute dotnet test** - Simpler, works for most cases
  3. **VSTest.Console** - More control

**Choice:** Start with shell execute `dotnet test --list-tests` for discovery, `dotnet test` for execution. This is simpler and works for all .NET test frameworks.

### D6: Code Fixes via Roslyn

**Decision:** Use `Microsoft.CodeAnalysis` for diagnostics and code fixes.

**Rationale:**
- EnvDTE does not expose Roslyn code fixes
- Need to query `DiagnosticAnalyzer` results
- Apply fixes via `CodeAction` operations

**Challenge:** Requires loading Roslyn workspace in VS context. May need to use VS service APIs.

**Fallback:** If Roslyn access is complex, provide `diagnostics_get` only (read errors), skip `code_fix_apply` for initial version.

## Risks / Trade-offs

### R1: EnvDTE Thread Requirements
**Risk:** All EnvDTE calls must be on UI thread. Forgetting `SwitchToMainThreadAsync()` causes COM exceptions.
**Mitigation:** Create base helper method that all debug tools call first. Add unit tests that verify thread affinity.

### R2: Debug Session State
**Risk:** Tools called when not debugging could throw exceptions or return invalid data.
**Mitigation:** Every debug tool checks `dte.Debugger.CurrentMode` first. Return clear error if not in break mode.

### R3: Expression Evaluation Errors
**Risk:** Invalid expressions or out-of-scope variables cause exceptions.
**Mitigation:** `GetExpression()` returns `IsValidValue` flag. Return structured error in tool response.

### R4: Breakpoint File Paths
**Risk:** File paths may not match VS internal format (forward vs backslash, case sensitivity).
**Mitigation:** Normalize paths using `Path.GetFullPath()` before breakpoint operations.

### R5: Test Execution Async
**Risk:** `dotnet test` is async, may take long time. MCP tools should be responsive.
**Mitigation:** Return immediately with "test started", provide `test_results` tool to poll completion.

## Migration Plan

Not applicable - this is a new feature, no migration needed.

## Open Questions

1. **Test debugging**: Can we launch `dotnet test` under debugger programmatically? May need to set `VSTEST_HOST_DEBUG=1` environment variable.

2. **Roslyn access**: What's the best way to access Roslyn diagnostics from VS extension? May need `IAnalyzersService` or similar.

3. **Refactoring**: Can we use `ICodeRefactoringService` for extract method? Or implement manually via text edits?
