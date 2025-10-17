using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace PatternDiscoveryExtension
{
    /// <summary>
    /// Main package class for Pattern Discovery Extension.
    /// Monitors code changes and detects repetitive editing patterns based on AST analysis.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class PatternDiscoveryPackage : AsyncPackage
    {
        public const string PackageGuidString = "b93f8a5c-4e3f-4d7b-9c1e-8a2f3b4c5d6e";

        private PatternMonitor? _patternMonitor;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            try
            {
                _patternMonitor = new PatternMonitor(this);
                await _patternMonitor.InitializeAsync();
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync("Pattern Discovery Extension",
                    $"Failed to initialize: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _patternMonitor?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
