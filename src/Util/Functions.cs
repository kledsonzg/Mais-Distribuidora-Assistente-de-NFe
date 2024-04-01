using System.Diagnostics;

namespace NFeAssistant.Util
{
    internal class Functions
    {
        internal static DateTime GetDateTimeFromString(string dateString)
        {
            int year = 1, month= 1, day = 1;
            if(!string.IsNullOrEmpty(dateString.Trim() ) )
            {
                int.TryParse(dateString.Substring(6, 4), out year);
                int.TryParse(dateString.Substring(3, 2), out month);
                int.TryParse(dateString.Substring(0, 2), out day);
            }
            
            var date = new DateTime(year, month, day);
            return date;
        }

        internal static void OpenFolder(string folder)
        {
            if(Path.GetFileName(folder) != null)
            {
                var parentFolder = Directory.GetParent(folder);
                if(parentFolder == null)
                    return;
                
                folder = parentFolder.FullName;
            }

            var process = new ProcessStartInfo()
            {
                FileName = "explorer",
                Arguments = $"\"{folder}\"",
                UseShellExecute = true
            };

            Process.Start(process);
        }

        internal static Process? StartPage(string url)
        {
            if(Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute) == false)
                return null;
            
            var process = new ProcessStartInfo()
            {
                FileName = url,
                UseShellExecute = true
            };

            return Process.Start(process);
        }
    }
}