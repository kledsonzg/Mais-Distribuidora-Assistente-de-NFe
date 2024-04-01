using NFeAssistant.Config;
using NFeAssistant.Testing;
using NFeAssistant.HttpServer;

namespace NFeAssistant.Main
{
    public static class Program
    {
        internal static Configuration Config = new ();
        private static void Main()
        {
            Console.WriteLine("Assistente de NFe por KledsonZG");

            Test.Run();
            try
            {
                Run();
            }
            catch(Exception e)
            {
                Logger.Logger.Write($"O programa foi encerrado devido a uma exceção: {e.Message} | Pilha: {e.StackTrace}");
            }

            try
            {
                Config.Save();
            }
            catch(Exception e)
            {
                Logger.Logger.Write($"Ocorreu um erro durante a gravação das configurações! Verifique se houve alguma corrupção de informação.\n Mensagem da exceção: {e.Message} | Pilha: {e.StackTrace}");
            }          
        }

        private static void Run()
        {
            PrintLine("Em execução!");
            Logger.Logger.Start();
            Server.Start();
        }

        public static void PrintLine(string? value)
        {
            if(value == null)
            {
                Console.WriteLine();
                return;
            }
            
            Console.WriteLine($"{DateTime.Now.ToShortDateString()} - {DateTime.Now.ToLongTimeString()}: {value}");
        }
    }
}
