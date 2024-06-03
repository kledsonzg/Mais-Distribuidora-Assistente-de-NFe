using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NFeAssistant.Invoice;
using NFeAssistant.Main;

namespace NFeAssistant.Database.Model
{
    public class InvoiceModel
    {
        public ObjectId Id;
        public Client Client;
        public ShippingCompany ShippingCompany;
        public InvoiceValue Value;
        public List<Product> Products;
        public DateTime Emission;
        public DateTime Shipment;
        public string FilePath {get; set;}
        public string NumberCode;
        public string AccessKey;
        public float Weight;
        public float Volumes;

        public InvoiceModel()
        {
            Id = new();
            Client = new();
            ShippingCompany = new();
            Value = new();
            Products = new();
            Emission = new();
            Shipment = new();
            FilePath = "";
            NumberCode = "";
            AccessKey = "";
            Weight = 0f;
            Volumes = 0f;
        }

        internal Xml.Invoice? ToXmlInvoice()
        {
            return Xml.Invoice.GetFromInvoiceDatabaseModel(this);
        }

        internal static InvoiceModel? GetFromXmlInvoice(Xml.Invoice invoice)
        {
            var model =  Newtonsoft.Json.JsonConvert.DeserializeObject<InvoiceModel>(Newtonsoft.Json.JsonConvert.SerializeObject(invoice) );
            return model;
        }
    }
}