## Why

VSMCP currently provides basic VS operations (solution, project, document, build, navigation), but lacks critical debugging capabilities that AI assistants need for autonomous software development. Without debugging tools, AI cannot:
- Set and manage breakpoints
- Step through code execution
- Inspect variables, locals, and watch expressions
- Navigate call stacks and threads
- Diagnose and fix runtime issues
- Run and verify tests

This severely limits AI's ability to complete the full development cycle independently.

## What Changes

### New MCP Tools (41 total)

**Debugging Control (8 tools)**
- `debugger_state` - Get current debug mode and process info
- `debugger_start` - Start debugging session (F5)
- `debugger_stop` - Stop debugging (Shift+F5)
- `debugger_continue` - Continue execution from breakpoint
- `debugger_step_into` - Step into function (F11)
- `debugger_step_over` - Step over line (F10)
- `debugger_step_out` - Step out of function (Shift+F11)
- `debugger_run_to_cursor` - Run to cursor position (Ctrl+F10)

**Breakpoint Management (5 tools)**
- `breakpoint_list` - List all breakpoints with details
- `breakpoint_set` - Set breakpoint at file:line with optional condition
- `breakpoint_remove` - Remove breakpoint(s)
- `breakpoint_toggle` - Enable/disable breakpoint
- `breakpoint_set_condition` - Set condition or hit count on breakpoint

**Variable & Watch (9 tools)**
- `debugger_evaluate` - Evaluate expression in debug context
- `debugger_get_locals` - Get local variables in current frame
- `debugger_get_arguments` - Get method arguments
- `debugger_inspect_variable` - Deep inspect object/collection
- `debugger_set_variable` - Modify variable value during debug
- `debugger_get_watch` - Get watch window expressions
- `debugger_add_watch` - Add expression to watch
- `debugger_remove_watch` - Remove from watch
- `debugger_clear_watch` - Clear all watches

**Call Stack & Threads (4 tools)**
- `debugger_call_stack` - Get current call stack
- `debugger_set_frame` - Switch active stack frame
- `debugger_threads` - List all threads in process
- `debugger_set_thread` - Switch to different thread

**Diagnostics (3 tools)**
- `diagnostics_get` - Get Roslyn diagnostics (errors/warnings/suggestions)
- `error_list_get` - Get VS Error List contents
- `code_fix_apply` - Apply suggested code fix

**Testing (5 tools)**
- `test_discover` - Discover all tests in solution
- `test_run_all` - Run all tests
- `test_run_specific` - Run specific test(s)
- `test_debug` - Debug specific test
- `test_results` - Get test execution results

**Refactoring (3 tools)**
- `refactor_rename` - Rename symbol across solution
- `refactor_extract_method` - Extract selected code to method
- `refactor_organize_usings` - Sort and remove unused usings

**Output & Project (4 tools)**
- `output_get_build` - Get build output window content
- `output_get_debug` - Get debug output window content
- `project_add_file` - Add existing file to project
- `project_create_item` - Create new project item (class/interface/etc)

## Capabilities

### New Capabilities

- `debugger-control`: Debug session lifecycle - start, stop, continue, step operations
- `debugger-breakpoints`: Breakpoint management - create, remove, enable, set conditions
- `debugger-inspection`: Variable and expression inspection - locals, arguments, evaluate, watch
- `debugger-threads`: Call stack and thread navigation - frames, threads, context switching
- `diagnostics`: Code diagnostics and fixes - errors, warnings, code fixes
- `testing`: Test discovery and execution - run, debug, results
- `refactoring`: Code refactoring operations - rename, extract, organize
- `output-windows`: Output window access - build output, debug output

### Modified Capabilities

None - all capabilities are new additions.

## Impact

### Code Changes
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/DebugControlTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/BreakpointTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/InspectionTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/ThreadStackTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/DiagnosticTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/TestTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/RefactorTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/OutputTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Server/Tools/ProjectTools.cs`
- **New**: `CodingWithCalvin.MCPServer.Shared/Models/DebuggingModels.cs`
- **New**: `CodingWithCalvin.MCPServer.Shared/Models/DiagnosticModels.cs`
- **New**: `CodingWithCalvin.MCPServer.Shared/Models/TestModels.cs`
- **Modified**: `CodingWithCalvin.MCPServer.Shared/RpcContracts.cs` - Add debug RPC methods
- **Modified**: `CodingWithCalvin.MCPServer/Services/IVisualStudioService.cs` - Add debug service methods
- **Modified**: `CodingWithCalvin.MCPServer/Services/VisualStudioService.cs` - Implement debug methods
- **Modified**: `CodingWithCalvin.MCPServer.Server/RpcClient.cs` - Register new tool types

### Dependencies
- Uses existing `EnvDTE` and `EnvDTE80` for debugger access (no new dependencies)
- Uses `EnvDTE80.Debugger` object model for all debug operations

### API Surface
- 41 new MCP tools exposed via HTTP/SSE endpoint
- All tools follow existing MCP tool patterns
