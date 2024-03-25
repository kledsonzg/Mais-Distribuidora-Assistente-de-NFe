using Newtonsoft.Json;
using NFeAssistant.Interface;
using NFeAssistant.Invoice;
using NFeAssistant.Logger;
using NFeAssistant.Main;

namespace NFeAssistant.Xml;

internal static class SummaryGenerator
{
    internal static string? GenerateResult(string requestBody)
    {
        var filter = JsonConvert.DeserializeObject<ISummaryGenerationJSONBody>(requestBody);

        if(filter == null)
            return null;

        //Program.PrintLine(filter.XmlDateFilter.FromDate.ToShortDateString() );

        if(!Directory.Exists(filter.OutputFolder) )
            return null;
        
        var xmlPaths = Program.Config.Properties.App.XmlPath;
        var threadList = new List<Thread>();
        var rows = new List<ExcelRow>();

        foreach(var path in xmlPaths)
        {
            var thread = new Thread(new ThreadStart(delegate 
            {
                GetResultFromXmlFolder(path, filter, rows);
            } ) );
            
            threadList.Add(thread);
            thread.Start();
        }

        threadList.ForEach(thread => thread.Join() );
        var result = new ISummaryResult(){
            OutputFolder = filter.OutputFolder,
            Rows = rows.ToArray()
        };
        return JsonConvert.SerializeObject(result);
    }

    private static void GetResultFromXmlFolder(string folder, ISummaryGenerationJSONBody filter, List<ExcelRow> rows)
    {
        var files = Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories);
        var threadList = new List<Thread>();
        var fileList = new List<string>();
        var rowsToAdd = new List<ExcelRow>();
        var invalidDate = new DateTime(1, 1, 1);

        foreach(var file in files)
        {
            var creationTime = File.GetCreationTime(file);
            var modificationTime = File.GetLastWriteTime(file);       

            if(modificationTime < creationTime)
                creationTime = modificationTime;
            
            if(filter.XmlDateFilter.FromDate != invalidDate)
            {
                if( !(creationTime >= filter.XmlDateFilter.FromDate) )
                    continue;
            }
            if(filter.XmlDateFilter.ToDate != invalidDate)
            {
                if( !(creationTime < filter.XmlDateFilter.ToDate.AddDays(1) ) )
                    continue;
            }
            
            fileList.Add(file);
        }

        foreach(var file in fileList)
        {
            var thread = new Thread(new ThreadStart(delegate
            {
                try
                {
                    var invoice = Invoice.GetFromXMLFile(file);
                    if(invoice == null || !invoice.NoFails)
                    {
                        Logger.Logger.Write($"Houve um erro no arquivo: '{file}' - " + invoice == null ? "Instância nula." : "Instância com erros.");
                        return;
                    }

                    if(filter.EmissionDateFilter.FromDate != invalidDate)
                    {
                        if( !(invoice.Emission >= filter.EmissionDateFilter.FromDate) )
                            return;
                    }
                    if(filter.EmissionDateFilter.ToDate != invalidDate)
                    {
                        if( !(invoice.Emission < filter.EmissionDateFilter.ToDate.AddDays(1) ) )
                            return;
                    }

                    if(filter.ShippingCompany.Precise)
                    {
                        if(filter.ShippingCompany.Value.ToLower().Contains(invoice.ShippingCompany.Nome.ToLower() ) == false)
                            return;
                    }
                    else if(invoice.ShippingCompany.Nome.ToLower().Contains(filter.ShippingCompany.Value.ToLower() ) == false)
                        return;
                    
                    if(filter.Volumes.Precise)
                    {
                        if(filter.Volumes.Value.Contains(invoice.Volumes.ToString() ) == false)
                            return;
                    }
                    else if(invoice.Volumes.ToString().Contains(filter.Volumes.Value ) == false)
                        return;

                    if(filter.Weight.Precise)
                    {
                        if(filter.Weight.Value.Contains(invoice.Weight.ToString() ) == false)
                            return;
                    }
                    else if(invoice.Weight.ToString().Contains(filter.Weight.Value ) == false)
                        return;
                    
                    var row = new ExcelRow
                    {
                        NF = invoice.NumberCode,
                        EmissionDate = invoice.Emission.ToShortDateString(),
                        Client = invoice.Client.Nome,
                        City = invoice.Client.Cidade,
                        Volumes = invoice.Volumes.ToString(),
                        Weight = invoice.Weight.ToString(),
                        Value = invoice.Value.Total.ToString()
                    };

                    lock(rowsToAdd)
                    {
                        rowsToAdd.Add(row);
                    }
                }
                catch(Exception e)
                {
                    Logger.Logger.Write($"Houve uma exceção no arquivo: '{file}'. Motivo: {e.Message} | Pilhas: {e.StackTrace}");
                }

            } ) );

            threadList.Add(thread);
            thread.Start();
        }

        threadList.ForEach(thread => thread.Join() );
        
        lock(rows)
        {
            rows.AddRange(rowsToAdd);
        }
    }
}