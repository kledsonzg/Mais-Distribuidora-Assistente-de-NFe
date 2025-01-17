using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IConfig
    {
        [JsonProperty]
        internal IAppConfig App { get; set; }
        [JsonProperty]
        internal ISummaryConfig Summary { get; set; }     

        internal IConfig()
        {
            App = new();
            Summary = new();
        }
    }
}