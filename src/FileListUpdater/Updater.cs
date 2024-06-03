using NFeAssistant.Cache;
using NFeAssistant.Main;

namespace NFeAssistant.FileListUpdater
{
    
    internal static class Updater
    {
        private static List<FileInfo> _xmlFileList = new();
        private static List<FileInfo> _excelFileList = new();
        private static List<FileSystemWatcher> _fileWatcherList = new();
        private static FileDateComparer _comparer = new();

        internal static void Start()
        {
            int[] steps = new int[]{0, 4};
            Program.PrintLine($"(PASSO {++steps[0]}/{steps[1] }) Buscando arquivos XML e Planilhas...");
            FillFileLists();
            Program.PrintLine("Busca concluída!");
            Program.PrintLine($"Total de arquivos 'XML' encontrados: {_xmlFileList.Count}.");
            Program.PrintLine($"Total de planilhas encontradas: {_excelFileList.Count}.");

            Program.PrintLine($"(PASSO {++steps[0]}/{steps[1] }) Inicializando os verificadores de pastas...");
            InitializeFileSystemWatchers();
            Program.PrintLine("Verificadores prontos!");

            Program.PrintLine($"(PASSO {++steps[0]}/{steps[1] }) Inserindo relatórios na memória...");
            InsertSummariesIntoCache();
            Program.PrintLine($"(PASSO {++steps[0]}/{steps[1] }) Inserindo Notas Fiscais na memória...");
            InsertInvoicesIntoCache();

            Program.PrintLine("Processo de cache concluído!");
            Database.Invoice.SaveAll();
        }

        private static void InsertSummariesIntoCache()
        {
            Parallel.ForEach(_excelFileList, (fileInfo) => 
            {
                bool result = false;

                result = InsertSummaryIntoCacheInSafeMode(fileInfo);

                if(!result)
                {
                    Logger.Logger.Write($"Falha ao inserir o seguinte arquivo (relatório) na memória:\nArquivo: {fileInfo.FullName}");
                }
            } );
        }

        private static bool InsertSummaryIntoCacheInSafeMode(FileInfo fileInfo)
        {
            bool result = false;
            if(fileInfo.Name.IndexOf("~$") == 0)
                return true;
            
            try
            {
                result = NFeAssistant.Cache.Cache.InsertSummaryFileIntoCache(fileInfo);
            }
            catch(IOException exception)
            {
                bool solved = false;
                try
                {
                    var tempFolder = $"{Path.GetTempPath()}/KledsonZG/Assistente de NFe";
                    var fileDest = $"{tempFolder}/{Path.GetFileName(fileInfo.FullName)}";
                    File.Copy(fileInfo.FullName, fileDest, true);
                    result = NFeAssistant.Cache.Cache.InsertSummaryFileIntoCache(new FileInfo(fileDest), fileInfo.FullName);
                    solved = true;
                }
                catch(Exception exceptionFromCopyFile)
                {
                    Logger.Logger.Write($"Erro durante processo de cópia de um relatório para fazer a leitura. Arquivo: {fileInfo.FullName} | Motivo: {exceptionFromCopyFile.Message} | Detalhes: {exceptionFromCopyFile.StackTrace}");
                }
                if(!solved)
                {
                    Logger.Logger.Write($"Houve um erro durante a leitura do arquivo: {fileInfo.FullName} | Tipo de exceção: {exception.GetBaseException()}");
                }
            }
            catch(Exception exception)
            {
                Logger.Logger.Write($"Houve um erro durante a leitura do arquivo: {fileInfo.FullName} | Tipo de exceção: {exception.GetBaseException()}");
            }

            return result;
        }

        private static bool InsertInvoiceIntoCacheInSafeMode(FileInfo fileInfo)
        {
            try
            {
                NFeAssistant.Cache.Cache.InsertInvoiceFileIntoCache(fileInfo);
            }
            catch(Exception exception)
            {
                Logger.Logger.Write($"Houve um erro durante a leitura do arquivo: {fileInfo.FullName} | Tipo de exceção: {exception.GetBaseException()}");
                return false;
            }

            return true;
        }

        private static void InsertInvoicesIntoCache()
        {
            // Antes de criar os objetos de nota fiscal através dos arquivos '.xml', podemos economizar o tempo de leitura dos arquivos obtendo as informações das notas fiscais que já estão no banco de dados.
            Cache.Cache.InsertInvoicesIntoCache(Database.Invoice.GetAll() );
            // Agora que obtemos os objetos do banco de dados, podemos remover os arquivos que já se tornaram objetos da lista de 'FileInfo(s)'.
            List<FileInfo> list = new(_xmlFileList);
            foreach(var invoice in Cache.Cache.InvoiceList)
            {
                list.RemoveAll(filter => filter.FullName == new FileInfo(invoice.FilePath).FullName );
            }

            Parallel.ForEach(list, (fileInfo) => 
            {
                bool result = InsertInvoiceIntoCacheInSafeMode(fileInfo);
                if(!result)
                {
                    Logger.Logger.Write($"Falha ao inserir o seguinte arquivo (Nota Fiscal) na memória:\nArquivo: {fileInfo.FullName}");
                }
            } );
        }

        private static void FillFileLists()
        {
            foreach(var path in Program.Config.Properties.App.XmlPath)
            {
                var files = Directory.GetFiles(path, "*.xml", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    _xmlFileList.Add(new FileInfo(file) );
                }
            }

            foreach(var path in Program.Config.Properties.App.SummaryPath)
            {
                var files = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    _excelFileList.Add(new FileInfo(file) );
                }
                files = Directory.GetFiles(path, "*.xls", SearchOption.AllDirectories);
                foreach(var file in files)
                {
                    _excelFileList.Add(new FileInfo(file) );
                }
            }

            SortList(_xmlFileList);
            SortList(_excelFileList);
        }

        private static void SortList(List<FileInfo> list)
        {
            list.Sort(_comparer);
        }

        private static void InitializeFileSystemWatchers()
        {
            foreach(var path in Program.Config.Properties.App.XmlPath)
            {            
                var watcher = new FileSystemWatcher()
                {
                    Path = path,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    Filter = "*.xml",
                    NotifyFilter = NotifyFilters.Attributes
                        | NotifyFilters.CreationTime
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastAccess
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Security
                        | NotifyFilters.Size      
                };

                watcher.Renamed += OnFileRename;
                watcher.Created += OnFileCreate;
                watcher.Deleted += OnFileDelete;
                watcher.Changed += OnFileUpdate;
                _fileWatcherList.Add(watcher);
            }
            foreach(var path in Program.Config.Properties.App.SummaryPath)
            {
                var watcher = new FileSystemWatcher()
                {
                    Path = path,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    Filter = "*.xlsx",
                    NotifyFilter = NotifyFilters.Attributes
                        | NotifyFilters.CreationTime
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastAccess
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Security
                        | NotifyFilters.Size      
                };

                watcher.Renamed += OnFileRename;
                watcher.Created += OnFileCreate;
                watcher.Deleted += OnFileDelete;
                watcher.Changed += OnFileUpdate;
                _fileWatcherList.Add(watcher);

                watcher = new FileSystemWatcher()
                {
                    Path = path,
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true,
                    Filter = "*.xls",
                    NotifyFilter = NotifyFilters.Attributes
                        | NotifyFilters.CreationTime
                        | NotifyFilters.DirectoryName
                        | NotifyFilters.FileName
                        | NotifyFilters.LastAccess
                        | NotifyFilters.LastWrite
                        | NotifyFilters.Security
                        | NotifyFilters.Size
                };

                watcher.Renamed += OnFileRename;
                watcher.Created += OnFileCreate;
                watcher.Deleted += OnFileDelete;
                watcher.Changed += OnFileUpdate;
                _fileWatcherList.Add(watcher);
            }
        }

        private static void OnFileUpdate(object sender, FileSystemEventArgs e)
        {                
            NFeAssistant.Cache.Cache.RemoveContentFromCacheByFile(new FileInfo(e.FullPath) );

            switch(Path.GetExtension(e.FullPath).ToLower() )
            {
                case ".xml":
                {
                    InsertInvoiceIntoCacheInSafeMode(new FileInfo(e.FullPath) );
                    Database.Invoice.SaveAll();
                    break;
                }
                case ".xlsx" : case ".xls":
                {
                    InsertSummaryIntoCacheInSafeMode(new FileInfo(e.FullPath) );
                    break;
                }
                default: return;
            }         
        }
        
         private static void OnFileRename(object sender, RenamedEventArgs e)
        {
            var oldExtension = Path.GetExtension(e.OldFullPath).ToLower();
            var extension = Path.GetExtension(e.FullPath).ToLower();

            FileInfo oldFile = new(e.OldFullPath);
            FileInfo file = new(e.FullPath);

            switch(oldExtension)
            {
                case ".xml":
                {
                    lock(_xmlFileList)
                    {
                        _xmlFileList.RemoveAll(fileInfo => fileInfo.FullName == oldFile.FullName);
                    }
                    break;
                }
                case ".xlsx" : case ".xls":
                {
                    lock(_excelFileList)
                    {
                        _excelFileList.RemoveAll(fileInfo => fileInfo.FullName == oldFile.FullName);
                    } 
                    break;
                }
            }
            
            NFeAssistant.Cache.Cache.RemoveContentFromCacheByFile(oldFile);

            switch(extension)
            {
                case ".xml":
                {
                    lock(_xmlFileList)
                    {
                        _xmlFileList.Add(file);
                        SortList(_xmlFileList);
                    }

                    InsertInvoiceIntoCacheInSafeMode(file);
                    Database.Invoice.SaveAll();
                    break;
                }
                case ".xlsx" : case ".xls":
                {                
                    lock(_excelFileList)
                    {
                        _excelFileList.Add(file);
                        SortList(_excelFileList);
                    }

                    InsertSummaryIntoCacheInSafeMode(file);
                    break;
                }
            }
        }

        private static void OnFileDelete(object sender, FileSystemEventArgs e)
        {
            List<FileInfo> fileList;

            switch(Path.GetExtension(e.FullPath).ToLower() )
            {
                case ".xml":
                {
                    fileList = _xmlFileList;
                    break;
                }
                case ".xlsx" : case ".xls":
                {
                    fileList = _excelFileList;
                    break;
                }
                default: return;
            }
                
            lock(fileList)
            {
                string oldFilePath = e.FullPath.Replace('/', '\\');
                int index = fileList.FindIndex(file => file.FullName == oldFilePath);
                if(index == -1)
                    return;
                
                fileList.RemoveAt(index);
            }

            NFeAssistant.Cache.Cache.RemoveContentFromCacheByFile(new FileInfo(e.FullPath) );
        }

        private static void OnFileCreate(object sender, FileSystemEventArgs e)
        {
            Program.PrintLine($"Arquivo criado: {e.FullPath}");
            Thread newFileThread = new(new ThreadStart(delegate 
            {
                List<FileInfo> fileList;
                // Espera 5 segundos antes do arquivo ser lido.
                Thread.Sleep(5000);
                NFeAssistant.Cache.Cache.RemoveContentFromCacheByFile(new FileInfo(e.FullPath) );
                switch(Path.GetExtension(e.FullPath).ToLower() )
                {
                    case ".xml":
                    {
                        fileList = _xmlFileList;
                        InsertInvoiceIntoCacheInSafeMode(new FileInfo(e.FullPath) );
                        Database.Invoice.SaveAll();
                        break;
                    }
                    case ".xlsx" : case ".xls":
                    {                 
                        fileList = _excelFileList;
                        InsertSummaryIntoCacheInSafeMode(new FileInfo(e.FullPath) );
                        break;
                    }
                    default: return;
                }
                    
                lock(fileList)
                {
                    fileList.Add(new FileInfo(e.FullPath) );
                    SortList(fileList);
                }
            } ) );

            newFileThread.Start(); 
        }

        internal static List<FileInfo> GetXMLFiles() => new List<FileInfo>(_xmlFileList);
        internal static List<FileInfo> GetExcelSheetFiles() => new List<FileInfo>(_excelFileList);
    }
}