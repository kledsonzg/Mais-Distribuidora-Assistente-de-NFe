using NFeAssistant.Interface;
using NFeAssistant.Logger;
using NFeAssistant.Main;
using JSON = Newtonsoft.Json;

namespace NFeAssistant.ExcelBase
{
    internal static class Search
    {
        internal static string ResultFromSearchRequestContent(string jsonContent)
        {
            var summaryFolders = Program.Config.Properties.App.SummaryPath;
            var searchContent = JSON.JsonConvert.DeserializeObject<ISearchJSONBody>(jsonContent);
            
            //Console.WriteLine($"peso:{searchContent.ToSearch.Weight.Value},valor:{searchContent.ToSearch.Value.Value},cliente:{searchContent.ToSearch.Client.Value},cidade:{searchContent.ToSearch.City.Value},volumes:{searchContent.ToSearch.Volumes.Value},nfe:{searchContent.ToSearch.NfeNumber.Value},transportadora:{searchContent.ToSearch.ShippingCompany.Value},data:{searchContent.ToSearch.Date.Value}");

            List<ISearchResult> results = new();

            var resultArray = SearchInSummaries(searchContent.ToSearch);
            results.AddRange(resultArray);

            Program.PrintLine("TOTAL DE RESULTADOS: " + results.Count);

            return JSON.JsonConvert.SerializeObject(results, JSON.Formatting.Indented);
        }

        private static ISearchResult[] SearchInSummaries(ISearchRequestContent searchContent)
        {
            List<ISearchResult> validResults = new();
            List<FileInfo> files = FileListUpdater.Updater.GetExcelSheetFiles();

            var date = searchContent.GetDateTime();
            var invalidDate = new DateTime(1, 1, 1);

            for(int i = 0; i < files.Count && files.Count > 0; i++)
            {
                var file = files[i];

                var creationTime = file.CreationTime > file.LastWriteTime ? file.LastWriteTime : file.CreationTime;

                if(searchContent.SummaryDateFilter.FromDate != invalidDate)
                {
                    if( !(creationTime >= searchContent.SummaryDateFilter.FromDate) )
                    {
                        files.Remove(file);
                        i = -1;
                        continue;
                    }
                }
                if(searchContent.SummaryDateFilter.ToDate != invalidDate)
                {
                    if( !(creationTime < searchContent.SummaryDateFilter.ToDate.AddDays(1f)) )
                    {
                        files.Remove(file);
                        i = -1;
                        continue;
                    }
                }
                
                if(creationTime < date)
                {
                    files.Remove(file);
                    i = -1;
                    continue;
                }
                
                // Arquivo gerado pelo excel temporariamente como uma forma de backup (Acredito eu)
                if(file.Name.IndexOf("~$") == 0)
                {
                    files.Remove(file);
                    i = -1;
                    continue;
                }
            }

            Parallel.ForEach(files, (file) =>
            {
                SearchAndAddFromSummarie(file, searchContent, validResults);
            } );

            Program.PrintLine($"TOTAL DE ARQUIVOS: {files.Count}.");      
            return validResults.ToArray();
        }

        static void SearchAndAddFromSummarie(FileInfo file, ISearchRequestContent searchContent, List<ISearchResult> validResults)
        {
            var results = new List<SearchResult>();
            var summary = NFeAssistant.Cache.Cache.SummaryList.FirstOrDefault(_summary => _summary.FilePath.Replace('/', '\\') == file.FullName);
            if(summary == null)
                return;         

            if(searchContent.ShippingCompany.Precise)
            {
                if(searchContent.ShippingCompany.Value.ToLower().Contains(summary.Title.ToLower() ) )
                {
                    Logger.Logger.Write($"Não foi encontrado planilhas com a transportadora procurada. Arquivo: '{file.FullName}' | Transportadora procurada: '{searchContent.ShippingCompany.Value}' | Titulo: {summary.Title}");
                    return;
                }
            }
            else
            {
                if(summary.Title.ToLower().Contains(searchContent.ShippingCompany.Value.ToLower() ) == false)
                {
                    Logger.Logger.Write($"Não foi encontrado planilhas com a transportadora procurada. Arquivo: '{file.FullName}' | Transportadora procurada: '{searchContent.ShippingCompany.Value}' | Titulo: {summary.Title}");
                    return;
                }
            }

            var dates = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_DATE);
            var numbers = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_NUMBER);
            var weights = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_WEIGHT);
            var clients = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_CLIENT);
            var cities = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_CITY);;
            var values = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_VALUE);
            var volumes = summary.Contents.Where(content => content.Type == Definitions.Excel.ColumnType.COLUMN_NFE_VOLUME);

            foreach(var result in dates)
            {                    
                if(searchContent.Date.Precise)
                {
                    if(searchContent.Date.Value.Contains(result.Content) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.Contains(searchContent.Date.Value) )
                    results.Add(result);
            }
            foreach(var result in numbers)
            {
                if(searchContent.NfeNumber.Precise)
                {
                    if(searchContent.NfeNumber.Value.Contains(result.Content) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.Contains(searchContent.NfeNumber.Value) )
                    results.Add(result);
            }
            foreach(var result in weights)
            {
                if(searchContent.Weight.Precise)
                {
                    if(searchContent.Weight.Value.Contains(result.Content) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.Contains(searchContent.Weight.Value) )
                    results.Add(result);
            }
            foreach(var result in clients)
            {
                if(searchContent.Client.Precise)
                {
                    if(searchContent.Client.Value.ToLower().Contains(result.Content.ToLower() ) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.ToLower().Contains(searchContent.Client.Value.ToLower() ) )
                    results.Add(result);
            }
            foreach(var result in cities)
            {
                if(searchContent.City.Precise)
                {
                    if(searchContent.City.Value.ToLower().Contains(result.Content.ToLower() ) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.ToLower().Contains(searchContent.City.Value.ToLower() ) )
                    results.Add(result);
            }
            foreach(var result in values)
            {
                if(searchContent.Value.Precise)
                {
                    if(searchContent.Value.Value.Contains(result.Content) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.Contains(searchContent.Value.Value) )
                    results.Add(result);
            }
            foreach(var result in volumes)
            {
                if(searchContent.Volumes.Precise)
                {
                    if(searchContent.Volumes.Value.Contains(result.Content) )
                        results.Add(result);
                    
                    continue;
                }

                if(result.Content.Contains(searchContent.Volumes.Value) )
                    results.Add(result);
            }

            // Validar se os resultados estão na mesma linha.
            var validationDictionary = new Dictionary<int, int>();                      
            
            foreach(var result in results)
            {
                int rowIndex = ExcelFile.GetRowFromCellAddress(result.CellAddress);
                if(!validationDictionary.ContainsKey(rowIndex) )
                    validationDictionary.Add(rowIndex, 0);
                
                validationDictionary[rowIndex]++;

                // Significa que uma linha possui todos os requisitos da pesquisa.
                if(validationDictionary[rowIndex] >= 7)
                {
                    var resultsFromRow = results.Where(predicate => ExcelFile.GetRowFromCellAddress(predicate.CellAddress) == rowIndex).ToList();

                    var date = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_DATE);
                    var number = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_NUMBER);
                    var client = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_CLIENT);
                    var city = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_CITY);
                    var volume = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_VOLUME);
                    var weight = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_WEIGHT);
                    var value = resultsFromRow.Find(predicate => predicate.Type == Definitions.Excel.ColumnType.COLUMN_NFE_VALUE);

                    if(date == null || number == null || client == null || city == null || volume == null || weight == null || value == null)
                        continue;
                    
                    string content = $"{date.Content}\t{number.Content}\t{client.Content}\t{city.Content}\t{volume.Content}\t{weight.Content}\t{value.Content}";
                    lock(validResults)
                    {
                        validResults.Add(new ISearchResult{ RowIndex = rowIndex, FilePath = file.FullName, Content = content, SheetName = date.SheetName } );
                    }                          
                }
            }
        }
    }
}