using NFeAssistant.ExcelBase;
using NFeAssistant.Xml;

namespace NFeAssistant.Cache
{
    internal static class Cache
    {
        private static List<Summary> _excelSummarytList = new();
        private static List<NFeAssistant.Xml.Invoice> _invoiceList = new();

        internal static int RemoveContentFromCacheByFile(FileInfo fileInfo)
        {
            int removingCount = 0;
            lock(_excelSummarytList)
            {
                removingCount += _excelSummarytList.RemoveAll(summary => summary.FilePath.Replace('/', '\\') == fileInfo.FullName);
            }
            lock(_invoiceList)
            {
                removingCount += _invoiceList.RemoveAll(content => content.FilePath.Replace('/', '\\') == fileInfo.FullName);
            }

            return removingCount;
        }

        internal static bool InsertSummaryFileIntoCache(FileInfo fileInfo, string filePath)
        {
            ExcelFile? excelFile =  null;
            excelFile = ExcelFile.Read(fileInfo.FullName);

            if(excelFile == null)
                return false;

            var summary = Summary.FromExcelFileInstance(excelFile, filePath);
            if(summary == null)
                return false;
            
            lock(_excelSummarytList)
            {
                _excelSummarytList.Add(summary);
            }

            return true;
        }
        
        internal static bool InsertSummaryFileIntoCache(FileInfo fileInfo)
        {
            return InsertSummaryFileIntoCache(fileInfo, fileInfo.FullName);
        }

        internal static bool InsertInvoiceFileIntoCache(FileInfo fileInfo)
        {
            var invoice = Xml.Invoice.GetFromXMLFile(fileInfo.FullName);
            if(invoice == null)
                return false;

            if(!invoice.NoFails)
                return false;

            lock(_invoiceList)
            {
                _invoiceList.Add(invoice);
            }

            return true;
        }

        internal static List<Summary> SummaryList
        {
            get
            {
                return new List<Summary>(_excelSummarytList);
            }
        }

        internal static List<Xml.Invoice> InvoiceList
        {
            get
            {
                return new List<Xml.Invoice>(_invoiceList);
            }
        }
    }
}