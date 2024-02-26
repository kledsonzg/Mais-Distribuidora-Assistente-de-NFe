using Newtonsoft.Json;
using NFeAssistant.ExcelBase;

namespace NFeAssistant.Interface
{
    internal class ISearchResult
    {
        [JsonProperty]
        internal int RowIndex { get; set; }
        [JsonProperty]
        internal string FilePath { get; set; }

        internal ISearchResult()
        {
            RowIndex = 0;
            FilePath = "";
        }
    }
}