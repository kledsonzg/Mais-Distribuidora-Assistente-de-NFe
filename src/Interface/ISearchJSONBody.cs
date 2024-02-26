using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISearchJSONBody
    {
        [JsonProperty]
        internal ISearchRequestContent ToSearch { get; set; }

        internal ISearchJSONBody()
        {
            ToSearch = new();
        }
    }
}