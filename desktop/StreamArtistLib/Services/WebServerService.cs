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
        if (context.Request.RawUrlWithoutQuery.StartsWith("/update")) {
            await context.Response.Send(await UpdateAnimations());
            return;
        }

        // Default serving path.
        using var stream = await FileSystem.OpenAppPackageFileAsync("animator.html");
        using var reader = new StreamReader(stream);
        var contents = reader.ReadToEnd();
        await context.Response.Send(contents);
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