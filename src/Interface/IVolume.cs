using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IVolume
    {
        [JsonProperty]
        internal string[] Products;
        [JsonProperty]
        internal int Index;

        internal IVolume()
        {
            Products = Array.Empty<string>();
            Index = -1;
        }
    }
}