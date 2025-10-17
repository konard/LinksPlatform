using System;
using System.IO;

namespace Platform.BlackBox
{
    /// <summary>
    /// Main facade for the Black Box activity recording system.
    /// Provides easy-to-use interface for recording activity and generating bug reports.
    /// </summary>
    public class BlackBox
    {
        private static readonly Lazy<BlackBox> _instance = new Lazy<BlackBox>(() => new BlackBox());

        /// <summary>
        /// Gets the singleton instance of the BlackBox.
        /// </summary>
        public static BlackBox Instance => _instance.Value;

        private readonly IActivityRecorder _recorder;
        private readonly IBugReportGenerator _reportGenerator;
        private bool _isEnabled = true;

        /// <summary>
        /// Creates a new BlackBox instance with default settings.
        /// </summary>
        public BlackBox()
            : this(new CircularBufferActivityRecorder(), new BugReportGenerator())
        {
        }

        /// <summary>
        /// Creates a new BlackBox instance with custom recorder and generator.
        /// </summary>
        public BlackBox(IActivityRecorder recorder, IBugReportGenerator reportGenerator)
        {
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
            _reportGenerator = reportGenerator ?? throw new ArgumentNullException(nameof(reportGenerator));
        }

        /// <summary>
        /// Gets or sets whether recording is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        /// <summary>
        /// Records a simple activity event.
        /// </summary>
        public void Record(string eventType, string source, string description, string severity = "Info")
        {
            if (!_isEnabled) return;
            _recorder.Record(eventType, source, description, severity);
        }

        /// <summary>
        /// Records an exception.
        /// </summary>
        public void RecordException(Exception exception, string source, string additionalContext = null)
        {
            if (!_isEnabled) return;
            _recorder.RecordException(exception, source, additionalContext);
        }

        /// <summary>
        /// Records a custom activity event.
        /// </summary>
        public void Record(ActivityEvent activityEvent)
        {
            if (!_isEnabled) return;
            _recorder.Record(activityEvent);
        }

        /// <summary>
        /// Generates a bug report from the current activity log.
        /// </summary>
        public BugReport GenerateBugReport(string title = null, string description = null)
        {
            var events = _recorder.GetEvents();
            return _reportGenerator.Generate(events, title, description);
        }

        /// <summary>
        /// Generates and exports a bug report to a file.
        /// </summary>
        public string GenerateAndSaveBugReport(string filePath = null, BugReportFormat format = BugReportFormat.Markdown, string title = null, string description = null)
        {
            var report = GenerateBugReport(title, description);
            var content = _reportGenerator.Export(report, format);

            if (filePath == null)
            {
                var extension = format switch
                {
                    BugReportFormat.Markdown => "md",
                    BugReportFormat.Json => "json",
                    BugReportFormat.PlainText => "txt",
                    _ => "txt"
                };
                filePath = $"bug-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.{extension}";
            }

            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// Gets the current count of recorded events.
        /// </summary>
        public int EventCount => _recorder.Count;

        /// <summary>
        /// Clears all recorded events.
        /// </summary>
        public void Clear()
        {
            _recorder.Clear();
        }

        /// <summary>
        /// Helper method to execute an action and automatically record exceptions.
        /// </summary>
        public void Execute(Action action, string source)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                RecordException(ex, source);
                throw;
            }
        }

        /// <summary>
        /// Helper method to execute a function and automatically record exceptions.
        /// </summary>
        public T Execute<T>(Func<T> func, string source)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                RecordException(ex, source);
                throw;
            }
        }
    }
}
