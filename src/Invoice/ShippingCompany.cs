using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    public class ShippingCompany
    {
        public string Nome { get; set; }
        public string CNPJ { get; set; }
        public string IE { get; set; }
        public string Endereco { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        
        public ShippingCompany()
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