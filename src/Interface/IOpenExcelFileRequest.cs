using System.Diagnostics;
using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IOpenExcelFileRequest
    {
        [JsonProperty]
        internal string FilePath { get; set; }

        internal IOpenExcelFileRequest()
        {
            FilePath = "";
        }

        internal bool FileExists() =>  File.Exists(FilePath);  

        internal bool IsExcelFile()
        {
            if(!FileExists() )
                return false;
            
            var extension = Path.GetExtension(FilePath);

            return extension == null ? false : new string[]{".xlsx", "xls"}.Contains(extension.ToLower() );
        }

        internal bool Open()
        {
            var process = new ProcessStartInfo {
                UseShellExecute = true,
                FileName =  "excel",
                Arguments = $"\"{FilePath}\""
            };

            var processResult = Process.Start(process);
            if(processResult == null)
                return false;
            
            processResult.WaitForInputIdle();
            return true;
        }
    }
}