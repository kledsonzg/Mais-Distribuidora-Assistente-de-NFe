using NFeAssistant.Interface;
using NFeAssistant.Main;
using OfficeOpenXml;

namespace NFeAssistant.ExcelBase
{
    internal class Reader
    {
        internal ExcelPackage Content;
        private string file = "";
        internal string FilePath { get{ return file; } }
        internal bool Error = false;

        private Reader()
        {
            file = "";
        }
        
        internal static Reader GetExcelReader(string pathToFile)
        {
            var reader = new Reader();

            reader.SetFilePath(pathToFile);
            return reader;
        }

        internal string GetTitle()
        {   
            var titleCell = Program.Config.Properties.Summary.DefaultTitleCell;
            var sheet = GetSummarySheet();
            if(sheet == null)
                return "";

            return sheet.Cells[titleCell].Text;
        }

        internal SearchResult[] GetListContentByRules(IRulesConfig rules)
        {
            var sheet = GetSummarySheet();
            if(sheet == null)
                return [];
            
            var list = new List<SearchResult>();
            ExcelRangeBase? cell = null;

            foreach(var ruleName in rules.Names)
            {
                cell = sheet.Cells.FirstOrDefault(element => element.Text.ToLower().Contains(ruleName), null);
                if(cell != null)
                    break;
            }

            if(cell == null)
            {
                return [];
            }

            var column = GetCellColumn(cell.AddressAbsolute);
            var row = GetCellRow(cell.AddressAbsolute);
            var canBreak = false;
            while(canBreak == false)
            {
                var value = sheet.Cells[$"{column}{++row}"].Text;
                if(string.IsNullOrEmpty(value.Trim() ) )
                {
                    break;
                }
                foreach(var delimiter in rules.Delimiter)
                {
                    //Console.WriteLine($"delimiter:{delimiter} | value: {value.ToLower() }");
                    if(!value.ToLower().Contains(delimiter) )
                        continue;
                    
                    canBreak = true;
                    break;
                }
                if(canBreak)
                    break;
                
                list.Add(new SearchResult{
                    CellAddress = sheet.Cells[$"{column}{row}"].AddressAbsolute,
                    Content = value,
                    FilePath = FilePath
                } );
            }

            // Retorna os elementos da lista em uma array.
            return [.. list];
        }

        internal SearchResult[] GetDates()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Date;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetFiscalNumbers()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.NF;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetCities()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.City;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetClients()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Client;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetWeights()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Weight;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetVolumes()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Volumes;
            return GetListContentByRules(rules);
        }

        internal SearchResult[] GetValues()
        {
            var rules = Program.Config.Properties.Summary.TableNameRules.Value;
            return GetListContentByRules(rules);
        }

        private ExcelWorksheet? GetSummarySheet()
        {
            var sheets = Content.Workbook.Worksheets.ToList();
            foreach(var sheet in sheets)
            {
                if(sheet.Name != Program.Config.Properties.Summary.DefaultWorksheetName)
                    continue;
                    
                return sheet;
            }

            return null;
        }

        internal static string GetCellColumn(string absoluteAddress)
        {
            string name = "";
            int dollarSignCount = 0;
            for(int i = 0; i < absoluteAddress.Length; i++)
            {
                if(absoluteAddress[i] == '$'){
                    dollarSignCount++;
                    continue;
                }
                if(dollarSignCount == 0)
                    continue;
                if(dollarSignCount == 2)
                    break;

                name += absoluteAddress[i];
            }

            return name;
        }

        internal static int GetCellRow(string absoluteAddress)
        {
            string name = "";
            int dollarSignCount = 0;
            for(int i = 0; i < absoluteAddress.Length; i++)
            {
                if(absoluteAddress[i] == '$'){
                    dollarSignCount++;
                    continue;
                }
                if(dollarSignCount != 2)
                    continue;

                name += absoluteAddress[i];
            }

            return int.Parse(name);
        }

        private void SetFilePath(string pathToFile)
        {
            if(!File.Exists(pathToFile) )
            {
                Error = true;
                return;
            }
            
            var fileExtension = Path.GetExtension(pathToFile);
            if(fileExtension == null)
            {
                Error = true;
                return;
            }
            fileExtension = fileExtension.ToLower();
            if(fileExtension != ".xls" && fileExtension != ".xlsx")
            {
                Error = true;
                return;
            }

            Content = new ExcelPackage(pathToFile);
            file = pathToFile; 
        }
    }
}