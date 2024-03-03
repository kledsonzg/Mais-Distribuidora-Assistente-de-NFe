using Newtonsoft.Json;
using NFeAssistant.Util;

namespace NFeAssistant.Interface
{
    internal class ISearchRequestContent
    {
        [JsonProperty]
        internal ISearchFilter Date { get; set; }
        [JsonProperty]
        internal ISearchFilter NfeNumber { get; set; }
        [JsonProperty]
        internal ISearchFilter Client { get; set; }
        [JsonProperty]
        internal ISearchFilter City { get; set; }
        [JsonProperty]
        internal ISearchFilter Volumes { get; set; }
        [JsonProperty]
        internal ISearchFilter Weight { get; set; }
        [JsonProperty]
        internal ISearchFilter Value { get; set; }
        [JsonProperty]
        internal ISearchFilter ShippingCompany { get; set; }
        [JsonProperty]
        internal ISummaryDateFilter SummaryDateFilter { get; set; }

        internal ISearchRequestContent()
        {
            Date = new();
            NfeNumber = new();
            Client = new();
            City = new();
            Volumes = new();
            Weight = new();
            Value = new();
            ShippingCompany = new();
            SummaryDateFilter = new();
        }

        internal DateTime GetDateTime()
        {
            return Functions.GetDateTimeFromString(Date.Value);
        }
    }
}