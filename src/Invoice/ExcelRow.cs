using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    internal class ExcelRow
    {
        [JsonProperty]
        internal string EmissionDate { get; set; }
        [JsonProperty]
        internal string NF { get; set; }
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

        internal ExcelRow()
        {
            EmissionDate = "";
            NF = "";
            Client = "";
            City = "";
            Volumes = "";
            Weight = "";
            Value = "";
        }
    }
}