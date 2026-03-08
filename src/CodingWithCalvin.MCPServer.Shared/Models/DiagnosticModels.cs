using System.Collections.Generic;

namespace CodingWithCalvin.MCPServer.Shared.Models;

/// <summary>
/// Severity level of a diagnostic.
/// </summary>
public enum DiagnosticSeverity
{
    Hidden,
    Info,
    Warning,
    Error
}

/// <summary>
/// Information about a code diagnostic (error, warning, or suggestion).
/// </summary>
public class DiagnosticInfo
{
    public string Id { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DiagnosticSeverity Severity { get; set; }
    public string? FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public string? ProjectName { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Information about an available code fix.
/// </summary>
public class CodeFixInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DiagnosticId { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public int Line { get; set; }
    public int Column { get; set; }
}

/// <summary>
/// Result of applying a code fix.
/// </summary>
public class CodeFixResult
{
    public bool Success { get; set; }
    public string? NewContent { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> ChangedFiles { get; set; } = new();
}

/// <summary>
/// Request to apply a code fix.
/// </summary>
public class ApplyCodeFixRequest
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string DiagnosticId { get; set; } = string.Empty;
    public string? FixId { get; set; }
    public bool Preview { get; set; }
}
