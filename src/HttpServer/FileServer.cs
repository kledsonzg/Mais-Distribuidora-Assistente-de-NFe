using NFeAssistant.Main;

namespace NFeAssistant.HttpServer
{
    internal class FileServer
    {
        private static string clientPath = "";
        internal FileServer()
        {
            Setup();
        }

        internal byte[]? GetBytesFromFile(string localPath, bool returnIndexIfNotFound)
        {
            var filePath = $"{clientPath}{localPath}";
            if(!File.Exists(filePath) )
            {
                if(returnIndexIfNotFound)
                {
                    filePath = $"{clientPath}index.html";
                    var bytes = File.ReadAllBytes(filePath);

                    return bytes;
                }
                else return null;
            }

            return File.ReadAllBytes(filePath);
        }

        private void Setup()
        {
            var exePath = Environment.ProcessPath;
            if(exePath == null)
            {
                Program.PrintLine("Não foi possível encontrar o diretório deste software.");
                return;
            }
            
            var directoryInfo = Directory.GetParent(exePath);
            if(directoryInfo == null)
            {
                Program.PrintLine("Não foi possível encontrar o diretório deste software.");
                return;
            }
            
            clientPath = $"{directoryInfo.FullName}\\www\\";
        }
    }
}