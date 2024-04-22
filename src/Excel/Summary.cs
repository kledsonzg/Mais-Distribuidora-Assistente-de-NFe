namespace NFeAssistant.ExcelBase
{
    internal class Summary
    {
        internal List<SearchResult> Contents;
        internal string Title;
        internal string FilePath;

        private Summary()
        {
            Contents = new();
            Title = "";
            FilePath = "";
        }
        
        internal static Summary? FromExcelFileInstance(ExcelFile excelFile, string filePath)
        {
            var result = new Summary();

            if(excelFile == null)
                return null;
            
            List<List<SearchResult> > list = new();

            list.Add(excelFile.GetDates().ToList() );
            list.Add(excelFile.GetFiscalNumbers().ToList() );
            list.Add(excelFile.GetClients().ToList() );
            list.Add(excelFile.GetCities().ToList() );
            list.Add(excelFile.GetVolumes().ToList() );
            list.Add(excelFile.GetWeights().ToList() );
            list.Add(excelFile.GetValues().ToList() );

            foreach(List<SearchResult> contentList in list)
            {
                if(contentList.Count == 0)
                    continue;
                
                result.Contents.AddRange(contentList);
            }

            result.Title = excelFile.Title;
            result.FilePath = filePath;
            return result;
        }
    }
}