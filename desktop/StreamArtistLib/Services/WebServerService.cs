using System;
using System.Text.Json;
using System.Web;
using System.IO;
using System.Collections.Generic;
using StreamArtist.Domain;
using Microsoft.Maui.Storage;
using WatsonWebserver;
using System.Threading.Tasks;

namespace StreamArtist.Services {

public class WebServerService
{
    private const int Port = 42984;
    
    private WatsonWebserver.Server? _server;

    public string HealthCheckUrl {
        get {
            return $"http://localhost:{Port}/health";
        }
    }

    public string UpdateAnimationsUrl {
        get {
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
        Console.WriteLine("Requested: " + context.Request.RawUrlWithQuery);
        if (context.Request.RawUrlWithoutQuery.StartsWith("/update")) {
            await context.Response.Send(await UpdateAnimations());
            return;
        }

        if (context.Request.RawUrlWithoutQuery.StartsWith("/health")) {
            await context.Response.Send(await HealthCheck());
            return;
        }

        if (context.Request.RawUrlWithoutQuery.StartsWith("/donate")) {
            await context.Response.Send( Donate(context.Request));
            return;
        }

        if (context.Request.RawUrlWithoutQuery.StartsWith("/files")) {
            await context.Response.Send(await GetFile(context.Request));
            return;
        }

        // Default serving path.
        using var stream = await FileSystem.OpenAppPackageFileAsync("animator.html");
        using var reader = new StreamReader(stream);
        var contents = reader.ReadToEnd();
        await context.Response.Send(contents);
    }

    private async Task<string> GetFile(HttpRequest request) {
        var p = System.Web.HttpUtility.ParseQueryString(request.FullUrl.Split('?')[1]);
        string file = p["file"];
        if (file == null) {
            return "";
        }
        using var stream = await FileSystem.OpenAppPackageFileAsync(file);
        using var reader = new StreamReader(stream);
        var contents = reader.ReadToEnd();
        return contents;
    }

    private string Donate(HttpRequest request) {
        return "";
    }

    private async Task<string> HealthCheck() {
        return "{\"effects-server-status\": \"ok\"}";
    }

    private async Task<string> UpdateAnimations() {

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

    
}
public abstract class Person
{
    public string Name { get; set; } 
}

public class Employee : Person
{
    public int EmployeeId { get; set; }
}}