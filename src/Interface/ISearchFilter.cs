using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISearchFilter
    {
        [JsonProperty]
        internal string Value { get; set; } = string.Empty;
        [JsonProperty]
        internal bool Precise { get; set; }
    }

    internal class ISearchWeightFilter : ISearchFilter
    {
        [JsonProperty]
        internal bool IsGrossWeight { get; set; }
    }
}