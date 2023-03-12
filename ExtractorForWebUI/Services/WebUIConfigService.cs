using ExtractorForWebUI.Data.Config;
using ExtractorForWebUI.SDConnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class WebUIConfigService
{
    public HttpClient HttpClient = new HttpClient();

    List<ConfigTaskPack> tasks = new();

    public WebUIConfigService(ServiceSharedData sharedData)
    {
        this.sharedData = sharedData;
    }
    ServiceSharedData sharedData;

    public void Tick()
    {
        foreach (var (key, d) in sharedData.webUIServers)
        {
            ServerState(d);
        }

        tasks.RemoveAll(TaskDone);
    }

    void ServerState(WebUIServer server)
    {
        if (server.state == WebUIServerState.SSHConnected)
        {
            server.state = WebUIServerState.NotConfigured;
        }

        if (server.activate && server.state == WebUIServerState.NotConfigured && server.retryAt < Stopwatch.GetTimestamp())
        {
            if (!tasks.Any(u => u.server == server))
            {
                tasks.Add(new ConfigTaskPack(HttpClient.GetAsync(new Uri(server.URL, "config")), server));
                server.state = WebUIServerState.Configuring;
            }
        }
    }

    bool TaskDone(ConfigTaskPack taskPack)
    {
        var receive = taskPack.task;
        var server = taskPack.server;
        if (!receive.IsCompleted)
            return receive.IsCompleted;

        try
        {
            var result = receive.Result;
            var content = result.Content;
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                string s1 = content.ReadAsStringAsync().Result;
                Console.WriteLine((int)result.StatusCode + s1);
                Retry(server);
            }
            else if (content.Headers.ContentType.MediaType == "application/json")
            {
                var response = content.ReadFromJsonAsync<ConfigData>().Result;
                server.SDWebUIConfig.Config(response);
                if (server.txt2img_fn_index != -1)
                {
                    server.state = WebUIServerState.Configured;
                    Console.WriteLine("Server {0} Configured.", server.viewName);
                }
                else
                {
                    Retry(server);
                }
            }
            else
            {
                Retry(server);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Retry(server);
        }
        return true;
    }

    void Retry(WebUIServer server)
    {
        server.state = WebUIServerState.NotConfigured;
        server.retryAt = Stopwatch.GetTimestamp() + 30L * Stopwatch.Frequency;
    }

    class ConfigTaskPack
    {
        public Task<HttpResponseMessage> task;
        public WebUIServer server;
        public ConfigTaskPack(Task<HttpResponseMessage> task, WebUIServer server)
        {
            this.task = task;
            this.server = server;
        }
    }
}
