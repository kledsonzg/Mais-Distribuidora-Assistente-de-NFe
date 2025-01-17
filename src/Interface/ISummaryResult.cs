using Newtonsoft.Json;
using NFeAssistant.Invoice;

namespace NFeAssistant.Interface
{
    internal class ISummaryResult
    {
        [JsonProperty]
        internal string OutputFolder { get; set; }
        [JsonProperty]
        internal ExcelRow[] Rows { get; set; }

        internal ISummaryResult()
        {
            OutputFolder = "";
            Rows = Array.Empty<ExcelRow>();
        }
    }
}