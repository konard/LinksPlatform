using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Shell;
using PatternDiscoveryExtension.Models;

namespace PatternDiscoveryExtension
{
    /// <summary>
    /// Handles user interface for pattern suggestions.
    /// Notifies users when well-formed patterns are detected and offers to repeat them.
    /// </summary>
    public class PatternSuggestionUI
    {
        private DateTime _lastSuggestionTime = DateTime.MinValue;
        private readonly TimeSpan _suggestionCooldown = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Shows a suggestion to the user about detected patterns.
        /// </summary>
        public async Task ShowPatternSuggestionAsync(List<Pattern> patterns, Document document)
        {
            // Avoid spamming the user with suggestions
            if (DateTime.UtcNow - _lastSuggestionTime < _suggestionCooldown)
                return;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                var mostConfident = patterns.OrderByDescending(p => p.Confidence).First();

                var message = $"Pattern detected: {mostConfident.Description}\n\n" +
                              $"This pattern has occurred {mostConfident.Occurrences} times " +
                              $"with {mostConfident.Confidence:P0} confidence.\n\n" +
                              $"Would you like to repeat this pattern?";

                var result = await VS.MessageBox.ShowAsync(
                    "Pattern Discovery",
                    message,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_YESNO);

                if (result == Microsoft.VisualStudio.VSConstants.MessageBoxResult.IDYES)
                {
                    await ApplyPatternAsync(mostConfident, document);
                }

                _lastSuggestionTime = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync("Pattern Discovery",
                    $"Error showing suggestion: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies a detected pattern to the document.
        /// </summary>
        private async Task ApplyPatternAsync(Pattern pattern, Document document)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                // This is a placeholder for the actual pattern application logic
                // In a full implementation, this would:
                // 1. Determine the current cursor position
                // 2. Apply the pattern changes at that position
                // 3. Update the document with the new code

                var message = $"Pattern repetition would apply {pattern.Changes.Count} changes.\n\n" +
                              "Note: Automatic pattern application is not yet fully implemented.\n" +
                              "This feature will be enhanced in future versions.";

                await VS.MessageBox.ShowAsync(
                    "Pattern Repetition",
                    message,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO,
                    Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
            }
            catch (Exception ex)
            {
                await VS.MessageBox.ShowErrorAsync("Pattern Discovery",
                    $"Error applying pattern: {ex.Message}");
            }
        }

        /// <summary>
        /// Shows information about all detected patterns.
        /// </summary>
        public async Task ShowPatternSummaryAsync(List<Pattern> patterns)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var wellFormed = patterns.Where(p => p.IsWellFormed).ToList();
            var message = wellFormed.Count > 0
                ? $"Detected {wellFormed.Count} well-formed pattern(s):\n\n" +
                  string.Join("\n", wellFormed.Select(p => $"• {p}"))
                : "No well-formed patterns detected yet.\n\n" +
                  "Keep coding! The extension will notify you when it detects repetitive patterns.";

            await VS.MessageBox.ShowAsync(
                "Pattern Discovery Summary",
                message,
                Microsoft.VisualStudio.Shell.Interop.OLEMSGICON.OLEMSGICON_INFO,
                Microsoft.VisualStudio.Shell.Interop.OLEMSGBUTTON.OLEMSGBUTTON_OK);
        }
    }
}
