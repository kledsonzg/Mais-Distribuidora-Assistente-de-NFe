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

            if(json == null)
            {
                return null;
            }

            var nfsList = new List<Invoice>();
            GetInvoicesFromXML(new List<string>(json), nfsList);

            return JsonConvert.SerializeObject(nfsList);
        }

        private static void GetInvoicesFromXML(List<string> numberCodes, List<Invoice> invoiceList)
        {
            var files = FileListUpdater.Updater.GetXMLFiles();
            var invoices = new List<Invoice>();

            Parallel.ForEach(files, (file, loopState) =>
            {
                try
                {
                    if(numberCodes.Count == 0)
                    {
                        loopState.Stop();
                    }

                    var invoice = Invoice.GetFromXMLFile(file.FullName);
                    if(invoice == null)
                    {
                        Logger.Logger.Write($"A instância de 'Invoice' no arquivo: '{file.FullName}' é nula.");
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
                    Logger.Logger.Write($"Houve uma exceção no arquivo: '{file.FullName}'. Motivo: {e.Message} | Pilhas: {e.StackTrace}");
                }
            } );

            invoiceList.AddRange(invoices);
        }
    }
}