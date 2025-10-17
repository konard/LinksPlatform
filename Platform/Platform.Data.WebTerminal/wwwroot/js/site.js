// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// View mode toggle functionality
(function() {
    // Initialize view mode from localStorage or default to 'read'
    var viewMode = localStorage.getItem('viewMode') || 'read';

    function applyViewMode(mode) {
        var body = document.body;
        var icon = document.getElementById('view-mode-icon');

        if (mode === 'read') {
            body.classList.remove('write-mode');
            body.classList.add('read-mode');
            if (icon) icon.textContent = '👁';
        } else {
            body.classList.remove('read-mode');
            body.classList.add('write-mode');
            if (icon) icon.textContent = '✎';
        }

        localStorage.setItem('viewMode', mode);
    }

    // Apply initial mode on page load
    document.addEventListener('DOMContentLoaded', function() {
        applyViewMode(viewMode);

        var toggleButton = document.getElementById('view-mode-toggle');
        if (toggleButton) {
            toggleButton.addEventListener('click', function() {
                viewMode = (viewMode === 'read') ? 'write' : 'read';
                applyViewMode(viewMode);
            });
        }
    });
})();
