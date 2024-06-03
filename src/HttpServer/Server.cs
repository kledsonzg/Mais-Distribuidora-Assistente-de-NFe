using System.Net;
using System.Text;
using Newtonsoft.Json;
using NFeAssistant.ExcelBase;
using NFeAssistant.Interface;
using NFeAssistant.Main;
using NFeAssistant.Xml;
using NPOI.Util.ArrayExtensions;

namespace NFeAssistant.HttpServer
{
    internal static class Server
    {
        static FileServer FileServer;
        static bool IsOnline = true;
        internal static void Start()
        {
            var httpServer = new HttpListener();
            httpServer.Prefixes.Add("http://127.0.0.1:3344/");

            foreach(var ipConfig in Program.Config.Properties.App.WebPageIpConfig)
            {
                Program.PrintLine("SERVIDOR PERSONALIZADO: " + ipConfig);
                httpServer.Prefixes.Add(ipConfig);
            }
            
            FileServer = new FileServer();
            httpServer.Start();
            IsOnline = true;
            Util.Functions.StartPage("http://127.0.0.1:3344");

            while (IsOnline)
            {
                var context = httpServer.GetContext();

                var contextWorker = new Thread(new ThreadStart(delegate {
                    try
                    {
                        OnHandleRequest(context);
                    }
                    catch(Exception e)
                    {
                        string errorMsg = $"CAPTURA GENÉRICA DE EXCEÇÃO\nExceção lançada durante um tratamento de requisição HTTP.\nExceção: {e.Message} | Pilha: {e.StackTrace}";
                        Program.PrintLine(errorMsg);
                        Logger.Logger.Write(errorMsg);
                        try
                        {
                            RespondRequest(context.Response, HttpStatusCode.FailedDependency, Encoding.UTF8.GetBytes(errorMsg) );
                        }
                        catch(Exception e2)
                        {
                            errorMsg = $"Erro ao tentar responder requisição HTTP após uma exceção genérica ser lançada.\nExceção: {e2.Message} | Pilha: {e2.StackTrace}";
                            Program.PrintLine(errorMsg);
                            Logger.Logger.Write(errorMsg);
                        }
                    }
                } ) );

                contextWorker.Start();
            }
        }

        private static void OnHandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var url = request.Url;

            if(url == null)
            {
                RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                return;
            }
            
            //Console.WriteLine(request.Url.LocalPath);
            //Console.WriteLine(request.Url.AbsolutePath);
            //Console.WriteLine(request.Url.AbsoluteUri);

            switch(url.LocalPath)
            {
                case "/":
                {
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("index.html", true) ?? Array.Empty<byte>() );
                    return;
                }
                case "/folder-suggestion": case "/folder-suggestion/":
                {
                    string? folderInput;
                    try
                    {
                        folderInput = request.QueryString["input"];
                    }
                    catch(Exception e)
                    {
                        Program.PrintLine($"Ocorreu um erro ao obter o valor de 'input' na requisição. | Motivo: {e.Message} | Pilhas: {e.StackTrace}");
                        RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                        return;
                    }
                    if(folderInput == null)
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }


                    var result = FileServer.GetFolderDirectories(folderInput);
                    if(result == null)
                        RespondRequest(response, HttpStatusCode.FailedDependency, Array.Empty<byte>() );
                    else RespondRequest(response, HttpStatusCode.OK, result);

                    return;
                }
                case "/generateSummary":
                {
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("pages/summary-generation.html", true) ?? Array.Empty<byte>() );
                    return;
                }
                case "/generate":
                {
                    if(request.HttpMethod.ToUpper() != "POST")
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }

                    var requestBody = new StreamReader(request.InputStream).ReadToEnd();
                    var json = Xml.SummaryGenerator.GenerateResult(requestBody);
                    if(json == null)
                    {
                        RespondRequest(response, HttpStatusCode.FailedDependency, Array.Empty<byte>() );
                        return;
                    }

                    RespondRequest(response, HttpStatusCode.OK, Encoding.UTF8.GetBytes(json) );
                    return;
                }
                case "/generateExcelFile":
                {
                    if(request.HttpMethod.ToUpper() != "POST")
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }

                    var requestBody = new StreamReader(request.InputStream).ReadToEnd();
                    var requestObj = JsonConvert.DeserializeObject<ISummaryFileGenerationRequest>(requestBody);
                    //Program.PrintLine(JsonConvert.SerializeObject(requestObj) );
                    if(requestObj == null)
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }

                    var file = ExcelBase.SummaryGenerator.Generate(requestObj.OutputFolder, requestObj.FileName, requestObj.Title, requestObj.Rows);
                    if(file == null)
                    {
                        RespondRequest(response, HttpStatusCode.FailedDependency, Array.Empty<byte>() );
                        return;
                    }

                    new IOpenExcelFileRequest() { FilePath = file}.Open();
                    
                    RespondRequest(response, HttpStatusCode.OK, Array.Empty<byte>() );
                    return;
                }
                case "/summarySearch":
                {
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("pages/summary-search.html", true) ?? Array.Empty<byte>() );
                    return;
                }
                case "/summaryOpen":
                {
                    bool success = Viewer.View(request.InputStream);
                    if(success)
                        RespondRequest(response, HttpStatusCode.OK, Array.Empty<byte>() );
                    else RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                    
                    return;
                }
                case "/search":
                {
                    if(request.HttpMethod.ToUpper() != "POST")
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }

                    var requestBody = new StreamReader(request.InputStream).ReadToEnd();
                    var json = Search.ResultFromSearchRequestContent(requestBody);
                    RespondRequest(response, HttpStatusCode.OK, Encoding.UTF8.GetBytes(json) );
                    return;
                }
                case "/volumeContentIdentification":
                {
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("pages/volume-identification.html", true) ?? Array.Empty<byte>() );
                    return;
                }
                case "/import-nfs":
                {
                    if(request.HttpMethod.ToUpper() != "POST")
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }
                    var requestBody = new StreamReader(request.InputStream).ReadToEnd();
                    var json = NFExporter.GetInvoices(requestBody);
                    if(json == null)
                    {
                        RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                        return;
                    }

                    RespondRequest(response, HttpStatusCode.OK, Encoding.UTF8.GetBytes(json) );
                    return;
                }
                case "/generateVolumeIdentification":
                {
                    if(request.HttpMethod.ToUpper() != "POST")
                    {
                        RespondRequest(response, HttpStatusCode.BadRequest, Array.Empty<byte>() );
                        return;
                    }
                    var requestBody = new StreamReader(request.InputStream).ReadToEnd();
                    var folder = Program.Config.GetVolumeIdentificationPath();
                    var volumeIdentificationRequest = JsonConvert.DeserializeObject<IVolumeIdentificationRequest>(requestBody);

                    if(volumeIdentificationRequest == null || folder == null)
                    {
                        RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                        return;
                    }

                    if(NFeAssistant.Word.Label.GenerateLabel($"{folder}/{volumeIdentificationRequest.Nfs}.docx", volumeIdentificationRequest.Volumes) == false)
                    {
                        RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                        return;
                    }

                    RespondRequest(response, HttpStatusCode.OK, Array.Empty<byte>() );
                    return;
                }
                default:
                {
                    var result = FileServer.GetBytesFromFile(url.LocalPath, false);
                    if(result == null)
                    {
                        RespondRequest(response, HttpStatusCode.NotFound, Array.Empty<byte>() );
                        return;
                    }

                    RespondRequest(response, HttpStatusCode.OK, result);
                    break;
                }
            }
        }

        private static void RespondRequest(HttpListenerResponse response, HttpStatusCode statusCode, byte[] content)
        {
            response.StatusCode = (int) statusCode;
            response.OutputStream.Write(content, 0, content.Length);
            response.Close();
        }
    }
}