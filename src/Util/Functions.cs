namespace NFeAssistant.Util
{
    internal class Functions
    {
        internal static DateTime GetDateTimeFromString(string dateString)
        {
            int year = 1, month= 1, day = 1;
            if(!string.IsNullOrEmpty(dateString.Trim() ) )
            {
                int.TryParse(dateString.Substring(6, 4), out year);
                int.TryParse(dateString.Substring(3, 2), out month);
                int.TryParse(dateString.Substring(0, 2), out day);
            }
            
            var date = new DateTime(year, month, day);
            return date;
        }
    }
}