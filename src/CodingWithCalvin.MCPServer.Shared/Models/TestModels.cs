using System.Collections.Generic;

namespace CodingWithCalvin.MCPServer.Shared.Models;

/// <summary>
/// Type of test.
/// </summary>
public enum TestType
{
    Unit,
    Integration,
    Performance,
    Unknown
}

/// <summary>
/// Status of a test result.
/// </summary>
public enum TestStatus
{
    Pending,
    Running,
    Passed,
    Failed,
    Skipped,
    NotFound
}

/// <summary>
/// Information about a discovered test.
/// </summary>
public class TestInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ClassName { get; set; }
    public string? Namespace { get; set; }
    public string? FilePath { get; set; }
    public int Line { get; set; }
    public TestType Type { get; set; }
    public string? ProjectName { get; set; }
    public List<string> Traits { get; set; } = new();
}

/// <summary>
/// Result of running a single test.
/// </summary>
public class TestResult
{
    public string TestId { get; set; } = string.Empty;
    public string TestName { get; set; } = string.Empty;
    public TestStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorStackTrace { get; set; }
    public long DurationMs { get; set; }
    public string? Output { get; set; }
}

/// <summary>
/// Summary of a test run.
/// </summary>
public class TestRunSummary
{
    public int Total { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public long TotalDurationMs { get; set; }
    public List<TestResult> Results { get; set; } = new();
}

/// <summary>
/// Request to run specific tests.
/// </summary>
public class RunTestsRequest
{
    public List<string>? TestNames { get; set; }
    public string? Filter { get; set; }
    public string? ProjectName { get; set; }
    public bool Verbose { get; set; }
}
