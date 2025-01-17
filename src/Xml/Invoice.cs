using System.Xml;
using Newtonsoft.Json;
using NFeAssistant.Invoice;
using NFeAssistant.Util;

namespace NFeAssistant.Xml
{
    internal class Invoice
    {
        private XmlDocument Document { get; }

        [JsonProperty] internal Client Client;
        [JsonProperty] internal ShippingCompany ShippingCompany;
        [JsonProperty] internal InvoiceValue Value;
        [JsonProperty] internal List<Product> Products;
        [JsonProperty] internal DateTime Emission;
        [JsonProperty] internal DateTime Shipment;
        [JsonProperty] internal string FilePath { get; }   
        [JsonProperty] internal string NumberCode;
        [JsonProperty] internal string AccessKey;
        [JsonProperty] internal float NetWeight;
        [JsonProperty] internal float GrossWeight;
        [JsonProperty] internal float Volumes;
        [JsonProperty] internal bool NoFails { get { return AllRight; } }
        private bool AllRight;


        private Invoice(string filePath) 
        {
            FilePath = filePath;
            Document = new();
            AllRight = false;

            var reader = new XmlTextReader(File.OpenRead(filePath) );
            reader.Namespaces = false;

            Document.Load(reader);
            Read();

            // Liberar os recursos.
            Document = new();
        }

        internal static Invoice? GetFromXMLFile(string filePath)
        {
            if(!File.Exists(filePath) )
                return null;
            
            var extension = Path.GetExtension(filePath);
            if(extension == null || !(".xml").Contains(extension.ToLower() ) )
                return null;
            
            return new Invoice(filePath);
        }

        private void Read()
        {
            var root = Document.DocumentElement;
            if(root == null)
                return;
            
            var aboutInvoiceElement = root.SelectSingleNode(".//ide");
            var clientElement = root.SelectSingleNode(".//dest");
            var valuesElement = root.SelectSingleNode(".//total/ICMSTot");
            var shippingCompanyElement = root.SelectSingleNode(".//transporta");
            var productsElements = root.GetElementsByTagName("det");
            var shippingInfoElement = root.SelectNodes(".//transp/vol");
            var invoiceProtocolElement = root.SelectSingleNode(".//protNFe/infProt");
            
            if(aboutInvoiceElement == null || clientElement == null || valuesElement == null || shippingInfoElement == null || invoiceProtocolElement == null)
                return;                 
            
            if(!SetClient(clientElement) || 
                !SetInvoiceValues(valuesElement) || !SetInvoiceInfo(aboutInvoiceElement) || 
                !SetVolumetry(shippingInfoElement) || !SetInvoiceAccessKey(invoiceProtocolElement) 
            )
                return;
            

            SetShippingCompany(shippingCompanyElement);          
            SetProdutsList(productsElements);
            AllRight = true;
        }

        private bool SetInvoiceAccessKey(XmlNode invoiceProtocolElement)
        {
            var keyElement = invoiceProtocolElement.SelectSingleNode(".//chNFe");
            if(keyElement == null)
                return false;

            this.AccessKey = keyElement.InnerText;
            return true;
        }

        private bool SetVolumetry(XmlNodeList volumeElements)
        {
            int count = 0; // Volumetria.
            float netWeight = 0f; //Peso liquido.
            float grossWeight = 0f; //Peso bruto.
            
            if(volumeElements.Count > 0) for(int i = 0; i < volumeElements.Count; i++)
            {
                var element = volumeElements[i];
                if(element == null)
                    continue;
                
                var volElement = element.SelectSingleNode(".//qVol") ?? element.SelectSingleNode(".//nVol");
                if(volElement == null)
                    continue;

                int _count;
                int.TryParse(volElement.InnerText.Replace('.', ','), out _count);

                string[] weightElementsTags = new string[]{"pesoL", "pesoB"};
                float[] weights = new float[weightElementsTags.Length];
                for(int j = 0; j < weightElementsTags.Length; j++)
                {
                    try
                    {
                        float.TryParse(element.SelectSingleNode($".//{weightElementsTags[j]}")?.InnerText.Replace('.', ','), out weights[j] );
                    }
                    catch
                    {
                        return false;
                    }
                }

                count += _count;
                netWeight += weights[0];
                grossWeight += weights[1];
            }

            NetWeight = netWeight;
            GrossWeight = grossWeight;
            Volumes = count;
            return true;
        }

        private bool SetInvoiceInfo(XmlNode aboutInvoiceElement)
        {
            var invoiceNumberElement = aboutInvoiceElement.SelectSingleNode(".//nNF");
            var emissionDateElement = aboutInvoiceElement.SelectSingleNode(".//dhEmi");
            var shipmentDateElement = aboutInvoiceElement.SelectSingleNode(".//dhSaiEnt");
            if(invoiceNumberElement == null || emissionDateElement == null || shipmentDateElement == null)
                return false;
            
            NumberCode = invoiceNumberElement.InnerText;
            string emissionString = emissionDateElement.InnerText;
            string shipmentString = shipmentDateElement.InnerText;
            Emission = Functions.GetDateTimeFromString($"{emissionString.Substring(8, 2)}/{emissionString.Substring(5, 2)}/{emissionString[..4]}");
            Shipment = Functions.GetDateTimeFromString($"{shipmentString.Substring(8, 2)}/{shipmentString.Substring(5, 2)}/{shipmentString[..4]}");
            return true;
        }

        private bool SetInvoiceValues(XmlNode valuesElement)
        {
            Value = new();

            var discountValueElement = valuesElement.SelectSingleNode(".//vDesc");
            var totalValueElement = valuesElement.SelectSingleNode(".//vNF");
            var productsValueElement = valuesElement.SelectSingleNode(".//vProd");

            if(discountValueElement == null || totalValueElement == null || productsValueElement == null)
                return false;
            
            float.TryParse(discountValueElement.InnerText.Replace('.', ','), out Value.Desconto);
            float.TryParse(totalValueElement.InnerText.Replace('.', ','), out Value.Total);
            float.TryParse(productsValueElement.InnerText.Replace('.', ','), out Value.Produtos);
            return true;
        }

        private void SetShippingCompany(XmlNode? shippingCompanyElement)
        {
            ShippingCompany = new();

            if(shippingCompanyElement == null)
                return;

            var cityElement = shippingCompanyElement.SelectSingleNode(".//xMun");
            var ufElement = shippingCompanyElement.SelectSingleNode(".//UF");
            var documentElement = shippingCompanyElement.SelectSingleNode(".//CNPJ");
            var addressElement = shippingCompanyElement.SelectSingleNode(".//xEnder");
            var nameElement = shippingCompanyElement.SelectSingleNode(".//xNome");
            var ieElement = shippingCompanyElement.SelectSingleNode(".//IE");
            
            ShippingCompany.Nome = nameElement?.InnerText ?? "";
            ShippingCompany.Cidade = cityElement?.InnerText ?? "";
            ShippingCompany.Estado = ufElement?.InnerText ?? "";
            ShippingCompany.Endereco = addressElement?.InnerText ?? "";
            ShippingCompany.CNPJ = documentElement?.InnerText ?? "";
            ShippingCompany.IE = ieElement?.InnerText ?? "";
        }

        private bool SetClient(XmlNode clientElement)
        {
            Client = new();
            
            var clientNameElement = clientElement.SelectSingleNode(".//xNome");
            var clientAddressElement = clientElement.SelectSingleNode(".//enderDest");
            var clientDocumentElement = clientElement.SelectSingleNode(".//CNPJ");

            if(clientDocumentElement == null)
            {
                clientDocumentElement = clientElement.SelectSingleNode(".//CPF");
                if(clientDocumentElement == null)
                    return false;
                Client.CPF = clientDocumentElement.InnerText;
            }
            else Client.CNPJ = clientDocumentElement.InnerText;

            if(clientNameElement == null || clientAddressElement == null)
                return false;
                
            Client.Nome = clientNameElement.InnerText;
            var addressElement = clientAddressElement.SelectSingleNode(".//xLgr");
            var numberElement = clientAddressElement.SelectSingleNode(".//nro");
            var districtElemennt = clientAddressElement.SelectSingleNode(".//xBairro");
            var cepElement = clientAddressElement.SelectSingleNode(".//CEP");
            var cityElement = clientAddressElement.SelectSingleNode(".//xMun");
            var ufElement = clientAddressElement.SelectSingleNode(".//UF");
            if(addressElement == null || numberElement == null || districtElemennt == null || cepElement == null || cityElement == null || ufElement == null)
                return false;
            
            Client.Cidade = cityElement.InnerText;
            Client.Estado = ufElement.InnerText;
            Client.Bairro = districtElemennt.InnerText;
            Client.CEP = cepElement.InnerText;
            Client.EnderecoNumero = numberElement.InnerText;
            
            return true;
        }

        private void SetProdutsList(XmlNodeList productsElements)
        {
            Products = new();

            for(int i = 0; i < productsElements.Count; i++)
            {
                var element = productsElements[i];
                if(element == null)
                    continue;
                
                var products = element.SelectNodes(".//prod");
                if(products == null)
                    continue;
                
                for(int j = 0; j < products.Count; j++)
                {
                    var productElement = products[j];

                    if(productElement == null)
                        continue;
                        
                    var productNameElement = productElement.SelectSingleNode(".//xProd");
                    var productCodeElement = productElement.SelectSingleNode(".//cProd");
                    var productQuantityElement = productElement.SelectSingleNode(".//qCom");
                    var productValueElement = productElement.SelectSingleNode(".//vProd");

                    if(productCodeElement == null || productNameElement == null || productQuantityElement == null || productValueElement == null)
                        continue;
                    
                    string name = productNameElement.InnerText;
                    int code = -1;
                    float quantity = 0f, value = 0f;
                    int.TryParse(productCodeElement.InnerText, out code);
                    float.TryParse(productQuantityElement.InnerText.Replace('.', ','), out quantity);
                    float.TryParse(productValueElement.InnerText.Replace('.', ','), out value);

                    var product = new Product{ Name = name, Code = code, Quantity = quantity, Value = value};
                    Products.Add(product);
                }
            }
        }
    }
}