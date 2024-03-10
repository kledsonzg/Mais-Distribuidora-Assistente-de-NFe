namespace NFeAssistant.Invoice
{
    internal class Product
    {
        internal string Name { get; set; }
        internal int Code { get; set; }
        internal float Quantity { get; set; }
        internal float Value { get; set; }

        internal Product()
        {
            Name = "";
            Code = -1;
            Quantity = -1;
            Value = 0f;
        }
    }
}