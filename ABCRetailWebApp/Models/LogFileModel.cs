using System.ComponentModel.DataAnnotations;

namespace ABCRetailWebApp.Models
{
    public class LogFileModel
    {
        // Captures the file name for the log file
        [Required(ErrorMessage = "Please specify a log file name.")]
        [RegularExpression(@"^[a-zA-Z0-9_\-\.]+$", ErrorMessage = "Invalid file name characters.")]
        public string FileName { get; set; } = "app-log.txt";

        // Captures the log details to append into the file
        [Required(ErrorMessage = "Please enter the log entry content.")]
        public string LogContent { get; set; } = string.Empty;
    }
}
