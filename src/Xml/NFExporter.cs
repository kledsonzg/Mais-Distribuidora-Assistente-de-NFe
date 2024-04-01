using Newtonsoft.Json;
using NFeAssistant.Main;

namespace NFeAssistant.Xml
{
    internal static class NFExporter
    {
        internal static string? GetInvoices(string requestBody)
        {
            var json = JsonConvert.DeserializeObject<string[]>(requestBody);
            var xmlPaths = Program.Config.Properties.App.XmlPath;
            var threadList = new List<Thread>();

            if(json == null)
            {
                return null;
            }

            var nfsList = new List<Invoice>();
            
            foreach(var path in xmlPaths)
            {
                var thread = new Thread(new ThreadStart(delegate 
                {
                    GetInvoicesFromXML(path, json, nfsList);
                } ) );

                threadList.Add(thread);
                thread.Start();
            }

            threadList.ForEach(thread => thread.Join() );
            return JsonConvert.SerializeObject(nfsList);
        }

        private static void GetInvoicesFromXML(string folder, string[] numberCodes, List<Invoice> invoiceList)
        {
            var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
            var threadList = new List<Thread>();
            var fileList = new List<string>();
            var invoices = new List<Invoice>();

            foreach(var file in files)
            {
            
                var thread = new Thread(new ThreadStart(delegate
                {
                    try
                    {
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
                    }
                    catch(Exception e)
                    {
                        Logger.Logger.Write($"Houve uma exceção no arquivo: '{file}'. Motivo: {e.Message} | Pilhas: {e.StackTrace}");
                    }
                } ) );

                threadList.Add(thread);
                thread.Start();
            }

            threadList.ForEach(thread => thread.Join() );

            lock(invoiceList)
            {
                invoiceList.AddRange(invoices);
            }
        }
    }
}