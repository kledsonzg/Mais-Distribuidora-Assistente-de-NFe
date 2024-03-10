namespace NFeAssistant.Invoice
{
    internal class Client
    {
        internal string Nome { get; set; }
        internal string Cidade { get; set; }
        internal string Bairro { get; set; }
        internal string Estado { get; set; }
        internal string CNPJ { get; set; }
        internal string CPF { get; set; }
        internal string Endereco { get; set; }
        internal string CEP { get; set; }

        internal int EnderecoNumero { get; set; }
        internal Client()
        {
            Nome = "";
            Cidade = "";
            Bairro = "";
            Estado = "";
            CNPJ = "";
            CPF = "";
            Endereco = "";
            CEP = "";
            
            EnderecoNumero = -1;
        }
    }
}