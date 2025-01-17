using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class ISummaryGenerationJSONBody
    {
        [JsonProperty]
        internal ISummaryDateFilter XmlDateFilter { get; set; }
        [JsonProperty]
        internal ISummaryDateFilter EmissionDateFilter { get; set; }
        [JsonProperty]
        internal ISearchFilter ShippingCompany { get; set; }
        [JsonProperty]
        internal ISearchFilter Volumes { get; set; }
        [JsonProperty]
        internal ISearchWeightFilter Weight { get; set; }
        [JsonProperty]
        internal string OutputFolder { get; set; }

        internal ISummaryGenerationJSONBody()
        {
            XmlDateFilter = new();
            EmissionDateFilter = new();
            ShippingCompany = new();
            Volumes = new();
            Weight = new();
            OutputFolder = "";
        }
    }
}