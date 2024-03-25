using Newtonsoft.Json;
using NFeAssistant.Invoice;

namespace NFeAssistant.Interface
{
    internal class ISummaryFileGenerationRequest
    {
        [JsonProperty]
        internal string OutputFolder { get; set; }
        [JsonProperty]
        internal string FileName { get; set; }
        [JsonProperty]
        internal string Title { get; set; }
        [JsonProperty]
        internal ExcelRow[] Rows { get; set; }

        internal ISummaryFileGenerationRequest()
        {
            OutputFolder = "";
            FileName = "";
            Title = "";
            Rows = Array.Empty<ExcelRow>();
        }
    }
}