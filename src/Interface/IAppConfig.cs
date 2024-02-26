using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IAppConfig
    {
        [JsonProperty]
        internal string[] XmlPath { get; set; }
        [JsonProperty]
        internal string[] SummaryPath { get; set; }

        internal IAppConfig()
        {
            XmlPath = [];
            SummaryPath = [];
        }
    }
}