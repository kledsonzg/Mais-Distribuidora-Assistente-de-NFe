using Newtonsoft.Json;
using NFeAssistant.Util;

namespace NFeAssistant.Interface
{
    internal class ISummaryDateFilter
    {
        [JsonProperty]
        internal string From 
        { 
            get 
            { 
                return FromDate.ToShortDateString(); 
            }
            set
            {
                FromDate = Functions.GetDateTimeFromString(value);
            }
        }
        [JsonProperty]
        internal string To 
        { 
            get 
            { 
                return ToDate.ToShortDateString(); 
            }
            set
            {
                ToDate = Functions.GetDateTimeFromString(value);
            }
        }

        internal DateTime FromDate;
        internal DateTime ToDate;

        internal ISummaryDateFilter()
        {
            FromDate = Functions.GetDateTimeFromString("");
            ToDate = Functions.GetDateTimeFromString("");
        }
    }
}