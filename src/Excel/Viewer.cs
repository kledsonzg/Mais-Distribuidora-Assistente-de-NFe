using System.Net;
using NFeAssistant.HttpServer;
using NFeAssistant.Interface;

namespace NFeAssistant.ExcelBase
{
    internal static class Viewer
    {
        internal static bool View(Stream requestBodyStream)
        {
            try
            {
                var excelFile = Newtonsoft.Json.JsonConvert.DeserializeObject<IOpenExcelFileRequest>(new StreamReader(requestBodyStream).ReadToEnd() );

                if(excelFile == null || !excelFile.IsExcelFile() )
                    return false;
                
                return excelFile.Open();
            }
            catch
            {
                return false;
            }
        }
    }
}