namespace CodingWithCalvin.MCPServer.Shared.Models;

public class CommandResult
{
    public bool Success { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}

public class IdeStatus
{
    public bool IsSolutionOpen { get; set; }
    public string? SolutionPath { get; set; }
    public bool IsDebugging { get; set; }
    public string? ActiveDocument { get; set; }
    public string BuildState { get; set; } = "NoBuildPerformed";
}
