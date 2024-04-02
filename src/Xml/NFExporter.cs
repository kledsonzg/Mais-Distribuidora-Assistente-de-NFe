using System.Collections.Concurrent;
using Newtonsoft.Json;
using NFeAssistant.Main;
using NPOI.Util.ArrayExtensions;

namespace NFeAssistant.Xml
{
    internal static class NFExporter
    {
        internal static string? GetInvoices(string requestBody)
        {
            var json = JsonConvert.DeserializeObject<string[]>(requestBody);
            var xmlPaths = Program.Config.Properties.App.XmlPath;

            if(json == null)
            {
                return null;
            }

            var nfsList = new List<Invoice>();
            
            Parallel.ForEach(xmlPaths, (path) =>
            {
                GetInvoicesFromXML(path, new List<string>(json), nfsList);
            } );

            return JsonConvert.SerializeObject(nfsList);
        }

        private static void GetInvoicesFromXML(string folder, List<string> numberCodes, List<Invoice> invoiceList)
        {
            var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
            var invoices = new List<Invoice>();

            Parallel.ForEach(files, (file, loopState) =>
            {
                try
                {
                    if(numberCodes.Count == 0)
                    {
                        loopState.Stop();
                    }

                    var invoice = Invoice.GetFromXMLFile(file);
                    if(invoice == null)
                    {
                        Logger.Logger.Write($"A instância de 'Invoice' no arquivo: '{file}' é nula.");
                        return;
                    }
                    if(!numberCodes.Contains(invoice.NumberCode) )
                        return;
                    
                    lock(invoices)
                    {
                        invoices.Add(invoice);
                    }

                    lock(numberCodes)
                    {
                        numberCodes.Remove(invoice.NumberCode);
                    }
                }
                catch(Exception e)
                {
                    Logger.Logger.Write($"Houve uma exceção no arquivo: '{file}'. Motivo: {e.Message} | Pilhas: {e.StackTrace}");
                }
            } );

            lock(invoiceList)
            {
                invoiceList.AddRange(invoices);
            }
        }
    }
}