using asknvl.logger;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Requests.Abstractions;

namespace coeffbot.rest
{
    public class RestService : IRestService
    {
        #region const
        string TAG = "RST";
        #endregion

        #region vars
        ILogger logger;
        #endregion

        #region properties
        public List<IRequestProcessor> RequestProcessors { get; set; } = new();
        #endregion

        public RestService(ILogger logger)
        {
            this.logger = logger;
        }

        #region private
        async Task<string> processGetRequest(HttpListenerContext context)
        {
            string res = string.Empty;
            await Task.Run(() =>
            {
            });
            return res;
        }

        async Task<(HttpStatusCode, string)> processPostRequest(HttpListenerContext context)
        {            
            HttpStatusCode code = HttpStatusCode.NotFound;
            string text = code.ToString();

            await Task.Run(async () =>
            {

                var request = context.Request;
                string path = request.Url.AbsolutePath;

                using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                var requestBody = await reader.ReadToEndAsync();
                var splt = path.Split('/');

                var m = $"RX:\n{path}\n{requestBody}";
                logger.dbg(TAG, m);

                try
                {
                    switch (splt[1])
                    {
                        case "pushes":

                            switch (splt[2])
                            {
                                case "send":
                                    var p = RequestProcessors.FirstOrDefault(p => p is PushRequestProcessor);
                                    if (p != null)
                                        (code, text) = await p.ProcessRequestData(requestBody);
                                    break;                                
                            }                            
                            break;

                        case "statuses":
                            switch (splt[2])
                            {
                                case "update":
                                    var p = RequestProcessors.FirstOrDefault(p => p is StatusUpdateRequestProcessor);
                                    if (p != null)
                                        (code, text) = await p.ProcessRequestData(requestBody);
                                    break;
                            }
                            break;

                        case "notifies":
                            switch (splt[2])
                            {
                                case "send":
                                    var p = RequestProcessors.FirstOrDefault(p => p is NotifyRequestProcessor);
                                    if (p != null)
                                        (code, text) = await p.ProcessRequestData(requestBody);
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                } catch (Exception ex)
                {
                }

            });
            return (code, text);
        }
        async Task processRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            HttpStatusCode code = HttpStatusCode.MethodNotAllowed;
            string responseText = code.ToString();

            switch (request.HttpMethod)
            {
                case "GET":
                    responseText = await processGetRequest(context);
                    break;

                case "POST":
                    (code, responseText) = await processPostRequest(context);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    responseText = response.StatusCode.ToString();
                    break;
            }

            response.StatusCode = (int)code;
                        

            var buffer = Encoding.UTF8.GetBytes(responseText);
            response.ContentLength64 = buffer.Length;
            var output = response.OutputStream;
            await output.WriteAsync(buffer, 0, buffer.Length);

            var m = $"TX:\n{code}";
            logger.dbg(TAG, m);

        }
        #endregion

        #region public
        public async void Listen()
        {
            var listener = new HttpListener();
#if DEBUG
            listener.Prefixes.Add($"http://*:5050/pushes/");
            listener.Prefixes.Add($"http://*:5050/statuses/");
            listener.Prefixes.Add($"http://*:5050/notifies/");
#elif DEBUG_TG_SERV
            listener.Prefixes.Add($"http://localhost:5050/pushes/");
            listener.Prefixes.Add($"http://localhost:5050/statuses/");
            listener.Prefixes.Add($"http://localhost:5050/notifies/");
#else
            listener.Prefixes.Add($"http://*:5000/pushes/");
            listener.Prefixes.Add($"http://*:5000/statuses/");
            listener.Prefixes.Add($"http://*:5000/notifies/");
#endif
            try
            {
                logger.inf(TAG, "Starting rest server...");
                listener.Start();
            }
            catch (Exception ex)
            {
                logger.err(TAG, $"Rest server not started {ex.Message}");
            }

            logger.inf(TAG, "Rest server started");

            while (true)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    await processRequest(context);
                } catch (Exception ex)
                {
                    logger.err(TAG, ex.Message);
                }
            }
        }
#endregion
    }
}
