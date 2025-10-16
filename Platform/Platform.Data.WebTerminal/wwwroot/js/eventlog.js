// Event Log JavaScript for WebTerminal
// Provides real-time event monitoring and display

(function() {
    'use strict';

    var eventLogContainer = null;
    var autoScroll = true;

    $(document).ready(function() {
        eventLogContainer = $('#event-log-container');

        // Auto-scroll toggle
        $(window).on('scroll', function() {
            var scrollTop = $(window).scrollTop();
            var docHeight = $(document).height();
            var winHeight = $(window).height();

            // Disable auto-scroll if user scrolls up
            if (scrollTop + winHeight < docHeight - 50) {
                autoScroll = false;
            } else {
                autoScroll = true;
            }
        });

        // Simulate periodic event updates (in real implementation, this would connect to a backend)
        // For now, we'll just highlight existing events
        highlightRecentEvents();
    });

    function highlightRecentEvents() {
        var items = $('.event-item:not(.event-header)');
        items.each(function(index) {
            var item = $(this);
            setTimeout(function() {
                item.css('background', '#002200');
                setTimeout(function() {
                    item.css('background', '');
                }, 500);
            }, index * 100);
        });
    }

    function scrollToBottom() {
        if (autoScroll && eventLogContainer) {
            eventLogContainer.scrollTop(eventLogContainer[0].scrollHeight);
        }
    }

    // Export functions for external use
    window.EventLog = {
        scrollToBottom: scrollToBottom,
        highlightRecentEvents: highlightRecentEvents
    };
})();
