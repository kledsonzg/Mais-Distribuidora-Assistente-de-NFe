using Newtonsoft.Json;

namespace NFeAssistant.Invoice
{
    public class Product
    {
        public string Name { get; set; }
        public int Code { get; set; }
        public float Quantity { get; set; }
        public float Value { get; set; }

        public Product()
        {
            Name = "";
            Code = -1;
            Quantity = -1;
            Value = 0f;
        }
    }
}