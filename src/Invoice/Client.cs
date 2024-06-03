namespace NFeAssistant.Invoice
{
    public class Client
    {
        public string Nome { get; set; }
        public string Cidade { get; set; }
        public string Bairro { get; set; }
        public string Estado { get; set; }
        public string CNPJ { get; set; }
        public string CPF { get; set; }
        public string Endereco { get; set; }
        public string CEP { get; set; }
        public string EnderecoNumero { get; set; }
        public Client()
        {
            Nome = "";
            Cidade = "";
            Bairro = "";
            Estado = "";
            CNPJ = "";
            CPF = "";
            Endereco = "";
            CEP = "";    
            EnderecoNumero = "";
        }
    }
}