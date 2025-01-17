using Newtonsoft.Json;
using NFeAssistant.Interface;
using NFeAssistant.Invoice;
using NFeAssistant.Main;

namespace NFeAssistant.Xml;

internal static class SummaryGenerator
{
    internal static string? GenerateResult(string requestBody)
    {
        var filter = JsonConvert.DeserializeObject<ISummaryGenerationJSONBody>(requestBody);

        if(filter == null)
            return null;

        if(!Directory.Exists(filter.OutputFolder) )
            return null;

        var rows = new List<ExcelRow>();

        GetResultFromXmlFolder(filter, rows);

        var result = new ISummaryResult(){
            OutputFolder = filter.OutputFolder,
            Rows = rows.ToArray()
        };
        return JsonConvert.SerializeObject(result);
    }

    private static void GetResultFromXmlFolder(ISummaryGenerationJSONBody filter, List<ExcelRow> rows)
    {
        var files = FileListUpdater.Updater.GetXMLFiles();
        var fileList = new List<string>();
        var rowsToAdd = new List<ExcelRow>();
        var invalidDate = new DateTime(1, 1, 1);
        
        Parallel.ForEach(files, (file) =>
        {
            var creationTime = file.CreationTime;
            var modificationTime = file.LastWriteTime;       

            if(modificationTime < creationTime)
                creationTime = modificationTime;
            
            if(filter.XmlDateFilter.FromDate != invalidDate)
            {
                if( !(creationTime >= filter.XmlDateFilter.FromDate) )
                    return;
            }
            if(filter.XmlDateFilter.ToDate != invalidDate)
            {
                if( !(creationTime < filter.XmlDateFilter.ToDate.AddDays(1) ) )
                    return;
            }
            
            lock(fileList)
            {
                fileList.Add(file.FullName);
            }
            
        } );

        Parallel.ForEach(NFeAssistant.Cache.Cache.InvoiceList, (invoice) =>
        {
            if(!fileList.Contains(invoice.FilePath.Replace('/', '\\') ) )
                return;
            
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

            var weight = filter.Weight.IsGrossWeight ? invoice.GrossWeight : invoice.NetWeight;
            if(filter.Weight.Precise)
            {
                if(filter.Weight.Value.Contains(weight.ToString() ) == false)
                    return;
            }
            else if(weight.ToString().Contains(filter.Weight.Value ) == false)
                return;
            
            var row = new ExcelRow
            {
                NF = invoice.NumberCode,
                EmissionDate = invoice.Emission.ToShortDateString(),
                Client = invoice.Client.Nome,
                City = invoice.Client.Cidade,
                Volumes = invoice.Volumes.ToString(),
                Weight = weight.ToString(),
                Value = invoice.Value.Total.ToString()
            };

            lock(rowsToAdd)
            {
                rowsToAdd.Add(row);
            }
        } );  

        rows.AddRange(rowsToAdd);
    }
}