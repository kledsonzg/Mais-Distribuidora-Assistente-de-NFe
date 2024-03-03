using NFeAssistant.ExcelBase;
using NFeAssistant.Main;
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
            Program.PrintLine("Fim dos testes.");
        }
    }
}