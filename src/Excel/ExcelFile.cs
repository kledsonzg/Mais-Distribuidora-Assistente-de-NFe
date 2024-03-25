using System.Configuration;
using NFeAssistant.Main;
using NFeAssistant.Interface;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using NPOI.HSSF.UserModel;
using System.Text;
using NFeAssistant.Definitions.Excel;

namespace NFeAssistant.ExcelBase
{
    internal class ExcelFile
    {
        static IConfig Config = Program.Config.Properties;
        internal XSSFWorkbook XSSWorkBook;
        internal HSSFWorkbook HSSWorkBook;
        internal bool IsOLE2Format;
        string FilePath = "";
        private Stream FileStream;

        private ExcelFile() {}
        private ExcelFile(string filePath)
        {
            FilePath = filePath;
            XSSWorkBook = new XSSFWorkbook(filePath);
        }
        private ExcelFile(string filePath, bool isOLE2)
        {
            if(isOLE2)
            {
                IsOLE2Format = true;
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite) )
                {
                    HSSWorkBook = new HSSFWorkbook(fs);
                }            
            }
            else
            {
                IsOLE2Format = false;
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite) )
                {
                    XSSWorkBook = new XSSFWorkbook(fs);
                }        
            }

            FilePath = filePath;
        }

        internal void Save(string filePath)
        {
            if(IsOLE2Format)
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite) )
                {
                    HSSWorkBook.Write(fs, false);
                }
            else{
                using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite) )
                {
                    XSSWorkBook.Write(fs, false);
                }
            }
        }

        internal static ExcelFile? Read(string filePath)
        {
            if(!File.Exists(filePath) )
            {
                Program.PrintLine($"O arquivo '{filePath}' não existe.");
                return null;
            }

            string[] supportedExcelFiles = new string[] {".xls", ".xlsx"};
            var fileExtension = Path.GetExtension(filePath);
            if(fileExtension == null)
            {
                Program.PrintLine($"O arquivo '{filePath}' não é válido.");
                return null;
            }

            fileExtension = fileExtension.ToLower();
            if(!supportedExcelFiles.Contains(fileExtension) )
            {
                Program.PrintLine($"O arquivo '{filePath}' não é um arquivo suportado.");
                return null;
            }

            return new ExcelFile(filePath, fileExtension == supportedExcelFiles[0] );
        }

        internal string Title
        {
            get
            {
                var titleCell = Config.Summary.DefaultTitleCell;
                var sheet = GetSummarySheet();

                if(sheet == null)
                    return "";
                
                for(int i = 0; i < sheet.LastRowNum; i++)
                {
                    if(sheet.GetRow(i) == null)
                        continue;
                    
                    foreach(var cell in sheet.GetRow(i).Cells)
                    {
                        if(cell.Address.FormatAsString() != titleCell)
                            continue;
                        
                        var value = cell.ToString();
                        if(value == null)
                            return "";
                        
                        return value;
                    }
                }

                return "";
            }
        }

        internal static string GetCellValue(ICell cell)
        {
            string? result = "";
            switch(cell.CellType)
            {
                case CellType.Numeric:
                {
                    if(cell.CellStyle.DataFormat == 14)
                    {
                        result = cell.DateCellValue.ToShortDateString();
                        break;
                    }                      

                    result = cell.NumericCellValue.ToString();
                    break;
                }
                
                default:
                {
                    result = cell.ToString();
                    break;
                }
            }

            return result ?? "";
        }

        internal static string GetRowContent(ISheet sheet, int rowIndex)
        {
            var row = sheet.GetRow(rowIndex);
            if(row == null)
                return "";

            string result = "";
            foreach(var cell in row.Cells)
            {
                if(cell == null)
                    continue;
                
                result += $"{GetCellValue(cell)}";
                if(cell == row.Last() )
                    continue;
                
                result += "\t";
            }

            return result;
        }
        internal string GetRowContent(int rowIndex)
        {
            var sheet = GetSummarySheet();

            if(sheet == null)
                return "";

            return GetRowContent(sheet, rowIndex);
        }

        internal static string GetColumnFromCellAddress(string address)
        {
            string column = "";
            for(int i = 0; i < address.Length; i++)
            {
                if(int.TryParse(address[i].ToString(), out int number) )
                    return column;
                
                column += address[i];
            }

            return column;
        }

        internal static int GetRowFromCellAddress(string address)
        {
            string rowStr = "";
            for(int i = 0; i < address.Length; i++)
            {
                if(!int.TryParse(address[i].ToString(), out int number) )
                    continue;
                
                rowStr += address[i];
            }
            
            return int.Parse(rowStr.Length > 0 ? rowStr : "-1");
        }
        
        internal SearchResult[] GetTableListByRules(IRulesConfig rules, ColumnType columnType)
        {
            var sheet = GetSummarySheet();
            var list = new List<SearchResult>();
            var allCells = new List<ICell>();
            ICell? titleCell = null;
            if(sheet == null)
            {
                return Array.Empty<SearchResult>();
            }
            var rowsDictionary = new Dictionary<int, int>();
            var rows = sheet.LastRowNum;
            for(int i = 0; i < rows; i++)
            {
                var cellRow = sheet.GetRow(i);
                if(cellRow == null)
                    continue;
                
                var cellsCount = cellRow.LastCellNum;
                if(cellsCount == -1)
                    continue;
                
                rowsDictionary.Add(i, cellsCount);
            }

            foreach(var key in rowsDictionary.Keys)
            {
                for(int i = 0; i < rowsDictionary[key]; i++)
                {
                    var cellRow = sheet.GetRow(key);
                    if(cellRow == null)
                        continue;
                    
                    var cell = cellRow.GetCell(i);
                    if(cell == null)
                        continue;

                    //Program.PrintLine($"{cell.Address.FormatAsString()}: {cell.ToString()} - {cell.CellStyle.DataFormat}");
                    allCells.Add(cell);
                }
            }

            foreach(var ruleName in rules.Names)
            {
                if(allCells == null)
                    break;
                
                titleCell = allCells.FirstOrDefault(cell => GetCellValue(cell).ToLower().Contains(ruleName), null);
                if(titleCell != null)
                    break;
            }

            if(titleCell == null)
            {
                return Array.Empty<SearchResult>();
            }

            var column = titleCell.Address.Column;
            var row = titleCell.Address.Row;
            var canBreak = false;
            while(!canBreak)
            {             
                if(sheet.GetRow(++row) == null)
                    break;

                ICell? cell = sheet.GetRow(row).GetCell(column);
                if(cell == null)
                    break;
                
                string value = GetCellValue(cell);
                
                if(string.IsNullOrEmpty(value.Trim() ) )
                {
                    break;
                }

                var valueLower = value.ToLower();
                foreach(var delimiter in rules.Delimiter)
                {
                    if(!valueLower.Contains(delimiter) )
                        continue;
                    
                    canBreak = true;
                    break;
                }
                if(canBreak)
                    break;
                
                list.Add(new SearchResult{
                    SheetName = sheet.SheetName,
                    CellAddress = sheet.GetRow(row).GetCell(column).Address.FormatAsString(),
                    Content = value ?? "",
                    FilePath = FilePath,
                    Type = columnType
                } );
            }

            return list.ToArray();
        }

        internal SearchResult[] GetDates()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Date;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_DATE);
        }

        internal SearchResult[] GetFiscalNumbers()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.NF;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_NUMBER);
        }

        internal SearchResult[] GetCities()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.City;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_CITY);
        }

        internal SearchResult[] GetClients()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Client;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_CLIENT);
        }

        internal SearchResult[] GetWeights()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Weight;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_WEIGHT);
        }

        internal SearchResult[] GetVolumes()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Volumes;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_VOLUME);
        }

        internal SearchResult[] GetValues()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Value;
            return GetTableListByRules(rules, ColumnType.COLUMN_NFE_VALUE);
        }

        internal bool ContainsValidSheet()
        {
            return (GetSummarySheet() != null);
        }

        internal ISheet? GetSummarySheet()
        {
            if(!IsOLE2Format)
            {
                foreach(var validName in Config.Summary.DefaultWorksheetName)
                {
                    var sheet = XSSWorkBook.GetSheet(validName);
                    if(sheet == null)
                        continue;
                    
                    return sheet;
                }
            }
            else
            {
                foreach(var validName in Config.Summary.DefaultWorksheetName)
                {
                    var sheet = HSSWorkBook.GetSheet(validName);
                    if(sheet == null)
                        continue;
                    
                    return sheet;
                }
            }

            //Program.PrintLine($"Não foi encontrado nenhuma planilha válida no arquivo '{FilePath}'.");
            return null;
        }
    }
}