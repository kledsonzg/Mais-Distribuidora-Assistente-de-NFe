using NFeAssistant.Main;

namespace NFeAssistant.FileListUpdater
{
    
    internal static class Updater
    {
        private static List<FileInfo> _xmlFileList = new();
        private static List<FileInfo> _excelFileList = new();
        private static FileDateComparer _comparer = new();

        internal static void Start()
        {
            Program.PrintLine("Buscando arquivos XML e Planilhas...");
            FillFileLists();
            Program.PrintLine("Busca concluída!");
            Program.PrintLine($"Total de arquivos 'XML' encontrados: {_xmlFileList.Count}.");
            Program.PrintLine($"Total de planilhas encontradas: {_excelFileList.Count}.");

            Program.PrintLine("Inicializando os verificadores de pastas...");
            InitializeFileSystemWatchers();
            Program.PrintLine("Verificadores prontos!");
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
            }
        }

        private static void OnFileRename(object sender, RenamedEventArgs e)
        {
            List<FileInfo> fileList;
            switch(Path.GetExtension(e.OldFullPath).ToLower() )
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
                
            {
                lock(fileList)
                {
                    int index = fileList.FindIndex(file => file.FullName == e.OldFullPath);
                    if(index == -1)
                        return;
                    
                    fileList[index] = new FileInfo(e.FullPath);
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
                
            {
                lock(fileList)
                {
                    int index = fileList.FindIndex(file => file.FullName == e.FullPath);
                    if(index == -1)
                        return;
                    
                    fileList.RemoveAt(index);
                }
            }
        }

        private static void OnFileCreate(object sender, FileSystemEventArgs e)
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
                
            {
                lock(fileList)
                {
                    fileList.Add(new FileInfo(e.FullPath) );
                    SortList(fileList);
                }
            }
        }

        internal static List<FileInfo> GetXMLFiles() => new List<FileInfo>(_xmlFileList);
        internal static List<FileInfo> GetExcelSheetFiles() => new List<FileInfo>(_excelFileList);
    }
}