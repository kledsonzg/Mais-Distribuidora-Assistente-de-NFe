using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Unicode;
using NFeAssistant.Main;

namespace NFeAssistant.Logger
{
    internal static class Logger
    {
        static bool inUse = false;
        static bool dataAppended = false;
        static FileStream? stream;
        internal static void Start()
        {
            var exePath = Environment.ProcessPath;
            if(exePath == null)
            {
                Program.PrintLine($"Houve um erro durante o processo de inicialização da classe 'Logger'. exePath: {exePath}");
                return;
            }

            var folder = Directory.GetParent(exePath);
            if(folder == null)
            {
                Program.PrintLine($"Houve um erro durante o processo de inicialização da classe 'Logger'. folder: {folder}");
                return;
            }
            var now = DateTime.Now;
            var month = now.Month < 10 ? "0" + now.Month : now.Month.ToString();
            var day = now.Day < 10 ? "0" + now.Day : now.Day.ToString();
            var hour = now.Hour < 10 ? "0" + now.Hour : now.Hour.ToString();
            var minute = now.Minute < 10 ? "0" + now.Minute : now.Minute.ToString();
            var second = now.Second < 10 ? "0" + now.Second : now.Second.ToString();

            var file = $"{folder}/log/{now.Year}-{month}-{day}_{hour}-{minute}-{second}.txt";
            var fileFolder = Directory.GetParent(file);

            if(fileFolder == null)
            {
                Program.PrintLine($"Houve um erro durante o processo de inicialização da classe 'Logger'. fileFolder: {fileFolder}");
                return;
            }

            Directory.CreateDirectory(fileFolder.FullName);
            stream = File.Create(file, 8192);
            
            StartSaveInFileThread();
        }

        internal static bool Write(string? text)
        {
            if(stream == null || text == null)
                return false;
            
            while(inUse)
            {
                Thread.Sleep(1000);
            }

            inUse = true;      
            text = $"{DateTime.Now.ToShortDateString()} - {DateTime.Now.ToLongTimeString()}: {text}\n";

            var bytes = Encoding.UTF8.GetBytes(text);

            lock(stream)
            {
                stream.Write(bytes);
            }

            dataAppended = true;
            inUse = false;
            return true;
        }

        private static void StartSaveInFileThread()
        {
            var thread = new Thread(new ThreadStart(delegate {
                while(true)
                {
                    while(inUse)
                    {
                        Thread.Sleep(1000);
                    }
                    inUse = true;

                    if(stream == null)
                    {
                        inUse = false;
                        Program.PrintLine("'stream' está nulo! Não foi possível prosseguir com o salvamento de dados.");
                        break;
                    }

                    if(dataAppended)
                    {
                        Program.PrintLine("Salvando dados...");
                        lock(stream)
                        {
                            var file = stream.Name;
                            stream.Dispose();
                            
                            stream = new FileStream(file, FileMode.Append);
                            dataAppended = false;
                        }
                        Program.PrintLine("Log salvo com sucesso! Um novo stream para o arquivo de log foi criado.");
                    }             
                    inUse = false;

                    Thread.Sleep(5000);
                }
            } ) );

            thread.Start();
        }
    }
}