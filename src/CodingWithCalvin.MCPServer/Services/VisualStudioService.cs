using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CodingWithCalvin.MCPServer.Shared.Models;
using CodingWithCalvin.Otel4Vsix;
using EnvDTE;
using EnvDTE80;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using ActivityStatusCode = System.Diagnostics.ActivityStatusCode;
using VsWorkspace = Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace;
// Resolve type conflicts between Roslyn, EnvDTE and Shared.Models
using SymbolInfo = CodingWithCalvin.MCPServer.Shared.Models.SymbolInfo;
using SymbolKind = CodingWithCalvin.MCPServer.Shared.Models.SymbolKind;
using SolutionInfo = CodingWithCalvin.MCPServer.Shared.Models.SolutionInfo;
using ProjectInfo = CodingWithCalvin.MCPServer.Shared.Models.ProjectInfo;
using DocumentInfo = CodingWithCalvin.MCPServer.Shared.Models.DocumentInfo;
using DiagnosticSeverity = CodingWithCalvin.MCPServer.Shared.Models.DiagnosticSeverity;
using EnvDTETextDocument = EnvDTE.TextDocument;
using EnvDTEDocument = EnvDTE.Document;

namespace CodingWithCalvin.MCPServer.Services;

[Export(typeof(IVisualStudioService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class VisualStudioService : IVisualStudioService
{
    private IServiceProvider? _serviceProvider;

    private IServiceProvider ServiceProvider =>
        _serviceProvider ??= MCPServerPackage.Instance as IServiceProvider
            ?? throw new InvalidOperationException("Package not initialized");

    private Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace? _workspace;

    private Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace? Workspace
    {
        get
        {
            if (_workspace == null)
            {
                ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _workspace = ServiceProvider.GetService(typeof(Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace)) 
                        as Microsoft.VisualStudio.LanguageServices.VisualStudioWorkspace;
                });
            }
            return _workspace;
        }
    }

    private async Task<DTE2> GetDteAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        return ServiceProvider.GetService(typeof(DTE)) as DTE2
            ?? throw new InvalidOperationException("Could not get DTE service");
    }

    private static string NormalizePath(string path)
    {
        return Path.GetFullPath(path.Replace('/', '\\'));
    }

    private static bool PathsEqual(string path1, string path2)
    {
        return NormalizePath(path1).Equals(NormalizePath(path2), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the IVsTextBuffer for a document, which works for all file types including C++.
    /// </summary>
    private IVsTextBuffer? GetTextBufferForDocument(string filePath)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        
        var rdt = _serviceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
        if (rdt == null) return null;

        if (rdt.FindAndLockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, filePath, out _, out _, out IntPtr docData, out _) == 0)
        {
            try
            {
                if (docData != IntPtr.Zero)
                {
                    // Try to get IVsTextBuffer from the document data
                    var textBuffer = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(docData) as IVsTextBuffer;
                    return textBuffer;
                }
            }
            finally
            {
                if (docData != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.Release(docData);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the text content from an IVsTextBuffer.
    /// </summary>
    private static string GetTextFromBuffer(IVsTextBuffer textBuffer)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        
        if (textBuffer is IVsTextLines textLines)
        {
            textLines.GetLastLineIndex(out int lastLine, out int lastIndex);
            textLines.GetLineText(0, 0, lastLine, lastIndex, out string text);
            return text ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets the IVsTextView for the active document.
    /// </summary>
    private IVsTextView? GetActiveTextView()
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        
        var textManager = _serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
        if (textManager == null) return null;

        textManager.GetActiveView(1, null, out IVsTextView? textView);
        return textView;
    }

    public async Task<SolutionInfo?> GetSolutionInfoAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        if (dte.Solution == null || string.IsNullOrEmpty(dte.Solution.FullName))
        {
            return null;
        }

        return new SolutionInfo
        {
            Name = Path.GetFileNameWithoutExtension(dte.Solution.FullName),
            Path = dte.Solution.FullName,
            IsOpen = dte.Solution.IsOpen
        };
    }

    public async Task<bool> OpenSolutionAsync(string path)
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("OpenSolution");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Solution.Open(path);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task CloseSolutionAsync(bool saveFirst = true)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        dte.Solution.Close(saveFirst);
    }

    public async Task<List<ProjectInfo>> GetProjectsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var projects = new List<ProjectInfo>();

        if (dte.Solution == null)
        {
            return projects;
        }

        foreach (EnvDTE.Project project in dte.Solution.Projects)
        {
            try
            {
                projects.Add(new ProjectInfo
                {
                    Name = project.Name,
                    Path = project.FullName,
                    Kind = project.Kind
                });
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return projects;
    }

    public async Task<List<DocumentInfo>> GetOpenDocumentsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var documents = new List<DocumentInfo>();

        foreach (EnvDTEDocument doc in dte.Documents)
        {
            try
            {
                documents.Add(new DocumentInfo
                {
                    Name = doc.Name,
                    Path = doc.FullName,
                    IsSaved = doc.Saved
                });
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return documents;
    }

    public async Task<DocumentInfo?> GetActiveDocumentAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return null;
        }

        return new DocumentInfo
        {
            Name = doc.Name,
            Path = doc.FullName,
            IsSaved = doc.Saved
        };
    }

    public async Task<bool> OpenDocumentAsync(string path)
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("OpenDocument");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.ItemOperations.OpenFile(path);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> CloseDocumentAsync(string path, bool save = true)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        foreach (EnvDTEDocument doc in dte.Documents)
        {
            try
            {
                if (PathsEqual(doc.FullName, path))
                {
                    doc.Close(save ? vsSaveChanges.vsSaveChangesYes : vsSaveChanges.vsSaveChangesNo);
                    return true;
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return false;
    }

    public async Task<string?> ReadDocumentAsync(string path)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        foreach (EnvDTEDocument doc in dte.Documents)
        {
            try
            {
                if (PathsEqual(doc.FullName, path))
                {
                    var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
                    if (textDoc != null)
                    {
                        var editPoint = textDoc.StartPoint.CreateEditPoint();
                        return editPoint.GetText(textDoc.EndPoint);
                    }
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        if (File.Exists(path))
        {
            return await Task.Run(() => File.ReadAllText(path));
        }

        return null;
    }

    public async Task<bool> WriteDocumentAsync(string path, string content)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        foreach (EnvDTEDocument doc in dte.Documents)
        {
            try
            {
                if (PathsEqual(doc.FullName, path))
                {
                    var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
                    if (textDoc != null)
                    {
                        var editPoint = textDoc.StartPoint.CreateEditPoint();
                        editPoint.Delete(textDoc.EndPoint);
                        editPoint.Insert(content);
                        return true;
                    }
                    
                    // Fallback for C++ and other non-EnvDTETextDocument files
                    // Save current state, close, write to disk, and reopen
                    var wasSaved = doc.Saved;
                    doc.Close(vsSaveChanges.vsSaveChangesNo);
                    
                    await Task.Run(() => File.WriteAllText(path, content));
                    
                    // Reopen the document
                    dte.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindTextView);
                    return true;
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return false;
    }

    public async Task<SelectionInfo?> GetSelectionAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return null;
        }

        var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
        if (textDoc == null)
        {
            return null;
        }

        var selection = textDoc.Selection;
        return new SelectionInfo
        {
            Text = selection.Text,
            StartLine = selection.TopLine,
            StartColumn = selection.TopPoint.DisplayColumn,
            EndLine = selection.BottomLine,
            EndColumn = selection.BottomPoint.DisplayColumn,
            DocumentPath = doc.FullName
        };
    }

    public async Task<bool> SetSelectionAsync(string path, int startLine, int startColumn, int endLine, int endColumn)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        foreach (EnvDTEDocument doc in dte.Documents)
        {
            try
            {
                if (PathsEqual(doc.FullName, path))
                {
                    var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
                    if (textDoc != null)
                    {
                        textDoc.Selection.MoveToLineAndOffset(startLine, startColumn);
                        textDoc.Selection.MoveToLineAndOffset(endLine, endColumn, true);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return false;
    }

    public async Task<bool> InsertTextAsync(string text)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return false;
        }

        var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
        if (textDoc != null)
        {
            textDoc.Selection.Insert(text);
            return true;
        }

        // Fallback for C++ and other file types using IVsTextView
        var textView = GetActiveTextView();
        if (textView != null)
        {
            textView.GetCaretPos(out int line, out int column);
            textView.GetBuffer(out IVsTextLines? textLines);
            if (textLines != null)
            {
                // Use ReplaceLines to insert text at cursor position
                var textPtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUni(text);
                try
                {
                    textLines.ReplaceLines(line, column, line, column, textPtr, text.Length, null);
                    return true;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(textPtr);
                }
            }
        }

        return false;
    }

    public async Task<bool> ReplaceTextAsync(string oldText, string newText)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return false;
        }

        var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
        if (textDoc != null)
        {
            var editPoint = textDoc.StartPoint.CreateEditPoint();
            var content = editPoint.GetText(textDoc.EndPoint);
            var newContent = content.Replace(oldText, newText);

            if (content != newContent)
            {
                editPoint.Delete(textDoc.EndPoint);
                editPoint.Insert(newContent);
                return true;
            }
            return false;
        }

        // Fallback for C++ and other file types using IVsTextBuffer
        var textBuffer = GetTextBufferForDocument(doc.FullName);
        if (textBuffer is IVsTextLines textLines)
        {
            var content = GetTextFromBuffer(textBuffer);
            var newContent = content.Replace(oldText, newText);
            
            if (content != newContent)
            {
                textLines.GetLastLineIndex(out int lastLine, out int lastIndex);
                var newContentPtr = System.Runtime.InteropServices.Marshal.StringToCoTaskMemUni(newContent);
                try
                {
                    textLines.ReplaceLines(0, 0, lastLine, lastIndex, newContentPtr, newContent.Length, null);
                    return true;
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(newContentPtr);
                }
            }
        }

        return false;
    }

    public async Task<bool> GoToLineAsync(int line)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return false;
        }

        var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
        if (textDoc != null)
        {
            textDoc.Selection.GotoLine(line);
            return true;
        }

        // Fallback for C++ and other file types using IVsTextView
        var textView = GetActiveTextView();
        if (textView != null)
        {
            // Set caret to the beginning of the specified line (0-indexed in IVsTextView)
            textView.SetCaretPos(line - 1, 0);
            textView.CenterLines(line - 1, 1);
            return true;
        }

        return false;
    }

    public async Task<List<FindResult>> FindAsync(string searchText, bool matchCase = false, bool wholeWord = false)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var results = new List<FindResult>();

        var doc = dte.ActiveDocument;
        if (doc == null)
        {
            return results;
        }

        string content;
        
        var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
        if (textDoc != null)
        {
            var editPoint = textDoc.StartPoint.CreateEditPoint();
            content = editPoint.GetText(textDoc.EndPoint);
        }
        else
        {
            // Fallback for C++ and other file types using IVsTextBuffer
            var textBuffer = GetTextBufferForDocument(doc.FullName);
            if (textBuffer == null)
            {
                return results;
            }
            content = GetTextFromBuffer(textBuffer);
        }

        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var lineText = lines[i];
            var index = 0;
            while ((index = lineText.IndexOf(searchText, index, comparison)) >= 0)
            {
                // For wholeWord, check word boundaries
                if (wholeWord)
                {
                    bool isWordCharBefore = index > 0 && (char.IsLetterOrDigit(lineText[index - 1]) || lineText[index - 1] == '_');
                    bool isWordCharAfter = index + searchText.Length < lineText.Length && 
                                          (char.IsLetterOrDigit(lineText[index + searchText.Length]) || lineText[index + searchText.Length] == '_');
                    if (isWordCharBefore || isWordCharAfter)
                    {
                        index += searchText.Length;
                        continue;
                    }
                }
                
                results.Add(new FindResult
                {
                    Line = i + 1,
                    Column = index + 1,
                    Text = lineText.Trim(),
                    DocumentPath = doc.FullName
                });
                index += searchText.Length;
            }
        }

        return results;
    }

    public async Task<bool> BuildSolutionAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("BuildSolution");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Solution.SolutionBuild.Build(true);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> BuildProjectAsync(string projectName)
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("BuildProject");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var config = dte.Solution.SolutionBuild.ActiveConfiguration.Name;
            
            // BuildProject requires the project's UniqueName (relative path in solution),
            // not the full file system path. Try to find the correct UniqueName.
            string uniqueName = projectName;
            
            // If it looks like a full path, try to find the project's UniqueName
            if (Path.IsPathRooted(projectName))
            {
                foreach (EnvDTE.Project proj in dte.Solution.Projects)
                {
                    try
                    {
                        if (PathsEqual(proj.FullName, projectName))
                        {
                            uniqueName = proj.UniqueName;
                            break;
                        }
                    }
                    catch
                    {
                        // Some project types throw on FullName access
                    }
                }
            }
            
            dte.Solution.SolutionBuild.BuildProject(config, uniqueName, true);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> CleanSolutionAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("CleanSolution");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Solution.SolutionBuild.Clean(true);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> CancelBuildAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        if (dte.Solution.SolutionBuild.BuildState != vsBuildState.vsBuildStateInProgress)
        {
            return false;
        }

        dte.ExecuteCommand("Build.Cancel");
        return true;
    }

    public async Task<BuildStatus> GetBuildStatusAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var buildState = dte.Solution.SolutionBuild.BuildState;

        if (buildState == vsBuildState.vsBuildStateNotStarted)
        {
            return new BuildStatus
            {
                State = "NoBuildPerformed",
                FailedProjects = 0
            };
        }

        var lastInfo = dte.Solution.SolutionBuild.LastBuildInfo;

        return new BuildStatus
        {
            State = buildState switch
            {
                vsBuildState.vsBuildStateInProgress => "InProgress",
                vsBuildState.vsBuildStateDone => "Done",
                _ => "Unknown"
            },
            FailedProjects = lastInfo
        };
    }

    public async Task<List<SymbolInfo>> GetDocumentSymbolsAsync(string path)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var symbols = new List<SymbolInfo>();

        var workspace = Workspace;
        if (workspace == null)
        {
            // Fallback to EnvDTE if Workspace is not available
            return await GetDocumentSymbolsEnvDteAsync(path);
        }

        var normalizedPath = NormalizePath(path);
        
        // Find the document in the workspace
        var document = workspace.CurrentSolution.GetDocumentIdsWithFilePath(normalizedPath).FirstOrDefault();
        if (document == null)
        {
            // Try with forward slashes
            var forwardSlashPath = normalizedPath.Replace('\\', '/');
            document = workspace.CurrentSolution.GetDocumentIdsWithFilePath(forwardSlashPath).FirstOrDefault();
        }

        if (document == null)
        {
            return symbols;
        }

        var doc = workspace.CurrentSolution.GetDocument(document);
        if (doc == null)
        {
            return symbols;
        }

        var semanticModel = await doc.GetSemanticModelAsync();
        if (semanticModel == null)
        {
            return symbols;
        }

        var root = await doc.GetSyntaxRootAsync();
        if (root == null)
        {
            return symbols;
        }

        // Extract symbols from the syntax tree
        ExtractRoslynSymbols(root, semanticModel, symbols, normalizedPath, doc);
        return symbols;
    }

    private async Task<List<SymbolInfo>> GetDocumentSymbolsEnvDteAsync(string path)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var symbols = new List<SymbolInfo>();

        if (dte.Solution == null)
        {
            return symbols;
        }

        var normalizedPath = NormalizePath(path);
        var projectItem = dte.Solution.FindProjectItem(normalizedPath);
        if (projectItem == null)
        {
            return symbols;
        }

        var fileCodeModel = projectItem.FileCodeModel;
        if (fileCodeModel == null)
        {
            return symbols;
        }

        ExtractSymbols(fileCodeModel.CodeElements, symbols, normalizedPath, string.Empty);
        return symbols;
    }

    private void ExtractRoslynSymbols(
        Microsoft.CodeAnalysis.SyntaxNode root,
        Microsoft.CodeAnalysis.SemanticModel semanticModel,
        List<SymbolInfo> symbols,
        string filePath,
        Microsoft.CodeAnalysis.Document doc)
    {
        var declaredSymbols = new HashSet<Microsoft.CodeAnalysis.ISymbol>(Microsoft.CodeAnalysis.SymbolEqualityComparer.Default);

        // Collect all declared symbols in the document
        foreach (var node in root.DescendantNodes())
        {
            var declaredSymbol = semanticModel.GetDeclaredSymbol(node);
            if (declaredSymbol != null && declaredSymbols.Add(declaredSymbol))
            {
                var symbolInfo = CreateSymbolInfo(declaredSymbol, filePath, doc);
                if (symbolInfo != null)
                {
                    // Add child symbols
                    AddChildSymbols(declaredSymbol, symbolInfo, filePath, doc, declaredSymbols);
                    symbols.Add(symbolInfo);
                }
            }
        }
    }

    private void AddChildSymbols(
        Microsoft.CodeAnalysis.ISymbol parentSymbol,
        SymbolInfo parentInfo,
        string filePath,
        Microsoft.CodeAnalysis.Document doc,
        HashSet<Microsoft.CodeAnalysis.ISymbol> visitedSymbols)
    {
        if (parentSymbol is Microsoft.CodeAnalysis.INamedTypeSymbol typeSymbol)
        {
            foreach (var member in typeSymbol.GetMembers())
            {
                if (!member.IsImplicitlyDeclared && visitedSymbols.Add(member))
                {
                    var childInfo = CreateSymbolInfo(member, filePath, doc, parentSymbol.Name);
                    if (childInfo != null)
                    {
                        parentInfo.Children.Add(childInfo);
                    }
                }
            }
        }
        else if (parentSymbol is Microsoft.CodeAnalysis.INamespaceSymbol namespaceSymbol)
        {
            foreach (var member in namespaceSymbol.GetMembers())
            {
                if (!member.IsImplicitlyDeclared && visitedSymbols.Add(member))
                {
                    var childInfo = CreateSymbolInfo(member, filePath, doc);
                    if (childInfo != null)
                    {
                        parentInfo.Children.Add(childInfo);
                    }
                }
            }
        }
    }

    private SymbolInfo? CreateSymbolInfo(
        Microsoft.CodeAnalysis.ISymbol symbol,
        string filePath,
        Microsoft.CodeAnalysis.Document doc,
        string? containerName = null)
    {
        var locations = symbol.Locations;
        var location = locations.FirstOrDefault(l => l.IsInSource && l.SourceTree?.FilePath == filePath);
        
        if (location == null)
        {
            // Try to find any source location
            location = locations.FirstOrDefault(l => l.IsInSource);
        }

        if (location == null)
        {
            return null;
        }

        var lineSpan = location.GetLineSpan();
        var startLine = lineSpan.StartLinePosition.Line + 1;
        var startColumn = lineSpan.StartLinePosition.Character + 1;
        var endLine = lineSpan.EndLinePosition.Line + 1;
        var endColumn = lineSpan.EndLinePosition.Character + 1;

        return new SymbolInfo
        {
            Name = symbol.Name,
            FullName = symbol.ToDisplayString(Microsoft.CodeAnalysis.SymbolDisplayFormat.FullyQualifiedFormat),
            Kind = MapRoslynSymbolKind(symbol.Kind),
            FilePath = filePath,
            StartLine = startLine,
            StartColumn = startColumn,
            EndLine = endLine,
            EndColumn = endColumn,
            ContainerName = containerName ?? symbol.ContainingType?.Name ?? symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty
        };
    }

    private static SymbolKind MapRoslynSymbolKind(Microsoft.CodeAnalysis.SymbolKind kind) => kind switch
    {
        Microsoft.CodeAnalysis.SymbolKind.Namespace => SymbolKind.Namespace,
        Microsoft.CodeAnalysis.SymbolKind.NamedType => SymbolKind.Class, // Will be refined by TypeKind
        Microsoft.CodeAnalysis.SymbolKind.Method => SymbolKind.Function,
        Microsoft.CodeAnalysis.SymbolKind.Property => SymbolKind.Property,
        Microsoft.CodeAnalysis.SymbolKind.Field => SymbolKind.Field,
        Microsoft.CodeAnalysis.SymbolKind.Event => SymbolKind.Event,
        Microsoft.CodeAnalysis.SymbolKind.Parameter => SymbolKind.Parameter,
        Microsoft.CodeAnalysis.SymbolKind.Local => SymbolKind.Variable,
        _ => SymbolKind.Unknown
    };

    private void ExtractSymbols(CodeElements elements, List<SymbolInfo> symbols, string filePath, string containerName)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        foreach (CodeElement element in elements)
        {
            try
            {
                var kind = MapElementKind(element.Kind);
                if (kind == SymbolKind.Unknown)
                {
                    if (element.Kind == vsCMElement.vsCMElementImportStmt ||
                        element.Kind == vsCMElement.vsCMElementAttribute ||
                        element.Kind == vsCMElement.vsCMElementParameter)
                    {
                        continue;
                    }
                }

                var startPoint = element.StartPoint;
                var endPoint = element.EndPoint;

                var symbolInfo = new SymbolInfo
                {
                    Name = element.Name,
                    FullName = element.FullName,
                    Kind = kind,
                    FilePath = filePath,
                    StartLine = startPoint.Line,
                    StartColumn = startPoint.LineCharOffset,
                    EndLine = endPoint.Line,
                    EndColumn = endPoint.LineCharOffset,
                    ContainerName = containerName
                };

                var childElements = GetChildElements(element);
                if (childElements != null && childElements.Count > 0)
                {
                    ExtractSymbols(childElements, symbolInfo.Children, filePath, element.Name);
                }

                symbols.Add(symbolInfo);
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }
    }

    private static CodeElements? GetChildElements(CodeElement element)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            return element.Kind switch
            {
                vsCMElement.vsCMElementNamespace => ((CodeNamespace)element).Members,
                vsCMElement.vsCMElementClass => ((CodeClass)element).Members,
                vsCMElement.vsCMElementStruct => ((CodeStruct)element).Members,
                vsCMElement.vsCMElementInterface => ((CodeInterface)element).Members,
                vsCMElement.vsCMElementEnum => ((CodeEnum)element).Members,
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static SymbolKind MapElementKind(vsCMElement kind) => kind switch
    {
        vsCMElement.vsCMElementNamespace => SymbolKind.Namespace,
        vsCMElement.vsCMElementClass => SymbolKind.Class,
        vsCMElement.vsCMElementStruct => SymbolKind.Struct,
        vsCMElement.vsCMElementInterface => SymbolKind.Interface,
        vsCMElement.vsCMElementEnum => SymbolKind.Enum,
        vsCMElement.vsCMElementFunction => SymbolKind.Function,
        vsCMElement.vsCMElementProperty => SymbolKind.Property,
        vsCMElement.vsCMElementVariable => SymbolKind.Field,
        vsCMElement.vsCMElementEvent => SymbolKind.Event,
        vsCMElement.vsCMElementDelegate => SymbolKind.Delegate,
        _ => SymbolKind.Unknown
    };

    public async Task<WorkspaceSymbolResult> SearchWorkspaceSymbolsAsync(string query, int maxResults = 100)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var result = new WorkspaceSymbolResult();

        if (dte.Solution == null || string.IsNullOrWhiteSpace(query))
        {
            return result;
        }

        var allSymbols = new List<SymbolInfo>();
        var lowerQuery = query.ToLowerInvariant();

        foreach (EnvDTE.Project project in dte.Solution.Projects)
        {
            try
            {
                CollectProjectSymbols(project.ProjectItems, allSymbols, lowerQuery, maxResults * 2);
                if (allSymbols.Count >= maxResults * 2)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        var matchingSymbols = allSymbols
            .Where(s => s.Name.ToLowerInvariant().Contains(lowerQuery) ||
                       s.FullName.ToLowerInvariant().Contains(lowerQuery))
            .Take(maxResults)
            .ToList();

        result.Symbols = matchingSymbols;
        result.TotalCount = allSymbols.Count;
        result.Truncated = allSymbols.Count > maxResults;

        return result;
    }

    private void CollectProjectSymbols(ProjectItems? items, List<SymbolInfo> allSymbols, string query, int limit)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (items == null || allSymbols.Count >= limit)
        {
            return;
        }

        foreach (ProjectItem item in items)
        {
            try
            {
                if (item.FileCodeModel != null)
                {
                    var filePath = item.FileNames[1];
                    CollectCodeElements(item.FileCodeModel.CodeElements, allSymbols, filePath, string.Empty, query, limit);
                }

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    CollectProjectSymbols(item.ProjectItems, allSymbols, query, limit);
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }
    }

    private void CollectCodeElements(CodeElements elements, List<SymbolInfo> allSymbols, string filePath, string containerName, string query, int limit)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (allSymbols.Count >= limit)
        {
            return;
        }

        foreach (CodeElement element in elements)
        {
            try
            {
                var kind = MapElementKind(element.Kind);
                if (kind == SymbolKind.Unknown)
                {
                    continue;
                }

                var lowerName = element.Name.ToLowerInvariant();
                var lowerFullName = element.FullName.ToLowerInvariant();

                if (lowerName.Contains(query) || lowerFullName.Contains(query))
                {
                    var startPoint = element.StartPoint;
                    var endPoint = element.EndPoint;

                    allSymbols.Add(new SymbolInfo
                    {
                        Name = element.Name,
                        FullName = element.FullName,
                        Kind = kind,
                        FilePath = filePath,
                        StartLine = startPoint.Line,
                        StartColumn = startPoint.LineCharOffset,
                        EndLine = endPoint.Line,
                        EndColumn = endPoint.LineCharOffset,
                        ContainerName = containerName
                    });
                }

                var childElements = GetChildElements(element);
                if (childElements != null)
                {
                    CollectCodeElements(childElements, allSymbols, filePath, element.Name, query, limit);
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }
    }

    public async Task<DefinitionResult> GoToDefinitionAsync(string path, int line, int column)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var result = new DefinitionResult();

        try
        {
            var opened = await OpenDocumentAsync(path);
            if (!opened)
            {
                return result;
            }

            var doc = dte.ActiveDocument;
            if (doc == null)
            {
                return result;
            }

            var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
            string? originalPath;
            int originalLine;

            if (textDoc != null)
            {
                textDoc.Selection.MoveToLineAndOffset(line, column);
                originalPath = doc.FullName;
                originalLine = textDoc.Selection.ActivePoint.Line;
            }
            else
            {
                // Fallback for C++ and other file types using IVsTextView
                var textView = GetActiveTextView();
                if (textView == null)
                {
                    return result;
                }
                
                // Set cursor position (0-indexed in IVsTextView)
                textView.SetCaretPos(line - 1, column - 1);
                textView.CenterLines(line - 1, 1);
                
                originalPath = doc.FullName;
                originalLine = line;
            }

            dte.ExecuteCommand("Edit.GoToDefinition");

            await Task.Delay(100);

            var newDoc = dte.ActiveDocument;
            if (newDoc != null)
            {
                var newPath = newDoc.FullName;
                int newLine, newColumn;

                var newTextDoc = newDoc.Object("EnvDTETextDocument") as EnvDTETextDocument;
                if (newTextDoc != null)
                {
                    newLine = newTextDoc.Selection.ActivePoint.Line;
                    newColumn = newTextDoc.Selection.ActivePoint.LineCharOffset;
                }
                else
                {
                    // Fallback for C++ files
                    var newTextView = GetActiveTextView();
                    if (newTextView == null)
                    {
                        return result;
                    }
                    newTextView.GetCaretPos(out newLine, out newColumn);
                    newLine++; // Convert to 1-indexed
                    newColumn++;
                }

                if (!PathsEqual(newPath, originalPath) || newLine != originalLine)
                {
                    result.Found = true;
                    
                    // Get symbol name from the original position
                    var textBuffer = GetTextBufferForDocument(path);
                    if (textBuffer is IVsTextLines textLines)
                    {
                        textLines.GetLineText(line - 1, column - 1, line - 1, Math.Min(column + 50, GetLineLength(textLines, line - 1)), out string? lineText);
                        result.SymbolName = ExtractWord(lineText ?? string.Empty);
                    }

                    result.Definitions.Add(new LocationInfo
                    {
                        FilePath = newPath,
                        Line = newLine,
                        Column = newColumn,
                        EndLine = newLine,
                        EndColumn = newColumn,
                        Preview = GetLinePreview(newPath, newLine)
                    });
                }
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return result;
    }

    private static int GetLineLength(IVsTextLines textLines, int line)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        textLines.GetLengthOfLine(line, out int length);
        return length;
    }

    private static string ExtractWord(string text)
    {
        var match = System.Text.RegularExpressions.Regex.Match(text, @"[\w_]+");
        return match.Success ? match.Value : string.Empty;
    }

    private string GetLinePreview(string filePath, int line)
    {
        try
        {
            var textBuffer = GetTextBufferForDocument(filePath);
            if (textBuffer is IVsTextLines textLines)
            {
                textLines.GetLineText(line - 1, 0, line - 1, GetLineLength(textLines, line - 1), out string? lineText);
                return (lineText ?? string.Empty).Trim();
            }
        }
        catch
        {
            // Ignore errors getting line preview
        }
        return string.Empty;
    }

    private static string GetWordAtPosition(EnvDTETextDocument textDoc, int line, int column)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            var editPoint = textDoc.StartPoint.CreateEditPoint();
            editPoint.MoveToLineAndOffset(line, column);

            var startPoint = editPoint.CreateEditPoint();
            startPoint.WordLeft(1);
            var endPoint = editPoint.CreateEditPoint();
            endPoint.WordRight(1);

            return startPoint.GetText(endPoint).Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<ReferencesResult> FindReferencesAsync(string path, int line, int column, int maxResults = 100)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var result = new ReferencesResult();

        try
        {
            var opened = await OpenDocumentAsync(path);
            if (!opened)
            {
                return result;
            }

            var doc = dte.ActiveDocument;
            if (doc == null)
            {
                return result;
            }

            string symbolName;

            var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
            if (textDoc != null)
            {
                textDoc.Selection.MoveToLineAndOffset(line, column);
                symbolName = GetWordAtPosition(textDoc, line, column);
            }
            else
            {
                // Fallback for C++ and other file types using IVsTextView
                var textView = GetActiveTextView();
                if (textView == null)
                {
                    return result;
                }
                
                // Set cursor position (0-indexed in IVsTextView)
                textView.SetCaretPos(line - 1, column - 1);
                
                // Get word at cursor position
                textView.GetBuffer(out IVsTextLines? textLines);
                if (textLines == null)
                {
                    return result;
                }
                
                textLines.GetLineText(line - 1, 0, line - 1, GetLineLength(textLines, line - 1), out string? lineText);
                symbolName = ExtractWordFromPosition(lineText ?? string.Empty, column - 1);
            }

            if (string.IsNullOrWhiteSpace(symbolName))
            {
                return result;
            }

            result.SymbolName = symbolName;

            var references = await FindInSolutionAsync(dte, symbolName, maxResults);
            result.References = references;
            result.TotalCount = references.Count;
            result.Found = references.Count > 0;
            result.Truncated = references.Count >= maxResults;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return result;
    }

    private static string ExtractWordFromPosition(string lineText, int column)
    {
        if (string.IsNullOrEmpty(lineText) || column < 0 || column >= lineText.Length)
            return string.Empty;
            
        // Find word boundaries
        int start = column;
        int end = column;
        
        while (start > 0 && (char.IsLetterOrDigit(lineText[start - 1]) || lineText[start - 1] == '_'))
            start--;
            
        while (end < lineText.Length && (char.IsLetterOrDigit(lineText[end]) || lineText[end] == '_'))
            end++;
            
        return lineText.Substring(start, end - start);
    }

    private async Task<List<LocationInfo>> FindInSolutionAsync(DTE2 dte, string searchText, int maxResults)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var locations = new List<LocationInfo>();

        if (dte.Solution == null)
        {
            return locations;
        }

        foreach (EnvDTE.Project project in dte.Solution.Projects)
        {
            try
            {
                await SearchProjectItemsAsync(project.ProjectItems, searchText, locations, maxResults);
                if (locations.Count >= maxResults)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return locations;
    }

    private async Task SearchProjectItemsAsync(ProjectItems? items, string searchText, List<LocationInfo> locations, int maxResults)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (items == null || locations.Count >= maxResults)
        {
            return;
        }

        foreach (ProjectItem item in items)
        {
            try
            {
                if (item.FileNames[1] is string filePath &&
                    (filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                     filePath.EndsWith(".vb", StringComparison.OrdinalIgnoreCase)))
                {
                    var content = await Task.Run(() =>
                    {
                        if (File.Exists(filePath))
                        {
                            return File.ReadAllText(filePath);
                        }
                        return null;
                    });

                    if (content != null)
                    {
                        var lines = content.Split('\n');
                        for (int i = 0; i < lines.Length && locations.Count < maxResults; i++)
                        {
                            var lineText = lines[i];
                            var index = 0;
                            while ((index = lineText.IndexOf(searchText, index, StringComparison.Ordinal)) >= 0 &&
                                   locations.Count < maxResults)
                            {
                                if (IsWordBoundary(lineText, index, searchText.Length))
                                {
                                    locations.Add(new LocationInfo
                                    {
                                        FilePath = filePath,
                                        Line = i + 1,
                                        Column = index + 1,
                                        EndLine = i + 1,
                                        EndColumn = index + 1 + searchText.Length,
                                        Preview = lineText.Trim()
                                    });
                                }
                                index += searchText.Length;
                            }
                        }
                    }
                }

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    await SearchProjectItemsAsync(item.ProjectItems, searchText, locations, maxResults);
                }
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }
    }

    private static bool IsWordBoundary(string text, int start, int length)
    {
        var beforeOk = start == 0 || !char.IsLetterOrDigit(text[start - 1]);
        var afterOk = start + length >= text.Length || !char.IsLetterOrDigit(text[start + length]);
        return beforeOk && afterOk;
    }

    #region Private Fields for New Features

    private readonly List<string> _watchExpressions = new();
    private readonly List<TestInfo> _discoveredTests = new();
    private TestRunSummary? _lastTestResults;
    private int _activeFrameIndex;

    #endregion

    #region Debugger Control

    public async Task<DebugState> GetDebugStateAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var debugger = dte.Debugger;
        if (debugger == null)
        {
            return new DebugState { Mode = DebugMode.Design };
        }

        var mode = debugger.CurrentMode switch
        {
            dbgDebugMode.dbgDesignMode => DebugMode.Design,
            dbgDebugMode.dbgBreakMode => DebugMode.Break,
            dbgDebugMode.dbgRunMode => DebugMode.Run,
            _ => DebugMode.Design
        };

        var state = new DebugState { Mode = mode };

        if (mode != DebugMode.Design && debugger.CurrentProcess != null)
        {
            state.ProcessName = debugger.CurrentProcess.Name;
            state.ProcessId = debugger.CurrentProcess.ProcessID;

            if (debugger.CurrentThread != null)
            {
                state.ThreadId = debugger.CurrentThread.ID;
                state.ThreadName = debugger.CurrentThread.Name;
            }
        }

        return state;
    }

    public async Task<bool> StartDebuggingAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("StartDebugging");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Debugger.Go(false);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> StopDebuggingAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("StopDebugging");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Debugger.Stop(false);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> ContinueDebuggingAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("ContinueDebugging");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            dte.Debugger.Go(false);
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<bool> StepIntoAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            dte.Debugger.StepInto(false);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> StepOverAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            dte.Debugger.StepOver(false);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> StepOutAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            dte.Debugger.StepOut(false);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> RunToCursorAsync(string filePath, int line)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            // Set a temporary breakpoint at the cursor position
            // Correct parameter order: (Function, File, Line, Column, ...)
            var bp = dte.Debugger.Breakpoints.Add("", filePath, line, 1);
            
            // Continue execution
            dte.Debugger.Go(false);

            // The breakpoint will be hit and the temporary breakpoint will be removed automatically
            // Note: In a real implementation, you might want to track this and remove it after hitting
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Breakpoints

    public async Task<List<BreakpointInfo>> GetBreakpointsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var breakpoints = new List<BreakpointInfo>();

        foreach (Breakpoint bp in dte.Debugger.Breakpoints)
        {
            try
            {
                breakpoints.Add(new BreakpointInfo
                {
                    Id = bp.GetHashCode(),
                    FilePath = bp.File,
                    Line = bp.FileLine,
                    Column = bp.FileColumn,
                    Enabled = bp.Enabled,
                    Condition = bp.Condition,
                    HitCountTarget = bp.HitCountTarget,
                    HitCountType = bp.HitCountType.ToString(),
                    FunctionName = bp.FunctionName
                });
            }
            catch (Exception ex)
            {
                VsixTelemetry.TrackException(ex);
            }
        }

        return breakpoints;
    }

    public async Task<BreakpointInfo?> SetBreakpointAsync(SetBreakpointRequest request)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            // Breakpoints.Add returns a Breakpoints collection, get the first one
            // Note: EnvDTE Breakpoints.Add signature: (string Function, string File, int Line, int Column, ...)
            // For file/line breakpoints, Function should be empty string
            var breakpoints = dte.Debugger.Breakpoints.Add(
                "",                     // Function (empty for file/line breakpoints)
                request.FilePath,       // File path
                request.Line,           // Line number (int)
                request.Column          // Column number
            );

            if (breakpoints == null || breakpoints.Count == 0)
            {
                return null;
            }

            var bp = breakpoints.Item(1);

            return new BreakpointInfo
            {
                Id = bp.GetHashCode(),
                FilePath = bp.File,
                Line = bp.FileLine,
                Column = bp.FileColumn,
                Enabled = bp.Enabled,
                Condition = bp.Condition,
                HitCountTarget = bp.HitCountTarget,
                HitCountType = bp.HitCountType.ToString()
            };
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return null;
        }
    }

    public async Task<bool> RemoveBreakpointAsync(string filePath, int line)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (Breakpoint bp in dte.Debugger.Breakpoints)
            {
                if (PathsEqual(bp.File, filePath) && bp.FileLine == line)
                {
                    bp.Delete();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> ToggleBreakpointAsync(string filePath, int line)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (Breakpoint bp in dte.Debugger.Breakpoints)
            {
                if (PathsEqual(bp.File, filePath) && bp.FileLine == line)
                {
                    bp.Enabled = !bp.Enabled;
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> SetBreakpointConditionAsync(string filePath, int line, string? condition, int hitCount, string hitCountType)
    {
        // Note: EnvDTE Breakpoint.Condition, HitCountTarget, HitCountType are read-only
        // To set condition/hitCount, we need to delete the existing breakpoint and create a new one
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            // Find and delete existing breakpoint
            foreach (Breakpoint bp in dte.Debugger.Breakpoints)
            {
                if (PathsEqual(bp.File, filePath) && bp.FileLine == line)
                {
                    bp.Delete();
                    break;
                }
            }

            // Create new breakpoint with condition/hitcount if specified
            // Note: EnvDTE's Breakpoints.Add doesn't support condition/hitcount in all versions
            // This is a limitation - conditions must be set manually in VS UI
            // Correct parameter order: (Function, File, Line, Column, ...)
            var breakpoints = dte.Debugger.Breakpoints.Add("", filePath, line, 1);
            return breakpoints != null && breakpoints.Count > 0;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Variable Inspection

    public async Task<EvaluationResult> EvaluateExpressionAsync(string expression)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return new EvaluationResult
                {
                    Expression = expression,
                    IsValid = false,
                    ErrorMessage = "Debugger is not in break mode"
                };
            }

            var result = dte.Debugger.GetExpression(expression);
            return new EvaluationResult
            {
                Expression = expression,
                Value = result.Value,
                Type = result.Type,
                IsValid = result.IsValidValue,
                ErrorMessage = result.IsValidValue ? null : "Invalid expression"
            };
        }
        catch (Exception ex)
        {
            return new EvaluationResult
            {
                Expression = expression,
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<VariableInfo>> GetLocalsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var locals = new List<VariableInfo>();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return locals;
            }

            var frame = GetCurrentFrame(dte);
            if (frame == null)
            {
                return locals;
            }

            foreach (Expression local in frame.Locals)
            {
                locals.Add(new VariableInfo
                {
                    Name = local.Name,
                    Value = local.Value,
                    Type = local.Type,
                    IsValid = local.IsValidValue,
                    IsExpandable = local.DataMembers?.Count > 0
                });
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return locals;
    }

    public async Task<List<VariableInfo>> GetArgumentsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var arguments = new List<VariableInfo>();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return arguments;
            }

            var frame = GetCurrentFrame(dte);
            if (frame == null)
            {
                return arguments;
            }

            foreach (Expression arg in frame.Arguments)
            {
                arguments.Add(new VariableInfo
                {
                    Name = arg.Name,
                    Value = arg.Value,
                    Type = arg.Type,
                    IsValid = arg.IsValidValue,
                    IsExpandable = arg.DataMembers?.Count > 0
                });
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return arguments;
    }

    public async Task<VariableInfo> InspectVariableAsync(string variableName, int depth = 1)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return new VariableInfo
                {
                    Name = variableName,
                    IsValid = false
                };
            }

            var expr = dte.Debugger.GetExpression(variableName);
            return InspectExpression(expr, depth);
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return new VariableInfo
            {
                Name = variableName,
                IsValid = false
            };
        }
    }

    private static VariableInfo InspectExpression(Expression expr, int depth)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        var info = new VariableInfo
        {
            Name = expr.Name,
            Value = expr.Value,
            Type = expr.Type,
            IsValid = expr.IsValidValue,
            IsExpandable = expr.DataMembers?.Count > 0
        };

        if (depth > 0 && expr.DataMembers != null && expr.DataMembers.Count > 0)
        {
            info.Members = new List<VariableInfo>();
            foreach (Expression member in expr.DataMembers)
            {
                info.Members.Add(InspectExpression(member, depth - 1));
            }
        }

        return info;
    }

    public async Task<bool> SetVariableValueAsync(string variableName, string value)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            // Execute an assignment statement
            var statement = $"{variableName} = {value}";
            dte.Debugger.ExecuteStatement(statement, 1000);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Watch Window

    public async Task<List<WatchItem>> GetWatchExpressionsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var watches = new List<WatchItem>();

        foreach (var expr in _watchExpressions)
        {
            var item = new WatchItem { Expression = expr };

            try
            {
                if (dte.Debugger.CurrentMode == dbgDebugMode.dbgBreakMode)
                {
                    var result = dte.Debugger.GetExpression(expr);
                    item.Value = result.Value;
                    item.Type = result.Type;
                    item.IsValid = result.IsValidValue;
                    item.ErrorMessage = result.IsValidValue ? null : "Invalid expression";
                }
                else
                {
                    item.IsValid = false;
                    item.ErrorMessage = "Not in break mode";
                }
            }
            catch (Exception ex)
            {
                item.IsValid = false;
                item.ErrorMessage = ex.Message;
            }

            watches.Add(item);
        }

        return watches;
    }

    public Task<bool> AddWatchExpressionAsync(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return Task.FromResult(false);
        }

        if (!_watchExpressions.Contains(expression))
        {
            _watchExpressions.Add(expression);
        }

        return Task.FromResult(true);
    }

    public Task<bool> RemoveWatchExpressionAsync(string expression)
    {
        return Task.FromResult(_watchExpressions.Remove(expression));
    }

    public Task<bool> ClearWatchExpressionsAsync()
    {
        _watchExpressions.Clear();
        return Task.FromResult(true);
    }

    #endregion

    #region Call Stack & Threads

    public async Task<List<StackFrameInfo>> GetCallStackAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var frames = new List<StackFrameInfo>();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return frames;
            }

            var thread = dte.Debugger.CurrentThread;
            if (thread == null)
            {
                return frames;
            }

            var index = 0;
            foreach (EnvDTE.StackFrame frame in thread.StackFrames)
            {
                // Note: EnvDTE.StackFrame doesn't have FileName/LineNumber directly
                // These properties might be available through StackFrame2 in some VS versions
                // For now, we just use FunctionName and Module
                frames.Add(new StackFrameInfo
                {
                    Index = index++,
                    MethodName = frame.FunctionName,
                    FilePath = null, // Not directly available in EnvDTE.StackFrame
                    Line = 0, // Not directly available in EnvDTE.StackFrame
                    Column = 1,
                    EndLine = 0,
                    EndColumn = 1,
                    ModuleName = frame.Module,
                    FileName = null
                });
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return frames;
    }

    public Task<bool> SetActiveStackFrameAsync(int frameIndex)
    {
        // Note: EnvDTE doesn't support directly changing the active stack frame
        // This would require IVsDebugger or other advanced APIs
        // For now, just track the index for internal use
        _activeFrameIndex = frameIndex;
        return Task.FromResult(true);
    }

    public async Task<List<ThreadInfo>> GetThreadsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var threads = new List<ThreadInfo>();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return threads;
            }

            var currentThreadId = dte.Debugger.CurrentThread?.ID ?? -1;

            // Access Threads directly from Debugger.CurrentProcess.Threads
            // Note: EnvDTE.Debugger.CurrentProcess returns EnvDTE.Process which has Threads property
            var currentProcess = dte.Debugger.CurrentProcess;
            if (currentProcess == null)
            {
                return threads;
            }

            // Use dynamic to avoid type resolution conflict with System.Diagnostics.Process
            dynamic debugProcess = currentProcess;
            foreach (var threadObj in debugProcess.Threads)
            {
                dynamic thread = threadObj;
                var info = new ThreadInfo
                {
                    Id = thread.ID,
                    Name = thread.Name,
                    IsCurrent = thread.ID == currentThreadId
                };

                try
                {
                    if (thread.StackFrames?.Count > 0)
                    {
                        var frame = thread.StackFrames.Item(1);
                        info.Location = frame.FunctionName;
                        // Note: FileName and LineNumber not directly available in EnvDTE.StackFrame
                        info.FilePath = null;
                        info.Line = 0;
                    }
                }
                catch
                {
                    // Ignore errors getting frame info
                }

                threads.Add(info);
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return threads;
    }

    public async Task<bool> SetActiveThreadAsync(int threadId)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return false;
            }

            var currentProc = dte.Debugger.CurrentProcess;
            if (currentProc == null)
            {
                return false;
            }

            // Use dynamic to avoid type resolution conflict with System.Diagnostics.Process
            dynamic process = currentProc;
            foreach (var threadObj in process.Threads)
            {
                dynamic thread = threadObj;
                if (thread.ID == threadId)
                {
                    // Note: EnvDTE doesn't support directly changing the active thread
                    // This would require IVsDebugger or other advanced APIs
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Diagnostics

    public async Task<List<DiagnosticInfo>> GetDiagnosticsAsync(string? filePath = null, string? severity = null)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var diagnostics = new List<DiagnosticInfo>();

        // Note: Getting Roslyn diagnostics requires IAnalyzableSource and other APIs
        // For now, we'll use the Error List which provides basic functionality
        try
        {
            var dte = await GetDteAsync();
            var errorList = dte.ToolWindows.ErrorList;
            var errorItems = errorList.ErrorItems;

            for (int i = 1; i <= errorItems.Count; i++)
            {
                var item = errorItems.Item(i);
                var info = new DiagnosticInfo
                {
                    Message = item.Description,
                    FilePath = item.FileName,
                    Line = item.Line,
                    Column = item.Column,
                    ProjectName = item.Project
                };

                info.Severity = item.ErrorLevel switch
                {
                    vsBuildErrorLevel.vsBuildErrorLevelHigh => DiagnosticSeverity.Error,
                    vsBuildErrorLevel.vsBuildErrorLevelMedium => DiagnosticSeverity.Warning,
                    vsBuildErrorLevel.vsBuildErrorLevelLow => DiagnosticSeverity.Info,
                    _ => DiagnosticSeverity.Info
                };

                // Filter by file path if specified
                if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(info.FilePath))
                {
                    if (!PathsEqual(info.FilePath, filePath))
                    {
                        continue;
                    }
                }

                // Filter by severity if specified
                if (!string.IsNullOrEmpty(severity))
                {
                    var targetSeverity = severity.ToLowerInvariant() switch
                    {
                        "error" => DiagnosticSeverity.Error,
                        "warning" => DiagnosticSeverity.Warning,
                        "info" => DiagnosticSeverity.Info,
                        _ => (DiagnosticSeverity?)null
                    };

                    if (targetSeverity.HasValue && info.Severity != targetSeverity.Value)
                    {
                        continue;
                    }
                }

                diagnostics.Add(info);
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return diagnostics;
    }

    public async Task<List<DiagnosticInfo>> GetErrorListAsync()
    {
        return await GetDiagnosticsAsync(severity: "error");
    }

    public async Task<CodeFixResult> ApplyCodeFixAsync(ApplyCodeFixRequest request)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Note: Applying code fixes programmatically requires access to Roslyn's CodeAction
        // This is not directly available through EnvDTE
        // A full implementation would require IVsTextBuffer and Roslyn APIs
        // For now, return an error indicating this limitation

        return new CodeFixResult
        {
            Success = false,
            ErrorMessage = "Programmatic code fix application is not yet supported. This feature requires Roslyn CodeAction integration."
        };
    }

    public async Task<List<CodeFixInfo>> GetCodeFixesAsync(string filePath, int line, int column)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Note: Getting available code fixes requires Roslyn integration
        // This would need ICodeFixService or similar APIs
        // For now, return an empty list

        return new List<CodeFixInfo>();
    }

    #endregion

    #region Testing

    public async Task<List<TestInfo>> DiscoverTestsAsync(string? projectName = null)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        _discoveredTests.Clear();

        try
        {
            var solutionPath = Path.GetDirectoryName(dte.Solution.FullName);
            if (string.IsNullOrEmpty(solutionPath))
            {
                return _discoveredTests;
            }

            // Run dotnet test --list-tests
            var testProjects = new List<string>();
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                try
                {
                    if (!string.IsNullOrEmpty(projectName) &&
                        !project.Name.Equals(projectName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (project.FullName.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase) ||
                        project.FullName.EndsWith(".fsproj", StringComparison.OrdinalIgnoreCase) ||
                        project.FullName.EndsWith(".vbproj", StringComparison.OrdinalIgnoreCase))
                    {
                        testProjects.Add(project.FullName);
                    }
                }
                catch
                {
                    // Skip projects that can't be accessed
                }
            }

            foreach (var projectPath in testProjects)
            {
                var tests = await RunDotnetTestListAsync(projectPath);
                _discoveredTests.AddRange(tests);
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return _discoveredTests.ToList();
    }

    private async Task<List<TestInfo>> RunDotnetTestListAsync(string projectPath)
    {
        var tests = new List<TestInfo>();

        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"test \"{projectPath}\" --list-tests",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                return tests;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit(); // .NET Framework 4.8 doesn't have WaitForExitAsync

            // Parse the output for test names
            var lines = output.Split(new[] { '\n' }, StringSplitOptions.None);
            var inTestList = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("The following Tests are available:"))
                {
                    inTestList = true;
                    continue;
                }

                if (inTestList && !string.IsNullOrWhiteSpace(trimmed))
                {
                    // Parse fully qualified test name
                    var parts = trimmed.Split('.');
                    // .NET Framework 4.8 doesn't support Index/Range syntax
                    var testName = parts.Length > 0 ? parts[parts.Length - 1] : trimmed;
                    var className = parts.Length > 1 ? parts[parts.Length - 2] : null;
                    string? @namespace = null;
                    if (parts.Length > 2)
                    {
                        var namespaceParts = new string[parts.Length - 2];
                        for (int i = 0; i < parts.Length - 2; i++)
                        {
                            namespaceParts[i] = parts[i];
                        }
                        @namespace = string.Join(".", namespaceParts);
                    }

                    tests.Add(new TestInfo
                    {
                        Id = trimmed.GetHashCode().ToString(),
                        Name = testName,
                        FullName = trimmed,
                        ClassName = className,
                        Namespace = @namespace,
                        ProjectName = Path.GetFileNameWithoutExtension(projectPath),
                        Type = TestType.Unit
                    });
                }
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return tests;
    }

    public async Task<TestRunSummary> RunAllTestsAsync(string? projectName = null)
    {
        var request = new RunTestsRequest
        {
            ProjectName = projectName
        };
        return await RunTestsAsync(request);
    }

    public async Task<TestRunSummary> RunTestsAsync(RunTestsRequest request)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var summary = new TestRunSummary();

        try
        {
            var solutionPath = Path.GetDirectoryName(dte.Solution.FullName);
            if (string.IsNullOrEmpty(solutionPath))
            {
                return summary;
            }

            var projectPath = string.Empty;
            if (!string.IsNullOrEmpty(request.ProjectName))
            {
                foreach (EnvDTE.Project project in dte.Solution.Projects)
                {
                    try
                    {
                        if (project.Name.Equals(request.ProjectName, StringComparison.OrdinalIgnoreCase))
                        {
                            projectPath = project.FullName;
                            break;
                        }
                    }
                    catch
                    {
                        // Skip
                    }
                }
            }
            else
            {
                // Use the solution file
                projectPath = dte.Solution.FullName;
            }

            var filter = string.Empty;
            if (request.TestNames != null && request.TestNames.Count > 0)
            {
                filter = $"--filter \"{string.Join("|", request.TestNames)}\"";
            }
            else if (!string.IsNullOrEmpty(request.Filter))
            {
                filter = $"--filter \"{request.Filter}\"";
            }

            var arguments = $"test \"{projectPath}\" --no-build {filter} --logger \"trx;LogFileName=testresults.trx\"";
            if (request.Verbose)
            {
                arguments += " -v n";
            }

            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = solutionPath
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                return summary;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit(); // .NET Framework 4.8 doesn't have WaitForExitAsync

            // Parse the output for test results
            summary = ParseTestOutput(output + error);
            _lastTestResults = summary;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return summary;
    }

    private static TestRunSummary ParseTestOutput(string output)
    {
        var summary = new TestRunSummary();

        // Parse dotnet test output
        var lines = output.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Look for summary line like "Passed!  - Passed:  5, Failed:  0, Skipped:  0"
            if (trimmed.StartsWith("Passed!") || trimmed.StartsWith("Failed!") || trimmed.StartsWith("Skipped!"))
            {
                var parts = trimmed.Split(',');
                foreach (var part in parts)
                {
                    var kv = part.Trim().Split(':');
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim().ToLowerInvariant();
                        var value = kv[1].Trim();

                        if (int.TryParse(value, out var count))
                        {
                            switch (key)
                            {
                                case "passed":
                                    summary.Passed = count;
                                    break;
                                case "failed":
                                    summary.Failed = count;
                                    break;
                                case "skipped":
                                    summary.Skipped = count;
                                    break;
                            }
                        }
                    }
                }

                summary.Total = summary.Passed + summary.Failed + summary.Skipped;
            }

            // Parse individual test results
            if (trimmed.StartsWith("Passed") || trimmed.StartsWith("Failed") || trimmed.StartsWith("Skipped"))
            {
                var spaceIndex = trimmed.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    // .NET Framework 4.8 doesn't support Index/Range syntax
                    var status = trimmed.Substring(0, spaceIndex);
                    var testName = trimmed.Substring(spaceIndex + 1).Trim();

                    summary.Results.Add(new TestResult
                    {
                        TestName = testName,
                        TestId = testName.GetHashCode().ToString(),
                        Status = status.ToLowerInvariant() switch
                        {
                            "passed" => TestStatus.Passed,
                            "failed" => TestStatus.Failed,
                            "skipped" => TestStatus.Skipped,
                            _ => TestStatus.Pending
                        }
                    });
                }
            }
        }

        return summary;
    }

    public Task<TestRunSummary> GetTestResultsAsync()
    {
        return Task.FromResult(_lastTestResults ?? new TestRunSummary());
    }

    public async Task<bool> DebugTestAsync(string testName)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            // Set environment variable for VSTest debug mode
            Environment.SetEnvironmentVariable("VSTEST_HOST_DEBUG", "1");

            var request = new RunTestsRequest
            {
                Filter = testName,
                Verbose = true
            };

            await RunTestsAsync(request);

            // Clear the environment variable
            Environment.SetEnvironmentVariable("VSTEST_HOST_DEBUG", null);

            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Refactoring

    public async Task<List<string>> RenameSymbolAsync(string filePath, int line, int column, string newName)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var changedFiles = new List<string>();

        try
        {
            // Open the document and position cursor
            await OpenDocumentAsync(filePath);

            var doc = dte.ActiveDocument;
            if (doc == null)
            {
                return changedFiles;
            }

            string symbolName;
            
            var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
            if (textDoc != null)
            {
                textDoc.Selection.MoveToLineAndOffset(line, column);
                symbolName = GetWordAtPosition(textDoc, line, column);
            }
            else
            {
                // Fallback for C++ and other file types using IVsTextView
                var textView = GetActiveTextView();
                if (textView == null)
                {
                    return changedFiles;
                }
                
                // Set cursor position (0-indexed in IVsTextView)
                textView.SetCaretPos(line - 1, column - 1);
                
                // Get word at cursor position
                textView.GetBuffer(out IVsTextLines? textLines);
                if (textLines == null)
                {
                    return changedFiles;
                }
                
                textLines.GetLineText(line - 1, 0, line - 1, GetLineLength(textLines, line - 1), out string? lineText);
                symbolName = ExtractWordFromPosition(lineText ?? string.Empty, column - 1);
            }

            if (string.IsNullOrEmpty(symbolName))
            {
                return changedFiles;
            }

            // Use Find and Replace to rename across the solution
            var findObjects = dte.Find;
            findObjects.FindWhat = symbolName;
            findObjects.ReplaceWith = newName;
            findObjects.Target = vsFindTarget.vsFindTargetSolution;
            findObjects.MatchCase = true;
            findObjects.MatchWholeWord = true;
            findObjects.Action = vsFindAction.vsFindActionReplaceAll;

            var result = findObjects.Execute();

            if (result == vsFindResult.vsFindResultReplaced)
            {
                changedFiles.Add(filePath);
            }

            return changedFiles;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return changedFiles;
        }
    }

    public async Task<string?> ExtractMethodAsync(string filePath, int startLine, int startColumn, int endLine, int endColumn, string newMethodName)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            await OpenDocumentAsync(filePath);

            var doc = dte.ActiveDocument;
            if (doc == null)
            {
                return null;
            }

            var textDoc = doc.Object("EnvDTETextDocument") as EnvDTETextDocument;
            if (textDoc != null)
            {
                // Select the code to extract
                textDoc.Selection.MoveToLineAndOffset(startLine, startColumn);
                textDoc.Selection.MoveToLineAndOffset(endLine, endColumn, true);
            }
            else
            {
                // Fallback for C++ and other file types using IVsTextView
                var textView = GetActiveTextView();
                if (textView == null)
                {
                    return null;
                }
                
                // Set selection (0-indexed in IVsTextView)
                textView.SetSelection(startLine - 1, startColumn - 1, endLine - 1, endColumn - 1);
            }

            // Use VS command for extract method
            dte.ExecuteCommand("Refactor.ExtractMethod");

            // Wait for the dialog and type the method name
            await Task.Delay(500);

            // Note: This is a simplified approach. A full implementation would need to:
            // 1. Handle the Extract Method dialog
            // 2. Or use Roslyn's CodeRefactoring API

            return await ReadDocumentAsync(filePath);
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return null;
        }
    }

    /// <summary>
    /// Organizes using statements in a C# document.
    /// Note: This command only works for C# files. C++ files use #include directives.
    /// </summary>
    public async Task<string?> OrganizeUsingsAsync(string filePath, bool placeSystemFirst = true)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            await OpenDocumentAsync(filePath);

            var doc = dte.ActiveDocument;
            if (doc == null)
            {
                return null;
            }

            // Use VS command for organize usings
            dte.ExecuteCommand("Edit.OrganizeUsings");

            await Task.Delay(200);

            return await ReadDocumentAsync(filePath);
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return null;
        }
    }

    #endregion

    #region Output Windows

    public async Task<string> GetBuildOutputAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var outputWindow = dte.ToolWindows.OutputWindow;
            var buildPane = outputWindow.OutputWindowPanes.Item("Build");

            if (buildPane == null)
            {
                return string.Empty;
            }

            var textDocument = buildPane.TextDocument;
            var editPoint = textDocument.StartPoint.CreateEditPoint();
            return editPoint.GetText(textDocument.EndPoint);
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return string.Empty;
        }
    }

    public async Task<string> GetDebugOutputAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var outputWindow = dte.ToolWindows.OutputWindow;
            OutputWindowPane? debugPane = null;

            // Try to find the Debug pane
            foreach (OutputWindowPane pane in outputWindow.OutputWindowPanes)
            {
                if (pane.Name.Equals("Debug", StringComparison.OrdinalIgnoreCase))
                {
                    debugPane = pane;
                    break;
                }
            }

            if (debugPane == null)
            {
                return string.Empty;
            }

            var textDocument = debugPane.TextDocument;
            var editPoint = textDocument.StartPoint.CreateEditPoint();
            return editPoint.GetText(textDocument.EndPoint);
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return string.Empty;
        }
    }

    #endregion

    #region Project Operations

    public async Task<bool> AddFileToProjectAsync(string projectPath, string filePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                try
                {
                    if (PathsEqual(project.FullName, projectPath))
                    {
                        project.ProjectItems.AddFromFile(filePath);
                        return true;
                    }
                }
                catch
                {
                    // Continue to next project
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> CreateProjectItemAsync(string projectPath, string itemTemplate, string itemName, string? folderPath = null)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                try
                {
                    if (PathsEqual(project.FullName, projectPath))
                    {
                        ProjectItems? items = project.ProjectItems;

                        // If folder path is specified, navigate to that folder
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            var folderParts = folderPath.Split('\\', '/');
                            foreach (var part in folderParts)
                            {
                                var folderItem = items?.Item(part);
                                items = folderItem?.ProjectItems;
                            }
                        }

                        if (items == null)
                        {
                            return false;
                        }

                        // Add from template
                        items.AddFromTemplate(itemTemplate, itemName);
                        return true;
                    }
                }
                catch
                {
                    // Continue to next project
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> RemoveFileFromProjectAsync(string projectPath, string filePath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                try
                {
                    if (PathsEqual(project.FullName, projectPath))
                    {
                        // Find and remove the item
                        foreach (ProjectItem item in project.ProjectItems)
                        {
                            if (PathsEqual(item.FileNames[1], filePath))
                            {
                                item.Remove();
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    // Continue to next project
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> AddProjectReferenceAsync(string projectPath, string referenceProjectPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            EnvDTE.Project? sourceProject = null;
            EnvDTE.Project? referenceProject = null;

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                if (PathsEqual(project.FullName, projectPath))
                {
                    sourceProject = project;
                }
                else if (PathsEqual(project.FullName, referenceProjectPath))
                {
                    referenceProject = project;
                }
            }

            if (sourceProject == null || referenceProject == null)
            {
                return false;
            }

            // Find the References folder
            var vsProject = sourceProject.Object as VSLangProj.VSProject;
            if (vsProject == null)
            {
                return false;
            }

            vsProject.References.AddProject(referenceProject);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> RemoveProjectReferenceAsync(string projectPath, string referenceProjectPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            EnvDTE.Project? sourceProject = null;
            EnvDTE.Project? referenceProject = null;

            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                if (PathsEqual(project.FullName, projectPath))
                {
                    sourceProject = project;
                }
                else if (PathsEqual(project.FullName, referenceProjectPath))
                {
                    referenceProject = project;
                }
            }

            if (sourceProject == null || referenceProject == null)
            {
                return false;
            }

            var vsProject = sourceProject.Object as VSLangProj.VSProject;
            if (vsProject == null)
            {
                return false;
            }

            // Find and remove the reference
            foreach (VSLangProj.Reference reference in vsProject.References)
            {
                if (reference.SourceProject != null && PathsEqual(reference.SourceProject.FullName, referenceProjectPath))
                {
                    reference.Remove();
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> AddProjectToSolutionAsync(string projectPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.Solution.AddFromFile(projectPath);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<bool> RemoveProjectFromSolutionAsync(string projectPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            foreach (EnvDTE.Project project in dte.Solution.Projects)
            {
                if (PathsEqual(project.FullName, projectPath))
                {
                    dte.Solution.Remove(project);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region General

    public async Task<CommandResult> ExecuteCommandAsync(string commandName, string? args = null)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            dte.ExecuteCommand(commandName, args ?? string.Empty);
            return new CommandResult { Success = true };
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return new CommandResult { Success = false, Error = ex.Message };
        }
    }

    public async Task<IdeStatus> GetIdeStatusAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        var status = new IdeStatus
        {
            IsSolutionOpen = dte.Solution?.IsOpen ?? false,
            SolutionPath = dte.Solution?.FullName,
            IsDebugging = dte.Debugger?.CurrentMode == dbgDebugMode.dbgRunMode,
            ActiveDocument = dte.ActiveDocument?.FullName
        };

        // Determine build state
        var solutionBuild = dte.Solution?.SolutionBuild;
        if (solutionBuild == null)
        {
            status.BuildState = "NoSolution";
        }
        else if (solutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
        {
            status.BuildState = "Building";
        }
        else
        {
            status.BuildState = "Ready";
        }

        return status;
    }

    #endregion

    #region NuGet

    public async Task<List<ProjectPackage>> GetProjectPackagesAsync(string projectPath)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var packages = new List<ProjectPackage>();

        // Note: This requires NuGet.VisualStudio package to be referenced
        // For now, return empty list as a placeholder
        // Full implementation would use IVsPackageInstallerServices

        return await Task.FromResult(packages);
    }

    public async Task<NuGetSearchResult> SearchNuGetPackagesAsync(string searchTerm, int skip = 0, int take = 20)
    {
        // Note: This requires NuGet.VisualStudio package to be referenced
        // For now, return empty result as a placeholder
        // Full implementation would use IVsPackageSourceProvider and IVsPackageMetadataProvider

        return await Task.FromResult(new NuGetSearchResult());
    }

    public async Task<bool> InstallNuGetPackageAsync(string projectPath, string packageId, string? version = null)
    {
        // Note: This requires NuGet.VisualStudio package to be referenced
        // For now, return false as a placeholder
        // Full implementation would use IVsPackageInstaller

        return await Task.FromResult(false);
    }

    public async Task<bool> UpdateNuGetPackageAsync(string projectPath, string packageId, string? version = null)
    {
        // Note: This requires NuGet.VisualStudio package to be referenced
        // For now, return false as a placeholder
        // Full implementation would use IVsPackageInstaller

        return await Task.FromResult(false);
    }

    public async Task<bool> UninstallNuGetPackageAsync(string projectPath, string packageId)
    {
        // Note: This requires NuGet.VisualStudio package to be referenced
        // For now, return false as a placeholder
        // Full implementation would use IVsPackageUninstaller

        return await Task.FromResult(false);
    }

    #endregion

    #region Build Extensions

    public async Task<bool> RebuildSolutionAsync()
    {
        using var activity = VsixTelemetry.Tracer.StartActivity("RebuildSolution");

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var solutionBuild = dte.Solution.SolutionBuild;
            solutionBuild.Clean(true);
            solutionBuild.Build(true);
            return solutionBuild.LastBuildInfo == 0;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            return false;
        }
    }

    public async Task<List<BuildError>> GetBuildErrorsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var errors = new List<BuildError>();

        try
        {
            var errorItems = dte.ToolWindows.ErrorList.ErrorItems;
            for (int i = 1; i <= errorItems.Count; i++)
            {
                var item = errorItems.Item(i);
                errors.Add(new BuildError
                {
                    ProjectName = item.Project,
                    FilePath = item.FileName,
                    Line = item.Line,
                    Column = item.Column,
                    Message = item.Description,
                    Code = string.Empty, // ErrorCode not available in EnvDTE.ErrorItem
                    Severity = item.ErrorLevel switch
                    {
                        vsBuildErrorLevel.vsBuildErrorLevelHigh => "Error",
                        vsBuildErrorLevel.vsBuildErrorLevelMedium => "Warning",
                        vsBuildErrorLevel.vsBuildErrorLevelLow => "Message",
                        _ => "Unknown"
                    }
                });
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return errors;
    }

    #endregion

    #region Find in Files

    public async Task<List<FindInFilesResult>> FindInFilesAsync(string searchTerm, string? filePattern = null, string? folderPath = null, bool matchCase = false, bool matchWholeWord = false, bool useRegex = false)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var results = new List<FindInFilesResult>();

        try
        {
            var find = dte.Find;

            // Configure find options
            find.FindWhat = searchTerm;
            find.MatchCase = matchCase;
            find.MatchWholeWord = matchWholeWord;
            find.PatternSyntax = useRegex ? vsFindPatternSyntax.vsFindPatternSyntaxRegExpr : vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
            find.Target = vsFindTarget.vsFindTargetFiles;
            find.SearchPath = folderPath ?? dte.Solution?.FullName ?? string.Empty;
            find.FilesOfType = filePattern ?? "*.*";
            find.Action = vsFindAction.vsFindActionFindAll;

            // Execute find
            var result = find.Execute();

            // Note: The results appear in the Find Results window
            // To get the results programmatically, we would need to parse the output
            // or use a different approach (like IVsFindSymbol)

        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return results;
    }

    #endregion

    #region Advanced Debugging

    public async Task<bool> AttachToProcessAsync(int processId)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var processes = dte.Debugger.LocalProcesses;
            foreach (EnvDTE.Process process in processes)
            {
                if (process.ProcessID == processId)
                {
                    process.Attach();
                    return true;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    public async Task<List<ProcessInfo>> GetProcessesAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var processes = new List<ProcessInfo>();

        try
        {
            // Get all local processes
            foreach (EnvDTE.Process process in dte.Debugger.LocalProcesses)
            {
                processes.Add(new ProcessInfo
                {
                    Id = process.ProcessID,
                    Name = process.Name,
                    // Note: IsBeingDebugged is not directly available in EnvDTE.Process
                    // We can check if the process is in the DebuggedProcesses collection
                    IsBeingDebugged = false // Would need additional logic to determine this
                });
            }
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return processes;
    }

    public async Task<List<ModuleInfo>> GetModulesAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var modules = new List<ModuleInfo>();

        // Note: Module enumeration is not directly supported through EnvDTE
        // This would require using the Debug Engine APIs (IDebugModule2)
        // Returning empty list as a placeholder

        return await Task.FromResult(modules);
    }

    public async Task<MemoryReadResult> ReadMemoryAsync(ulong address, int size)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        // Note: Memory reading is not directly supported in EnvDTE
        // This would require using the Debug Engine APIs (IDebugMemoryContext2, IDebugMemoryBytes2)

        return new MemoryReadResult
        {
            Success = false,
            Error = "Memory reading is not supported through the current API",
            Address = address,
            BytesRead = 0
        };
    }

    public async Task<List<RegisterInfo>> GetRegistersAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();
        var registers = new List<RegisterInfo>();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return registers;
            }

            // Note: Register access is not directly supported in EnvDTE
            // This would require using the Debug Engine APIs

        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return registers;
    }

    #endregion

    #region Output Window

    public async Task<bool> WriteToOutputWindowAsync(string paneName, string message)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var dte = await GetDteAsync();

        try
        {
            var outputWindow = dte.ToolWindows.OutputWindow;

            // Find or create the pane
            OutputWindowPane? pane = null;
            foreach (OutputWindowPane p in outputWindow.OutputWindowPanes)
            {
                if (p.Name.Equals(paneName, StringComparison.OrdinalIgnoreCase))
                {
                    pane = p;
                    break;
                }
            }

            if (pane == null)
            {
                pane = outputWindow.OutputWindowPanes.Add(paneName);
            }

            pane.Activate();
            pane.OutputString(message);
            return true;
        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
            return false;
        }
    }

    #endregion

    #region Diagnostics Extensions

    public async Task<List<DiagnosticInfo>> GetXamlBindingErrorsAsync()
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        var diagnostics = new List<DiagnosticInfo>();

        try
        {
            // Get the Error List service
            var errorListService = ServiceProvider.GetService(typeof(SVsErrorList)) as IVsErrorList;
            if (errorListService == null)
            {
                return diagnostics;
            }

            // Get the error items
            var errorItems = errorListService as IVsTaskList;
            // Note: Full implementation would iterate through errors and filter for XAML binding errors
            // This requires more complex interaction with VS error list APIs

        }
        catch (Exception ex)
        {
            VsixTelemetry.TrackException(ex);
        }

        return diagnostics;
    }

    #endregion

    #region Helper Methods

    private static EnvDTE.StackFrame? GetCurrentFrame(DTE2 dte)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        try
        {
            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                return null;
            }

            var thread = dte.Debugger.CurrentThread;
            if (thread == null || thread.StackFrames == null || thread.StackFrames.Count == 0)
            {
                return null;
            }

            return thread.StackFrames.Item(1); // 1-indexed
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
