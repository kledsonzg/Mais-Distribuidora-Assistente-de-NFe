using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    internal class Product
    {
        [JsonProperty] internal string Name { get; set; }
        [JsonProperty] internal int Code { get; set; }
        [JsonProperty] internal float Quantity { get; set; }
        [JsonProperty] internal float Value { get; set; }

        internal Product()
        {
            Name = "";
            Code = -1;
            Quantity = -1;
            Value = 0f;
        }
    }
}