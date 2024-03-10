using NFeAssistant.ExcelBase;
using NFeAssistant.Main;
using NFeAssistant.Xml;
using NPOI.HSSF.UserModel;

namespace NFeAssistant.Testing
{
    internal static class Test
    {
        internal static void Run()
        {
            Program.PrintLine("Rodando testes...");
            //var reader = ExcelFile.Read(@"F:\Users\kleds\Downloads\RELATORIOS DE CARREGAMENTO TRANSP\RELATORIOS DE CARREGAMENTO TRANSP\DNG\RELATÓRIO DNG\JULHO\DNG 28-07 MANHÃ.xls");
            //Program.PrintLine($"volumes count: {reader.GetVolumes().Length}.");
            // Program.PrintLine($"valores count: {reader.GetValues().Length}.");
            // foreach(var result in reader.GetValues() )
            // {
            //     Program.PrintLine($"Valor: {result.Content} | Endereço: {result.CellAddress}");
            // }

            // reader = ExcelFile.Read(@"F:\Users\kleds\Downloads\RELATORIOS DE CARREGAMENTO TRANSP\RELATORIOS DE CARREGAMENTO TRANSP\DNG\RELATÓRIO DNG\DNG 26-02 TARDE.xlsx");
            // Program.PrintLine($"dates count: {reader.GetDates().Length}.");
            // Program.PrintLine($"volumes count: {reader.GetVolumes().Length}.");
            // Program.PrintLine($"valores count: {reader.GetValues().Length}.");
            // foreach(var result in reader.GetValues() )
            // {
            //     Program.PrintLine($"Valor: {result.Content} | Endereço: {result.CellAddress}");
            // }
            // foreach(var format in HSSFDataFormat.GetBuiltinFormats() )
            // {
            //     Program.PrintLine($"FORMATO: {format} : {HSSFDataFormat.GetBuiltinFormat(format) }");
            // }
            //var invoice = NFeAssistant.Xml.Invoice.GetFromXMLFile(@"ocultado\Para análise - Relatorios e NF\31240211122552000199550010000923931000678487.xml");
            //Program.PrintLine($"Invoice: {invoice.FilePath}\n\t{invoice.Client.Nome}, {invoice.Client.Cidade}\n\t{invoice.ShippingCompany.Nome}\n\t{invoice.NumberCode}, {invoice.Volumes}, {invoice.Weight}, {invoice.Value.Total}\n\tTudo certo? {invoice.NoFails}");
            //Program.PrintLine($"Invoice: {invoice.FilePath}");
            //Program.PrintLine($"Invoice: {invoice.Client.Nome}");
            //Program.PrintLine($"Quantidade de produtos: {invoice.Products.Count}");
            //foreach(var product in invoice.Products)
            //{
            //    Program.PrintLine($"Produto: {product.Code} - {product.Name} - {product.Quantity} - {product.Value}");
            //}
            Program.PrintLine("Fim dos testes.");
        }
    }
}