using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    internal class ShippingCompany
    {
        [JsonProperty] internal string Nome { get; set; }
        [JsonProperty] internal string CNPJ { get; set; }
        [JsonProperty] internal string IE { get; set; }
        [JsonProperty] internal string Endereco { get; set; }
        [JsonProperty] internal string Cidade { get; set; }
        [JsonProperty] internal string Estado { get; set; }
        
        internal ShippingCompany()
        {
            Nome = "";
            CNPJ = "";
            IE = "";
            Endereco = "";
            Cidade = "";
            Estado = "";
        }
    }
}