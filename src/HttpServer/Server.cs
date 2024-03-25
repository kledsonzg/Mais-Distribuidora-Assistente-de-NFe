using System.Data.SqlTypes;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Unicode;
using Newtonsoft.Json;
using NFeAssistant.ExcelBase;
using NFeAssistant.Interface;
using NFeAssistant.Main;
using NFeAssistant.Xml;

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
            
            FileServer = new FileServer();
            httpServer.Start();
            IsOnline = true;

            while (IsOnline)
            {
                var context = httpServer.GetContext();

                var contextWorker = new Thread(new ThreadStart(delegate {
                    OnHandleRequest(context);
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
            
            Console.WriteLine(request.Url.LocalPath);
            Console.WriteLine(request.Url.AbsolutePath);
            //Console.WriteLine(request.Url.AbsoluteUri);

            switch(url.LocalPath)
            {
                case "/":
                {
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("index.html", true) );
                    return;
                }
                case "/folder-suggestion": case "/folder-suggestion/":
                {
                    string folderInput;
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
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("pages/summary-generation.html", true) );
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
                    RespondRequest(response, HttpStatusCode.OK, FileServer.GetBytesFromFile("pages/summary-search.html", true) );
                    return;
                }
                case "/summaryOpen":
                {
                    bool success = Viewer.View(request.InputStream, response);
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