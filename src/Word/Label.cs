using NFeAssistant.Interface;
using NFeAssistant.Main;
using NPOI.XWPF.UserModel;

namespace NFeAssistant.Word
{
    internal static class Label
    {
        internal static bool GenerateLabel(string filePath, IVolume[] volumes)
        {
            var blackLogoPicturePath = Program.Config.GetBlackLogoImagePath();
            if(blackLogoPicturePath == null)
                return false;
            var blackLogoPicture = File.ReadAllBytes(blackLogoPicturePath);

            if(File.Exists(filePath) )
                File.Delete(filePath);

            var document = CreateDocument();
                     
            var blackLogoId = document.AddPictureData(blackLogoPicture, (int) PictureType.PNG);
            XWPFParagraph? lastParagraph = null;
            
            volumes.ToList().ForEach(volume =>
            {
                var pr = CreateCenteredParagraph(document);
                pr.CreateRun().AddPicture(new MemoryStream(blackLogoPicture), (int) PictureType.PNG, blackLogoId, 1231862, 720226);
                if(volume != volumes[0] )
                    pr.IsPageBreak = true;

                pr = CreateCenteredParagraph(document);
                var content = pr.CreateRun();
                content.CharacterSpacing = 0;
                content.FontFamily = "Arial";
                content.IsBold = true;

                content.AppendText(volume.Index < 10 ? $"0{volume.Index}" : volume.Index.ToString() );
                

                CreateCenteredParagraph(document);
                
                volume.Products.ToList().ForEach(product => 
                {
                    pr = CreateCenteredParagraph(document);
                    var content = pr.CreateRun();
                    content.CharacterSpacing = 0;
                    content.FontFamily = "Calibri";
                    content.IsBold = true;

                    content.AppendText(product);
                    
                } );
            } );

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite) )
            {
                document.Write(fs);
            }

            Util.Functions.OpenFolder(filePath);
                     
            return true;
        }

        private static XWPFDocument CreateDocument()
        {
            var document = new XWPFDocument();
            document.Document.body.sectPr = new()
            {
                pgSz = new()
                {
                    w = 10 * 567,
                    h = 15 * 567
                },
                pgMar = new()
                {
                    top = (ulong) (0.5 * 567) ,
                    bottom = (ulong) (0.5 * 567) ,
                    left = (ulong) (0.5 * 567) ,
                    right = (ulong) (0.5 * 567) 
                },
            };

            return document;
        }

        private static XWPFParagraph CreateCenteredParagraph(XWPFDocument document)
        {
            var paragraph = document.CreateParagraph();
            paragraph.Alignment = ParagraphAlignment.CENTER;
            paragraph.SpacingAfter = 0;

            return paragraph;
        }
    }
}