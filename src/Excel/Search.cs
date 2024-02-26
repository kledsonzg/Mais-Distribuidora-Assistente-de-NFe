using NFeAssistant.Interface;
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
            
            Console.WriteLine($"peso:{searchContent.ToSearch.Weight},valor:{searchContent.ToSearch.Value},cliente:{searchContent.ToSearch.Client},cidade:{searchContent.ToSearch.City},volumes:{searchContent.ToSearch.Volumes},nfe:{searchContent.ToSearch.NfeNumber},transportadora:{searchContent.ToSearch.ShippingCompany},data:{searchContent.ToSearch.Date}");

            int workersCount = 0;
            List<ISearchResult> results = new();
            foreach(var folder in summaryFolders)
            {
                var workerThread = new Thread(new ThreadStart(delegate { results.AddRange(SearchInSummaries(folder, searchContent.ToSearch) ); workersCount--; } ) );
                workersCount++;
                workerThread.Start();
            }

            while(workersCount > 0)
                Thread.Sleep(1000);
            Program.PrintLine("results count: " + results.Count);

            return JSON.JsonConvert.SerializeObject(results, JSON.Formatting.Indented);
        }

        private static ISearchResult[] SearchInSummaries(string folder, ISearchRequestContent searchContent)
        {
            List<ISearchResult> validResults = new();
            List<DirectoryInfo> directoriesToRead = new();
            List<FileInfo> files = new();
            DirectoryInfo folderInfo = new DirectoryInfo(folder);

            var date = searchContent.GetDateTime();
            directoriesToRead.Add(folderInfo);

            for(int i = 0; i < directoriesToRead.Count; i++)
            {
                var pathInfo = directoriesToRead[i];
                foreach(var subdirectory in pathInfo.GetDirectories() )
                {
                    directoriesToRead.Add(subdirectory);
                }
            }

            string[] supportedFiles = [".xls", ".xlsx"];
            foreach(var directory in directoriesToRead)
            {
                foreach(var file in directory.GetFiles() )
                {             
                    Program.PrintLine(file.FullName);
                    if(file.CreationTime < date || !supportedFiles.Contains(file.Extension.ToLower() ) )
                        continue;

                    files.Add(file);
                }
            }
         
            int threadsCount = 0;
            foreach(var file in files)
            {
                var workerThread = new Thread(new ThreadStart(delegate 
                {
                    List<SearchResult> results = new();
                    try
                    {
                        var reader = Reader.GetExcelReader(file.FullName);

                        if(reader.GetTitle().ToLower().Contains(searchContent.ShippingCompany.ToLower() ) == false)
                        {
                            threadsCount--;
                            return;
                        }

                        var dates = reader.GetDates().ToList();
                        var numbers = reader.GetFiscalNumbers().ToList();
                        var weights = reader.GetWeights().ToList();
                        var clients = reader.GetClients().ToList();
                        var cities = reader.GetCities().ToList();
                        var values = reader.GetValues().ToList();
                        var volumes = reader.GetVolumes().ToList();

                        foreach(var result in dates)
                        {
                            if(result.Content.Contains(searchContent.Date) )
                                results.Add(result);
                        }
                        foreach(var result in numbers)
                        {
                            if(result.Content.Contains(searchContent.NfeNumber) )
                                results.Add(result);
                        }
                        foreach(var result in weights)
                        {
                            if(result.Content.Contains(searchContent.Weight) )
                                results.Add(result);
                        }
                        foreach(var result in clients)
                        {
                            if(result.Content.ToLower().Contains(searchContent.Client.ToLower() ) )
                                results.Add(result);
                        }
                        foreach(var result in cities)
                        {
                            if(result.Content.ToLower().Contains(searchContent.City.ToLower() ) )
                                results.Add(result);
                        }
                        foreach(var result in values)
                        {
                            if(result.Content.Contains(searchContent.Value) )
                                results.Add(result);
                        }
                        foreach(var result in volumes)
                        {
                            if(result.Content.Contains(searchContent.Volumes) )
                                results.Add(result);
                        }

                        // Validar se os resultados estão na mesma linha.
                        var validationDictionary = new Dictionary<int, int>();
                        
                        foreach(var result in results)
                        {
                            int rowIndex = Reader.GetCellRow(result.CellAddress);
                            if(!validationDictionary.ContainsKey(rowIndex) )
                                validationDictionary.Add(rowIndex, 0);
                            
                            validationDictionary[rowIndex]++;

                            // Significa que uma linha possui todos os requisitos da pesquisa.
                            if(validationDictionary[rowIndex] >= 7)
                            {
                                validResults.Add(new ISearchResult{ RowIndex = rowIndex, FilePath = file.FullName } );
                            }
                        }
                    }
                    catch(Exception exception)
                    {
                        Program.PrintLine($"Houve um erro durante a leitura do arquivo: {file.FullName} | Motivo: {exception.Message} | Detalhes: {exception.StackTrace}");
                    }

                    threadsCount--;
                } ) );

                threadsCount++;
                workerThread.Start();
            }                       

            while(threadsCount != 0)
                Thread.Sleep(1000);
            
            return validResults.ToArray();
        }
    }
}