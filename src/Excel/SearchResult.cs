using NFeAssistant.Definitions.Excel;

namespace NFeAssistant.ExcelBase
{
    internal class SearchResult
    {
        internal string Content { get; set; }
        internal string SheetName { get; set; }
        internal string CellAddress { get; set; }
        internal string FilePath { get; set; }
        internal ColumnType Type { get; set; }
        
        internal SearchResult()
        {
            Content = "";
            CellAddress = "";
            FilePath = "";
            Type = ColumnType.COLUMN_UNDEFINED;
        }
    }
}