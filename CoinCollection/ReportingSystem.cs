using System.IO;
using System.Text;
using System.Windows;

namespace CoinCollection
{
    /// <summary>
    /// The serverity of the report
    /// </summary>
    public enum ReportSeverity
    {
        None,
        Info,
        Warning, 
        Error
    }

    /// <summary>
    /// Report logging system
    /// </summary>
    public abstract class ReportingSystem
    {
        public string ReportFolderName { get; private set; }
        public string ReportFolderPath { get; private set; }

        public bool IsEnabled;

        protected readonly string[] _reportFrequency;

        protected string? _currentReport;

        protected string? _currentFrequency;

        protected DateTime _lastReportDate;

        private readonly EventTimer _eventTimer;

        /// <summary>
        /// Logs reports
        /// </summary>
        /// <param name="reportFolderName">Name of the folder that the reports are in</param>
        /// <param name="isEnabled">Should reporting be enabled</param>
        /// <param name="reportFrequency">The report frequencies</param>
        public ReportingSystem(string reportFolderName = "", bool isEnabled = false, params string[] reportFrequency) 
        { 
            IsEnabled = isEnabled;

            if(string.IsNullOrEmpty(reportFolderName))
            {
                ReportFolderName = "Reports";
            }
            else
            {
                ReportFolderName = reportFolderName;
            }

            ReportFolderPath = Path.Combine(Directory.GetCurrentDirectory(), ReportFolderName);

            Directory.CreateDirectory(ReportFolderPath);

            _reportFrequency = reportFrequency;

            UpdateFrequency();

            ReportSetUp();
            
            _eventTimer = new EventTimer(1, EventTimerType.hours, true, new EventTimerAction(() => { if (DateTime.Now.Hour == 0) { UpdateReportFrequency(); } }));
        }

        /// <summary>
        /// Shows a message box and reports the text in the report
        /// </summary>
        /// <param name="messageBoxText">A string that specifies the text to display.</param>
        /// <param name="caption">A string that specifies the title bar caption to display.</param>
        /// <param name="reportSeverity">The severity of the report</param>
        /// <param name="button">A MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="options">A MessageBoxOptions value object that specifies the options.</param>
        /// <returns>A MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public MessageBoxResult ShowMessage(string messageBoxText, string caption = "", ReportSeverity reportSeverity = ReportSeverity.None, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            AddReport(messageBoxText, reportSeverity);

            return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        }

        /// <summary>
        /// Shows a message box and reports the text in the report
        /// </summary>
        /// <param name="exception">Exception message</param>
        /// <param name="caption">A string that specifies the title bar caption to display.</param>
        /// <param name="reportSeverity">The severity of the report</param>
        /// <param name="button">A MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="options">A MessageBoxOptions value object that specifies the options.</param>
        /// <returns>A MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public MessageBoxResult ShowMessage(Exception exception, string caption = "", ReportSeverity reportSeverity = ReportSeverity.None, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            return ShowMessage(exception.Message, caption, reportSeverity, button, icon, defaultResult, options);
        }

        /// <summary>
        /// Shows a message box and reports the text in the report
        /// </summary>
        /// <param name="owner">A Window that represents the owner window of the message box.</param>
        /// <param name="messageBoxText">A string that specifies the text to display.</param>
        /// <param name="caption">A string that specifies the title bar caption to display.</param>
        /// <param name="reportSeverity">The severity of the report</param>
        /// <param name="button">A MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="options">A MessageBoxOptions value object that specifies the options.</param>
        /// <returns>A MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public MessageBoxResult ShowMessage(Window owner, string messageBoxText, string caption = "", ReportSeverity reportSeverity = ReportSeverity.None, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            AddReport(messageBoxText, reportSeverity);

            return MessageBox.Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
        }

        /// <summary>
        /// Shows a message box and reports the text in the report
        /// </summary>
        /// <param name="owner">A Window that represents the owner window of the message box.</param>
        /// <param name="exception">Exception message</param>
        /// <param name="caption">A string that specifies the title bar caption to display.</param>
        /// <param name="reportSeverity">The severity of the report</param>
        /// <param name="button">A MessageBoxButton value that specifies which button or buttons to display.</param>
        /// <param name="icon">A MessageBoxImage value that specifies the icon to display.</param>
        /// <param name="defaultResult">A MessageBoxResult value that specifies the default result of the message box.</param>
        /// <param name="options">A MessageBoxOptions value object that specifies the options.</param>
        /// <returns>A MessageBoxResult value that specifies which message box button is clicked by the user.</returns>
        public MessageBoxResult ShowMessage(Window owner, Exception exception, string caption = "", ReportSeverity reportSeverity = ReportSeverity.None, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            return ShowMessage(owner, exception.Message, caption, reportSeverity, button, icon, defaultResult, options);
        }

        /// <summary>
        /// Adds a report to the currently selected report
        /// </summary>
        /// <param name="info">Information about the report</param>
        /// <param name="severity">The severity of the report</param>
        public virtual void AddReport(string info, ReportSeverity severity = ReportSeverity.None)
        {
            if(!IsEnabled)
            {
                return;
            }

            StringBuilder sb = new ($"[{DateTime.Now.ToShortTimeString()}] {info}");

            if (severity != ReportSeverity.None)
            {
                switch (severity)
                {
                    case ReportSeverity.Info:
                        sb.Append('.');
                        break;
                    case ReportSeverity.Warning:
                        sb.Append('!');
                        break;
                    case ReportSeverity.Error:
                        sb.Append("!!!");
                        break;
                    default:
                        throw new NotImplementedException($"{severity} is not used!!!");
                }
            }

            if (string.IsNullOrEmpty(_currentReport))
            {
                ReportSetUp();
            }

            using StreamWriter sw = new(_currentReport!, true);
            sw.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Updates what kind of report to add reports to when the report frequency changes
        /// </summary>
        public virtual void UpdateReportFrequency()
        {
            AddReport("----------Frequency Changed----------");
            AddReport("----------File Closed----------");
            ReportSetUp();
        }

        /// <summary>
        /// Updates the current frequency
        /// </summary>
        protected abstract void UpdateFrequency();

        /// <summary>
        /// Sets up the report
        /// </summary>
        protected abstract void ReportSetUp();

        /// <summary>
        /// Create new report for the frequency if one does not exists
        /// </summary>
        /// <param name="reportFrequncy">Current frequency</param>
        /// <param name="info">Information about the report file name</param>
        protected virtual void FileCreation(string reportFrequncy, string info)
        {
            string newPathLoc = Path.Combine(ReportFolderPath, Path.Combine(reportFrequncy, $"{info}.txt"));

            if (_currentReport == newPathLoc)
            {
                return;
            }

            _currentReport = newPathLoc;

            if (!File.Exists(_currentReport))
            {
                File.Create(_currentReport).Close();
                AddReport("----------File Created----------");
            }

            AddReport("----------File Opened----------");
        }
    }
}