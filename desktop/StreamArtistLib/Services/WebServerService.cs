using StreamArtist.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
//using Microsoft.Maui.Storage;
using WatsonWebserver;

namespace StreamArtist.Services
{

    public class WebServerService
    {
        private const int Port = 42984;

        private WatsonWebserver.Server? _server;

        public string HealthCheckUrl
        {
            get
            {
                return $"http://localhost:{Port}/health";
            }
        }

        public string UpdateAnimationsUrl
        {
            get
            {
                return $"http://localhost:{Port}/update";
            }
        }

        public void StartLocalServer()
        {
            _server = new Server("127.0.0.1", Port, false, Serve);
            _server.Start();
        }

        public void StopLocalServer()
        {
            _server?.Stop();
            _server = null;
        }


        private async Task Serve(HttpContext context)
        {
            // Note: webcam is at http://localhost:42984/files?file=webcam.html.
            Console.WriteLine("Requested: " + context.Request.RawUrlWithQuery);
            if (context.Request.RawUrlWithoutQuery.StartsWith("/update"))
            {
                await context.Response.Send(await UpdateAnimations());
                return;
            }

            if (context.Request.RawUrlWithoutQuery.StartsWith("/health"))
            {
                await context.Response.Send(await HealthCheck());
                return;
            }

            if (context.Request.RawUrlWithoutQuery.StartsWith("/donate"))
            {
                await context.Response.Send(Donate(context.Request));
                return;
            }

            if (context.Request.RawUrlWithoutQuery.StartsWith("/files"))
            {
                await context.Response.Send(await GetFile(context.Request));
                return;
            }

            if (context.Request.RawUrlWithoutQuery.StartsWith("/video"))
            {
                await SendVideo(context);
                return;
            }

            if (context.Request.RawUrlWithoutQuery.StartsWith("/video/"))
            {
                string FileName = context.Request.RawUrlWithoutQuery.Substring("/video/".Length);
                // await ServeVideoFile(context, FileName);
                return;
            }

            // Fire from https://www.vecteezy.com/video/10366199-fire-frame-loop-effect-burning-background-with-fire-abstract-background-seamless-loop-fire-burn-flame-energy-4k

            // Default serving path.
            // TODO: fix when bringing back this feature.
            //using var stream = await FileSystem.OpenAppPackageFileAsync("animator.html");
            //using var reader = new StreamReader(stream);
            //var contents = reader.ReadToEnd();
            //await context.Response.Send(contents);
        }

        private async Task<string> GetFile(HttpRequest request)
        {
            var p = System.Web.HttpUtility.ParseQueryString(request.FullUrl.Split('?')[1]);
            string file = p["file"];
            if (file == null)
            {
                return "No file specified.";
            }

            Assembly assembly = Assembly.GetExecutingAssembly();

            // Add this at the beginning of LoadEmbeddedHtml to debug
            var names = assembly.GetManifestResourceNames();
            //MessageBox.Show("Available Resources:\n" + string.Join("\n", names));

            // Get the name of the embedded resource (adjust based on your project structure and file name)
            // The format is typically: Namespace.Folder.FileName.Extension
            string resourceName = "StreamArtistLib.Resources." + file;

            
            // Read the embedded resource as a stream
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Read the HTML content from the stream
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string htmlContent = reader.ReadToEnd();
                        return htmlContent;
                    }
                }
            }
            return $"{file} not found in {string.Join("<br/>", names)}";
        }

        static async Task SendVideo(HttpContext ctx)
        {
            //var request = ctx.Request;
            //var p = System.Web.HttpUtility.ParseQueryString(request.FullUrl.Split('?')[1]);
            //string file = p["file"];
            //if (file == null)
            //{
            //    return;
            //}
            //// var dir = FileSystem.CacheDirectory;
            //// file = dir + "\\" + file;
            //Console.WriteLine("- User requested mp4 " + file);
            //ctx.Response.StatusCode = 200;
            //ctx.Response.ContentType = "video/mp4";
            //ctx.Response.ChunkedTransfer = true;
            //ctx.Response.Headers.Add("Cache-Control", "no-cache");
            //ctx.Response.Headers.Add("Content-Disposition", "attachment; filename=\"video.mp4\"");



            //// long fileSize = new F?ileInfo(file).Length;
            //// Console.WriteLine("Sending file of size " + fileSize + " bytes");

            //long bytesSent = 0;
            //// 50,294,153 bytes needed.
            //// 50,294,153 sent.

            //using (var fs = await FileSystem.OpenAppPackageFileAsync(file))

            //// using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            //{

            //    byte[] buffer = new byte[16384];
            //    // long bytesRemaining = fileSize;

            //    while (true)
            //    {
            //        // Thread.Sleep(500);
            //        int bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length);

            //        if (bytesRead == 0)
            //        {
            //            break;
            //        }

            //        // bytesRemaining -= bytesRead;

            //        // TODO: What if this is perfectly on the boundary? 
            //        // Check filestream.Length perhaps.
            //        if (bytesRead == 16384)
            //        {
            //            Console.WriteLine("- Sending chunk of size " + bytesRead);

            //            if (bytesRead == buffer.Length)
            //            {
            //                await ctx.Response.SendChunk(buffer);
            //            }
            //            else
            //            {
            //                byte[] temp = new byte[bytesRead];
            //                Buffer.BlockCopy(buffer, 0, temp, 0, bytesRead);
            //                await ctx.Response.SendChunk(temp);
            //            }
            //        }
            //        else
            //        {
            //            Console.WriteLine("- Sending final chunk of size " + bytesRead);

            //            if (bytesRead == buffer.Length)
            //            {
            //                await ctx.Response.SendFinalChunk(buffer);
            //            }
            //            else
            //            {
            //                byte[] temp = new byte[bytesRead];
            //                Buffer.BlockCopy(buffer, 0, temp, 0, bytesRead);
            //                await ctx.Response.SendFinalChunk(temp);
            //            }
            //        }

            //        bytesSent += bytesRead;

            //    }
            //}

            //Console.WriteLine("Sent " + bytesSent + " bytes");
            return;
        }

        private string Donate(HttpRequest request)
        {
            return "";
        }

        private async Task<string> HealthCheck()
        {
            return "{\"effects-server-status\": \"ok\"}";
        }

        private async Task<string> UpdateAnimations()
        {

            EffectsService effectsService = new EffectsService();
            var result = await effectsService.ProcessChatAnimations();
            // FireRendererRequest fireAnimatorRequest = new FireRendererRequest();
            // fireAnimatorRequest.Amount = "$10";
            // fireAnimatorRequest.Size = 10;
            // fireAnimatorRequest.Name = "Joel Gerard";
            // result.Add(fireAnimatorRequest);
            var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return jsonString;
        }

        // private async Task ServeVideoFile(HttpContext context, string fileName)
        // {
        //     string filePath = Path.Combine(FileSystem.AppDataDirectory, "Videos", fileName);

        //     if (!File.Exists(filePath))
        //     {
        //         await context.Response.Send("File not found", 404, "text/plain");
        //         return;
        //     }

        //     string MimeType = "video/mp4"; // Default to MP4
        //     if (fileName.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
        //     {
        //         MimeType = "video/webm";
        //     }
        //     else if (fileName.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
        //     {
        //         MimeType = "video/ogg";
        //     }

        //     context.Response.Headers.Add("Content-Type", MimeType);
        //     context.Response.Headers.Add("Accept-Ranges", "bytes");

        //     await context.Response.SendFile(filePath, MimeType);
        // }

    }
    public abstract class Person
    {
        public string Name { get; set; }
    }

    public class Employee : Person
    {
        public int EmployeeId { get; set; }
    }
}
