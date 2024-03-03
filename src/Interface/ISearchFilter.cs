using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISearchFilter
    {
        [JsonProperty]
        internal string Value { get; set; }
        [JsonProperty]
        internal bool Precise { get; set; }

        internal ISearchFilter()
        {
            Value = "";
            Precise = false;
        }
    }
}