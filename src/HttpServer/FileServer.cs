using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
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

        internal static byte[]? GetBytesFromFile(string localPath, bool returnIndexIfNotFound)
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

        internal static byte[]? GetFolderDirectories(string path)
        {
            try
            {
                var directories = path == "0" ? Directory.GetLogicalDrives() :  Directory.GetDirectories(path);
                var json = JsonConvert.SerializeObject(directories);

                return Encoding.UTF8.GetBytes(json);
            }
            catch
            {
                return null;
            }
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