using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISearchResult
    {
        [JsonProperty]
        internal int RowIndex { get; set; }
        [JsonProperty]
        internal string FilePath { get; set; }
        [JsonProperty]
        internal string Content { get; set; }
        [JsonProperty]
        internal string SheetName { get; set; }

        internal ISearchResult()
        {
            RowIndex = 0;
            FilePath = "";
            Content = "";
            SheetName = "";
        }
    }
}