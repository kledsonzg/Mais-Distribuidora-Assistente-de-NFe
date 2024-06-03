using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IAppConfig
    {
        [JsonProperty]
        internal string[] XmlPath { get; set; }
        [JsonProperty]
        internal string[] SummaryPath { get; set; }
        [JsonProperty]
        internal string[] WebPageIpConfig { get; set; }

        internal IAppConfig()
        {
            XmlPath = Array.Empty<string>();
            SummaryPath = Array.Empty<string>();
            WebPageIpConfig = Array.Empty<string>();
        }
    }
}