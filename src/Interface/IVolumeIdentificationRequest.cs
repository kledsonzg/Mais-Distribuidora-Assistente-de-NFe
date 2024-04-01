using Newtonsoft.Json;

namespace NFeAssistant.Interface
{
    internal class IVolumeIdentificationRequest
    {
        [JsonProperty] internal IVolume[] Volumes { get; set; }
        [JsonProperty] internal string Nfs { get; set; }

        internal IVolumeIdentificationRequest()
        {
            Volumes = Array.Empty<IVolume>();
            Nfs = "";
        }
    }
}