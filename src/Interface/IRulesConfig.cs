using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IRulesConfig
    {
        [JsonProperty]
        internal string[] Names { get; set; }
        [JsonProperty]
        internal string[] Delimiter { get; set; }
        internal IRulesConfig()
        {
            Names = Array.Empty<string>();
            Delimiter = Array.Empty<string>();
        }
    }
}