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
            Test.Run();
            Run();
            Config.Save();
        }

        private static void Run()
        {
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
