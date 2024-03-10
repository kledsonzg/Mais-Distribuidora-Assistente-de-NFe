using NFeAssistant.Interface;

namespace NFeAssistant.Invoice
{
    internal class ShippingCompany
    {
        internal string Nome { get; set; }
        internal string CNPJ { get; set; }
        internal string IE { get; set; }
        internal string Endereco { get; set; }
        internal string Cidade { get; set; }
        internal string Estado { get; set; }
        
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