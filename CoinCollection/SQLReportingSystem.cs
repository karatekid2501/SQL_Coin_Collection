using System.Globalization;
using System.IO;

namespace CoinCollection
{
    /// <summary>
    /// Report system for the coin collection SQL
    /// </summary>
    /// <param name="reportFolderName">Name of the folder that the reports are in</param>
    /// <param name="isEnabled">Should reporting be enabled</param>
    /// <param name="reportFrequency">The report frequencies</param>
    public class SQLReportingSystem(string reportFolderName = "", bool isEnabled = false, params string[] reportFrequency) : ReportingSystem(reportFolderName, isEnabled, reportFrequency)
    {
        protected override void UpdateFrequency()
        {
            _currentFrequency = App.GetInstance().ConfigEditor.Get<string>("Report Frequency", "Report Settings");
        }

        protected override void ReportSetUp()
        {
            Directory.CreateDirectory(Path.Combine(ReportFolderPath, "Daily"));
            Directory.CreateDirectory(Path.Combine(ReportFolderPath, "Weekly"));
            Directory.CreateDirectory(Path.Combine(ReportFolderPath, "Monthly"));

            if(IsEnabled)
            {
                switch (_currentFrequency)
                {
                    case "Daily":
                        FileCreation("Daily", $"{DateTime.Now.DayOfWeek} ({DateTime.Now.ToShortDateString().Replace('/', '_')})");
                        break;
                    case "Weekly":
                        //https://weeknumber.co.uk/how-to/c-sharp
                        FileCreation("Weekly", $"Week [{ISOWeek.GetWeekOfYear(DateTime.Today)}] ({DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek).ToShortDateString().Replace('/', '_')})");
                        break;
                    case "Monthly":
                        //https://stackoverflow.com/questions/6765441/how-to-get-complete-month-name-from-datetime
                        FileCreation("Monthly", $"Month [{DateTime.Today.Month} ({DateTime.Today.ToString("MMMM", CultureInfo.InvariantCulture)})] ({DateTime.Today.Year})");
                        break;
                    default:
                        throw new Exception($"{_currentFrequency} is not a frequency!!!");
                }
            }
        }
    }
}
