namespace NFeAssistant.ExcelBase
{
    internal class SearchResult
    {
        internal string Content { get; set; }
        internal string CellAddress { get; set; }
        internal string FilePath { get; set; }

        internal SearchResult()
        {
            Content = "";
            CellAddress = "";
            FilePath = "";
        }
    }
}