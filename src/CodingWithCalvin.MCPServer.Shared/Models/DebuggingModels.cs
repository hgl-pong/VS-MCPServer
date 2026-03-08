using System.Collections.Generic;

namespace CodingWithCalvin.MCPServer.Shared.Models;

/// <summary>
/// Represents the current state of the debugger.
/// </summary>
public enum DebugMode
{
    Design,  // No debugging session
    Break,   // Paused at breakpoint or step
    Run      // Currently executing
}

/// <summary>
/// Information about the current debugger state.
/// </summary>
public class DebugState
{
    public DebugMode Mode { get; set; }
    public string? ProcessName { get; set; }
    public int ProcessId { get; set; }
    public int ThreadId { get; set; }
    public string? ThreadName { get; set; }
}

/// <summary>
/// Information about a breakpoint.
/// </summary>
public class BreakpointInfo
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public bool Enabled { get; set; }
    public string? Condition { get; set; }
    public int HitCountTarget { get; set; }
    public string HitCountType { get; set; } = string.Empty; // "always", "equal", "greater", "multiple"
    public string? FunctionName { get; set; }
}

/// <summary>
/// Request to set or modify a breakpoint.
/// </summary>
public class SetBreakpointRequest
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; } = 1;
    public string? Condition { get; set; }
    public int HitCount { get; set; }
    public string HitCountType { get; set; } = "always"; // "always", "equal", "greater", "multiple"
}

/// <summary>
/// Information about a variable or expression value.
/// </summary>
public class VariableInfo
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public List<VariableInfo>? Members { get; set; }
    public bool IsExpandable { get; set; }
}

/// <summary>
/// Result of evaluating an expression.
/// </summary>
public class EvaluationResult
{
    public string Expression { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Information about a stack frame in the call stack.
/// </summary>
public class StackFrameInfo
{
    public int Index { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string? FileName { get; set; }
}

/// <summary>
/// Information about a thread in the debugged process.
/// </summary>
public class ThreadInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public bool IsCurrent { get; set; }
    public string? FilePath { get; set; }
    public int Line { get; set; }
}

/// <summary>
/// An item in the watch window.
/// </summary>
public class WatchItem
{
    public string Expression { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
}
