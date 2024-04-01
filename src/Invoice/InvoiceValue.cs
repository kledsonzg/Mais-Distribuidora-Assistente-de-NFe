using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    internal class InvoiceValue
    {
        [JsonProperty] internal float Produtos = 0f;
        [JsonProperty] internal float Desconto = 0;
        [JsonProperty] internal float Total = 0;
    }
}