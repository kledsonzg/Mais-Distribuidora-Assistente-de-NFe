using NFeAssistant.Interface;
using NFeAssistant.Main;
using JSON = Newtonsoft.Json;

namespace NFeAssistant.Config
{

    internal class Configuration
    {
        private readonly string FilePath;
        internal IConfig Properties;
        internal Configuration()
        {
            var exePath = Environment.ProcessPath;
            if(exePath == null)
            {
                throw new ConfigFileException(new string[] {"Arquivo de configuração não encontrado. Entre em contato com o desenvolvedor para tentar encontrar uma solução."} );
            }

            var folder = Directory.GetParent(exePath);
            if(folder == null)
            {
                throw new ConfigFileException(new string[] {"Erro ao obter a pasta raíz do programa. Entre em contato com o desenvolvedor para tentar encontrar uma solução."} );
            }

            var configPathInfo = Directory.CreateDirectory($"{folder}/config");
            var configFilePath = $"{configPathInfo.FullName}/config.json";
            if(!File.Exists(configFilePath) )
            {
                CreateConfigFile(configFilePath);
            }

            var properties = JSON.JsonConvert.DeserializeObject<IConfig>(File.ReadAllText(configFilePath) );
            if(properties == null)
            {
                properties = CreateBlankConfigObject();
            }

            FilePath = configFilePath;
            Properties = properties;
        }

        internal void Save()
        {
            Save(FilePath, Properties);
        }

        internal void Save(string filePath, IConfig configuration)
        {
            var json = JSON.JsonConvert.SerializeObject(configuration, JSON.Formatting.Indented);
            Program.PrintLine("Salvando configurações...");
            File.WriteAllText(filePath, json);
            Program.PrintLine("Configurações salvas com sucesso!");
        }

        private void CreateConfigFile(string filePath)
        {
            File.Create(filePath).Dispose();

            var config = CreateBlankConfigObject();

            Save(filePath, config);
        }

        private static IConfig CreateBlankConfigObject() => new();

        internal string? GetExcelSummaryModelPath()
        {
            var configFolder = Directory.GetParent(FilePath);
            if(configFolder == null)
                return null;
            
            var folder = configFolder.Parent;
            if(folder == null)
                return null;
            
            var modelPath = $"{folder}/models/RELATÓRIO.xlsx";
            return modelPath;
        }

        internal string? GetBlackLogoImagePath()
        {
            var configFolder = Directory.GetParent(FilePath);
            if(configFolder == null)
                return null;
            
            var folder = configFolder.Parent;
            if(folder == null)
                return null;
            
            var modelPath = $"{folder}/models/black-logo-2.png";
            return modelPath;
        }

        internal string? GetVolumeIdentificationPath()
        {
            var configFolder = Directory.GetParent(FilePath);
            if(configFolder == null)
                return null;
            
            var folder = configFolder.Parent;
            if(folder == null)
                return null;

            var path = $"{folder}/Caixas Identificadas";
            Directory.CreateDirectory(path);

            return path;
        }
        
    }

    internal class ConfigFileException : FileNotFoundException
    {
        internal ConfigFileException(string[] messagesToPrint)
        {
            foreach(var message in messagesToPrint)
            {
                Console.WriteLine(message);
            }
        }
    }
}