using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using PatternDiscoveryExtension.Models;
using Task = System.Threading.Tasks.Task;

namespace PatternDiscoveryExtension
{
    /// <summary>
    /// Monitors code changes in real-time and coordinates pattern detection.
    /// </summary>
    public class PatternMonitor : IDisposable
    {
        private readonly AsyncPackage _package;
        private readonly ASTAnalyzer _analyzer;
        private readonly PatternDetector _detector;
        private readonly PatternSuggestionUI _ui;
        private VisualStudioWorkspace? _workspace;
        private readonly Dictionary<DocumentId, Document> _documentSnapshots = new();

        public PatternMonitor(AsyncPackage package)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            _analyzer = new ASTAnalyzer();
            _detector = new PatternDetector();
            _ui = new PatternSuggestionUI();
        }

        public async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                // Get Visual Studio workspace
                _workspace = await VS.GetServiceAsync<SVsExtensibility, VisualStudioWorkspace>();

                if (_workspace != null)
                {
                    // Subscribe to workspace changes
                    _workspace.WorkspaceChanged += OnWorkspaceChanged;

                    // Initialize document snapshots for all open documents
                    foreach (var project in _workspace.CurrentSolution.Projects)
                    {
                        foreach (var document in project.Documents)
                        {
                            _documentSnapshots[document.Id] = document;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync("Pattern Monitor",
                    $"Failed to initialize workspace monitoring: {ex.Message}");
            }
        }

        private void OnWorkspaceChanged(object? sender, WorkspaceChangeEventArgs e)
        {
            try
            {
                // Only process document changes
                if (e.Kind != WorkspaceChangeKind.DocumentChanged)
                    return;

                var documentId = e.DocumentId;
                if (documentId == null)
                    return;

                var newDocument = e.NewSolution.GetDocument(documentId);
                if (newDocument == null)
                    return;

                // Only analyze C# files
                if (newDocument.FilePath == null || !newDocument.FilePath.EndsWith(".cs"))
                    return;

                // Get old version of document
                if (!_documentSnapshots.TryGetValue(documentId, out var oldDocument))
                {
                    // First time seeing this document, just store it
                    _documentSnapshots[documentId] = newDocument;
                    return;
                }

                // Analyze changes
                _ = Task.Run(async () => await AnalyzeDocumentChangesAsync(oldDocument, newDocument));

                // Update snapshot
                _documentSnapshots[documentId] = newDocument;
            }
            catch (Exception)
            {
                // Silently ignore errors to not disrupt user's work
            }
        }

        private async Task AnalyzeDocumentChangesAsync(Document oldDocument, Document newDocument)
        {
            try
            {
                var oldText = await oldDocument.GetTextAsync();
                var newText = await newDocument.GetTextAsync();

                var textChanges = newText.GetTextChanges(oldText).ToList();
                if (!textChanges.Any())
                    return;

                // For each text change, analyze the AST changes
                foreach (var textChange in textChanges)
                {
                    var changeRange = new TextChangeRange(textChange.Span, textChange.NewText?.Length ?? 0);
                    var astChanges = _analyzer.AnalyzeChanges(oldDocument, newDocument, changeRange);

                    // Feed changes to pattern detector
                    foreach (var astChange in astChanges)
                    {
                        var wellFormedPatterns = _detector.AddChangeAndDetectPatterns(astChange);

                        // Show suggestions for well-formed patterns
                        if (wellFormedPatterns.Count > 0)
                        {
                            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                            await _ui.ShowPatternSuggestionAsync(wellFormedPatterns, newDocument);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently ignore to not disrupt the user
            }
        }

        public void Dispose()
        {
            if (_workspace != null)
            {
                _workspace.WorkspaceChanged -= OnWorkspaceChanged;
            }
            _documentSnapshots.Clear();
        }
    }
}
