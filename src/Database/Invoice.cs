using MongoDB.Bson;
using MongoDB.Driver;
using NFeAssistant.Main;

namespace NFeAssistant.Database
{
    internal static class Invoice
    {
        private static readonly MongoClient _mongoClient = new("mongodb://localhost:27017");
        private static readonly IMongoDatabase _mongoDB = _mongoClient.GetDatabase("NFeAssistantDB");

        internal static void SaveAll(List<Xml.Invoice> invoiceList)
        {
           IMongoCollection<Model.InvoiceModel> _invoiceCollection = _mongoDB.GetCollection<Model.InvoiceModel>("invoices");
           
           foreach(var invoice in invoiceList)
           {
                if(!invoice.NoFails)
                    continue;
                try
                {
                    // Se a nota fiscal existir no banco de dados, porém o caminho para o arquivo for diferente então essa entrada é removida do banco de dados.
                    _invoiceCollection.FindOneAndDelete(filter => filter.AccessKey == invoice.AccessKey && filter.FilePath != invoice.FilePath);

                    // Se a nota fiscal existir no banco de dados e o caminho para o arquivo .xml for igual, então não há porque inserir no banco de dados novamente.
                    if(_invoiceCollection.CountDocuments(filter => filter.AccessKey == invoice.AccessKey && filter.FilePath == invoice.FilePath) > 0)
                        continue;
                    
                    // Convertendo os dados da instância principal da nota fiscal em uma instância adaptada para o banco de dados.
                    var invoiceModel = Model.InvoiceModel.GetFromXmlInvoice(invoice);
                    if(invoiceModel == null)
                        continue;
                    
                    // E por fim inserimos essa instãncia adaptada.
                    _invoiceCollection.InsertOne(invoiceModel);
                }

                // Se o motivo da exceção for um erro de conexão com o banco de dados, não há motivo para tentar continuar o processo de iteração.
                catch(TimeoutException timeOutException)
                {
                    string[] msgs = {$"Falha durante o salvamento de notas fiscais no banco de dados (NF: {invoice.NumberCode}, FilePath: {invoice.FilePath}). Motivo: {timeOutException.Message}", 
                        $"Falha durante o salvamento de notas fiscais no banco de dados (NF: {invoice.NumberCode}, FilePath: {invoice.FilePath}). Motivo: {timeOutException.GetBaseException()}"};
                    Program.PrintLine(msgs[0] );
                    Logger.Logger.Write(msgs[1] );
                    break;
                }
                // Caso o motivo seja inesperado, então a iteração continua.
                catch(Exception exception)
                {
                    string[] msgs = {$"Falha durante o salvamento de uma nota fiscal (NF: {invoice.NumberCode}, FilePath: {invoice.FilePath}). Motivo: {exception.Message}",
                        $"Falha durante o salvamento de uma nota fiscal (NF: {invoice.NumberCode}, FilePath: {invoice.FilePath}). Motivo: {exception.GetBaseException()}"};
                    Program.PrintLine(msgs[0] );
                    Logger.Logger.Write(msgs[1] );
                }
           }
        }

        internal static void SaveAll() => SaveAll(Cache.Cache.InvoiceList);    

        internal static List<Xml.Invoice> GetAll()
        {
            try
            {
                IMongoCollection<Model.InvoiceModel> _invoiceCollection = _mongoDB.GetCollection<Model.InvoiceModel>("invoices");
                List<Xml.Invoice> invoiceList = new();
                var list = _invoiceCollection.Find(invoice => invoice != null).ToList();
                foreach(var invoiceModel in list)
                {
                    var invoice = Xml.Invoice.GetFromInvoiceDatabaseModel(invoiceModel);
                    if(invoice == null)
                        continue;
                    
                    if(!File.Exists(invoice.FilePath) )
                    {
                        _invoiceCollection.DeleteMany(filter => filter.FilePath == invoice.FilePath);
                        continue;
                    }
                    
                    invoiceList.Add(invoice);
                }

                Program.PrintLine($"Total de Notas Fiscais obtidas do banco de dados: {invoiceList.Count}");
                return invoiceList;
            }
            catch(Exception exception)
            {
                Program.PrintLine($"Falha ao obter as NF's do banco de dados. Motivo: {exception.GetBaseException()}");
                Logger.Logger.Write($"Falha ao obter as NF's do banco de dados. Motivo: {exception.GetBaseException()}");
            }

            return new();
        }
    }
}