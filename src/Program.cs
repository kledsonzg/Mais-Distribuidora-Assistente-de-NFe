using System;
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
            Run();

            Config.Save();
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
