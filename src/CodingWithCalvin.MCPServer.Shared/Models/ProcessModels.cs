namespace CodingWithCalvin.MCPServer.Shared.Models;

public class ProcessInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? MainWindowTitle { get; set; }
    public bool IsBeingDebugged { get; set; }
}

public class ModuleInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public ulong Address { get; set; }
    public ulong Size { get; set; }
    public bool IsOptimized { get; set; }
    public bool IsDynamic { get; set; }
    public bool IsInMemory { get; set; }
    public string? SymbolStatus { get; set; }
}

public class RegisterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Type { get; set; }
}

public class MemoryReadResult
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public byte[]? Data { get; set; }
    public ulong Address { get; set; }
    public int BytesRead { get; set; }
}
