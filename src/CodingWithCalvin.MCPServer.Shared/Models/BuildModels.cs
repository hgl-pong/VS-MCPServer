namespace CodingWithCalvin.MCPServer.Shared.Models;

public class BuildStatus
{
    public string State { get; set; } = string.Empty;
    public int FailedProjects { get; set; }
}

public class BuildError
{
    public string ProjectName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Error, Warning, Message
}

public class FindInFilesResult
{
    public string FilePath { get; set; } = string.Empty;
    public int Line { get; set; }
    public int Column { get; set; }
    public int EndLine { get; set; }
    public int EndColumn { get; set; }
    public string LineText { get; set; } = string.Empty;
    public string MatchText { get; set; } = string.Empty;
}
