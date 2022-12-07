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
    Dictionary<string, long> retryAt = new();

    public WebUIConfigService(ServiceSharedData sharedData)
    {
        this.sharedData = sharedData;
    }
    ServiceSharedData sharedData;

    public void Tick()
    {
        foreach (var (key, d) in sharedData.webUIServers)
        {
            if (retryAt.TryGetValue(key, out long timestamp))
            {
                if (timestamp < Stopwatch.GetTimestamp())
                {
                    d.canUse = true;
                    retryAt.Remove(key);
                }
            }

            if (d.canUse && d.activate)
            {
                if (d.fn_index != -1)
                {
                    continue;
                }
                if (!tasks.Any(u => u.server == d))
                {
                    tasks.Add(new ConfigTaskPack(HttpClient.GetAsync(new Uri(d.URL, "config")), d));
                }
            }
        }

        tasks.RemoveAll(TaskDone);
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
                return true;
            }
            if (content.Headers.ContentType.MediaType == "application/json")
            {
                var response = content.ReadFromJsonAsync<ConfigData>().Result;
                for (int i = 0; i < response.dependencies.Length; i++)
                {
                    ConfigDataDependency t = response.dependencies[i];
                    if (t.trigger == "click" && t.js == "submit")
                    {
                        server.fn_index = i;
                    }
                }
                if (server.fn_index > -1)
                {
                    Console.WriteLine("Server {0} Configured.", server.viewName);
                }
            }
            else
            {

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
        server.canUse = false;
        retryAt[server.internalName] = Stopwatch.GetTimestamp() + 30L * Stopwatch.Frequency;
    }

    class ConfigData
    {
        public ConfigDataDependency[] dependencies { get; set; }
    }

    class ConfigDataDependency
    {
        public string trigger { get; set; }
        public string js { get; set; }
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
