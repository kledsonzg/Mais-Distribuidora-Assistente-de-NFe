using Newtonsoft.Json;
using NFeAssistant.Util;

namespace NFeAssistant.Interface
{
    internal class ISearchRequestContent
    {
        [JsonProperty]
        internal string Date { get; set; }
        [JsonProperty]
        internal string NfeNumber { get; set; }
        [JsonProperty]
        internal string Client { get; set; }
        [JsonProperty]
        internal string City { get; set; }
        [JsonProperty]
        internal string Volumes { get; set; }
        [JsonProperty]
        internal string Weight { get; set; }
        [JsonProperty]
        internal string Value { get; set; }
        [JsonProperty]
        internal string ShippingCompany { get; set; }

        internal ISearchRequestContent()
        {
            Date = "";
            NfeNumber = "";
            Client = "";
            City = "";
            Volumes = "";
            Weight = "";
            Value = "";
            ShippingCompany = "";
        }

        internal DateTime GetDateTime()
        {
            return Functions.GetDateTimeFromString(Date);
        }
    }
}