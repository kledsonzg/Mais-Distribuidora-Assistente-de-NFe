using Newtonsoft.Json;
using NFeAssistant.Main;

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
            var invoices = new List<Invoice>();

            Parallel.ForEach(NFeAssistant.Cache.Cache.InvoiceList, (invoice, loopState) =>
            {
                if(numberCodes.Count == 0)
                {
                    loopState.Stop();
                }

                if(!numberCodes.Contains(invoice.NumberCode) )
                    return;
                
                lock(invoices)
                {                    
                    invoices.Add(invoice);
                }

                lock(numberCodes)
                {
                    numberCodes.RemoveAll(number => number == invoice.NumberCode);
                }
            } );

            invoiceList.AddRange(invoices);
        }
    }
}