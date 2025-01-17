using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ITableRulesConfig
    {
        [JsonProperty]
        internal IRulesConfig Client { get; set; }
        [JsonProperty]
        internal IRulesConfig City { get; set; }
        [JsonProperty]
        internal IRulesConfig Date { get; set; }
        [JsonProperty]
        internal IRulesConfig Volumes { get; set; }
        [JsonProperty]
        internal IRulesConfig Weight { get; set; }
        [JsonProperty]
        internal IRulesConfig Value { get; set; }
        [JsonProperty]
        internal IRulesConfig NF { get; set; }

        internal ITableRulesConfig()
        {
            Client = new();
            City = new();
            Date = new();
            Volumes = new();
            Weight = new();
            Value = new();
            NF = new();
        }
    }
}