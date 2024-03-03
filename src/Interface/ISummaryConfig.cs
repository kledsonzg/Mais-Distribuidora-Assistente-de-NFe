using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISummaryConfig
    {
        [JsonProperty]
        internal string[] DefaultWorksheetName { get; set; }
        [JsonProperty]
        internal string DefaultTitleCell { get; set; }
        [JsonProperty]
        internal ITableRulesConfig TableNameRules { get; set; }
        
        internal ISummaryConfig()
        {
            DefaultWorksheetName = Array.Empty<string>();
            DefaultTitleCell = "";
            TableNameRules = new();
        }
    }
}