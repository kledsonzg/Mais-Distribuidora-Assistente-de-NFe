using NFeAssistant.Invoice;
using NFeAssistant.Main;
using NPOI.SS.UserModel;

namespace NFeAssistant.ExcelBase
{
    internal static class SummaryGenerator
    {
        private static string? SummaryPath = Program.Config.GetExcelSummaryModelPath();

        internal static string? Generate(string outputFolder, string fileName, string title, ExcelRow[] rows)
        {
            var tempFolder = $"{Path.GetTempPath()}/KledsonZG/Assistente de NFe";
            var folder = Directory.CreateDirectory(tempFolder);

            var excelSummaryModelPath = Program.Config.GetExcelSummaryModelPath();
            if(excelSummaryModelPath == null || SummaryPath == null)
                return null;
            
            var tempFile = $"{tempFolder}/{fileName}.xlsx";
            
            File.Copy(SummaryPath, tempFile, true);

            var controller = ExcelFile.Read(tempFile);
            if(controller == null)
                return null;
            
            var sheet = controller.GetSummarySheet();
            if(sheet == null)
                return null;
            
            var firstRow = sheet.GetRow(2);
            int lastContentRow = -1;

            if(rows.Length > 1)
                sheet.ShiftRows(3, sheet.LastRowNum, rows.Length - 1, true, false);
            
            for(int i = 2; i < rows.Length + 2; i++)
            {
                var index = i - 2;
                var row = sheet.GetRow(i) ?? sheet.CreateRow(i);
                
                var date = Util.Functions.GetDateTimeFromString(rows[index].EmissionDate);
                var cells = new ICell[]{ row.GetCell(0) ?? row.CreateCell(0), row.GetCell(1) ?? row.CreateCell(1), row.GetCell(2) ?? row.CreateCell(2), row.GetCell(3) ?? row.CreateCell(3), row.GetCell(4) ?? row.CreateCell(4),
                row.GetCell(5) ?? row.CreateCell(5), row.GetCell(6) ?? row.CreateCell(6) };
                
                cells[0].SetCellValue(date);
                cells[1].SetCellValue(int.Parse(rows[index].NF) );
                cells[2].SetCellValue(rows[index].Client);
                cells[3].SetCellValue(rows[index].City);
                cells[4].SetCellValue(int.Parse(rows[index].Volumes) );
                cells[5].SetCellValue(double.Parse(rows[index].Weight) );
                cells[6].SetCellValue(double.Parse(rows[index].Value) );        

                for(int d = 0; d < 7; d++)
                {
                    var cellStyle = controller.XSSWorkBook.CreateCellStyle();
                    cellStyle.CloneStyleFrom(firstRow.GetCell(d).CellStyle);

                    cells[d].CellStyle = cellStyle;
                }
                lastContentRow = i;
            }

            lastContentRow++;
            var infoRow = sheet.GetRow(lastContentRow);
            int sumRow = -1;

            for(int i = sheet.LastRowNum; i >= 0; i--)
            {
                var row = sheet.GetRow(i);
                if(row == null)
                    continue;
                
                if(ExcelFile.GetRowContent(sheet, i).ToLower().Contains("count") == false)
                    continue;
                sumRow = i;
                break;
            }
            
            if(sumRow != -1)
            {
                var sumCells = sheet.GetRow(sumRow).Cells.Where(cell => cell.ToString() != null).Where(cell => cell.ToString().ToLower().Contains("sum") ).ToArray();
                var volumeSumCell = sumCells[0];
                var weightSumCell = sumCells[1];
                var valueSumCell = sumCells[2];

                volumeSumCell.CellFormula = $"SUMIF(E1:E{lastContentRow}, \">0\") & \" VOLUMES\"";
                weightSumCell.CellFormula = $"SUMIF(F1:F{lastContentRow}, \">0\") & \" Peso Kg\"";
                valueSumCell.CellFormula = $"SUMIF(G1:G{lastContentRow}, \">0\")";

                volumeSumCell.SetCellType(CellType.Formula);
                weightSumCell.SetCellType(CellType.Formula);
                valueSumCell.SetCellType(CellType.Formula);
            }

            sheet.GetRow(0).GetCell(0).SetCellValue(title);
            

            var filePath = $"{outputFolder}/{fileName}.xlsx";

            if(File.Exists(tempFile) )
            {
                File.Delete(tempFile);        
                controller.Save(tempFile);
                File.Copy(tempFile, filePath, true);

                return filePath;
            }
                
            return null;
        }
    }
}