using ExtractorForWebUI.Data;
using ExtractorForWebUI.SDConnection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class DataService
{
    public HttpClient HttpClient = new HttpClient();

    List<GenerateTaskPack> tasks = new();
    Queue<WebUIServer> servers = new();
    Dictionary<string, long> retryAt = new();

    bool noTask;

    public void Tick()
    {
        if (!noTask && sharedData.imageGenerateRequests.Count == 0)
        {
            noTask = true;
            Console.WriteLine("No tasks remain.");
        }

        servers.Clear();
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

            if (d.canUse && d.activate && d.fn_index != -1)
            {
                servers.Enqueue(d);
            }
        }
        while (sharedData.imageGenerateRequests.TryDequeue(out var r))
        {
            if (servers.Count == 0)
            {
                sharedData.imageGenerateRequests.Enqueue(r);
                break;
            }
            var s = servers.Dequeue();
            s.canUse = false;
            SendPost(r, s);
            if (s.imageBatchSize < r.imageCount)
            {
                r.imageCount -= s.imageBatchSize;
                sharedData.imageGenerateRequests.Enqueue(r);
            }
        }

        tasks.RemoveAll(TaskDone);
    }

    bool TaskDone(GenerateTaskPack taskPack)
    {
        var receive = taskPack.task;
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
                RetryTask(taskPack);
                return true;
            }
            if (content.Headers.ContentType.MediaType == "application/json")
            {
                GenerateTaskJson(taskPack);
            }
            else
            {
                var imageContent = content.ReadAsByteArrayAsync().Result;
                ResultOutput(new ImageGenerateResult
                {
                    imageCount = 1,
                    imageData = imageContent,
                    saveDirectory = taskPack.Request.saveDirectory,
                    prompt = taskPack.Request.prompt,
                    width = taskPack.Request.width,
                    height = taskPack.Request.height,
                    fileFormat = Path.GetExtension(result.RequestMessage.RequestUri.LocalPath)
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            RetryTask(taskPack);
        }
        return true;
    }

    bool GenerateTaskJson(GenerateTaskPack taskPack)
    {
        var receive = taskPack.task;
        var server = taskPack.server;

        var result = receive.Result;
        var content = result.Content;
        string s = content.ReadAsStringAsync().Result;
        var response = JsonSerializer.Deserialize<WebUIResponse>(s);
        if (response.data == null || response.data.Length < 1)
        {
            Console.WriteLine(s);
            return true;
        }
        var element = (JsonElement)response.data[0];
        var e0 = element[0];
        if (e0.ValueKind == JsonValueKind.Object)
        {
            int length = element.GetArrayLength();
            server.canUse = true;
            for (int i = 0; i < length; i++)
            {
                if (length > 2 && i == 0)
                    continue;
                if (!element[i].TryGetProperty("name", out var o))
                    continue;

                var path = o.GetString();
                var getTask = HttpClient.GetAsync(new Uri(server.URL, string.Format("file={0}", path)));
                var request = taskPack.Request.Clone();
                request.imageCount = 1;
                tasks.Add(new GenerateTaskPack(getTask, server, request));
            }
        }
        return true;
    }

    void ResultOutput(ImageGenerateResult result)
    {
        sharedData.AddResult(result);
    }

    void RetryTask(GenerateTaskPack pack)
    {
        if (sharedData.imageGenerateRequests.TryPeek(out var request) && request.IsSameRequest(pack.Request))
        {
            request.imageCount += pack.Request.imageCount;
        }
        else
        {
            sharedData.imageGenerateRequests.Enqueue(pack.Request);
        }
        Retry(pack.server);
    }

    void Retry(WebUIServer server)
    {
        server.canUse = false;
        retryAt[server.internalName] = Stopwatch.GetTimestamp() + 30L * Stopwatch.Frequency;
    }

    void SendPost(ImageGenerateRequest request, WebUIServer server)
    {
        int batchCount = 1;
        int batchSize = Math.Min(request.imageCount, server.imageBatchSize);

        request = request.Clone();
        request.imageCount = batchSize;

        WebUIFrame frame;
        Uri uri;
        JsonContent content;
        frame = new WebUIFrame()
        {
            fn_index = server.fn_index,
            session_hash = "spider",
            data = new object[]
            {
                request.prompt,
                request.negativePrompt,
                "None",
                "None",
                request.step,
                request.sampleMethod,
                request.restore_faces,
                request.tiling,
                batchCount,
                batchSize,
                request.cfgScale,
                request.seed,
                request.subSeed,
                request.subSeedStrength,
                0,
                0,
                false,
                request.height,
                request.width,
                false,
                request.denoiseStrenth,
                0,
                0,
                "None",
                false,
                false,
                false,
                "",
                "Seed",
                "",
                "Nothing",
                "",
                true,
                true,
                false,
                null,
                "",
                ""
            }
        };
        uri = new Uri(server.URL, "/run/predict/");
        content = JsonContent.Create(frame);
        var task = HttpClient.PostAsync(uri, content);
        tasks.Add(new GenerateTaskPack(task, server, request));
    }

    public DataService(ServiceSharedData serviceSharedData)
    {
        this.sharedData = serviceSharedData;
    }
    ServiceSharedData sharedData;

    class GenerateTaskPack
    {
        public Task<HttpResponseMessage> task;
        public WebUIServer server;
        public ImageGenerateRequest Request;

        public GenerateTaskPack(Task<HttpResponseMessage> task, WebUIServer server, ImageGenerateRequest request)
        {
            this.task = task;
            this.server = server;
            Request = request;
        }
    }

#pragma warning disable IDE1006 // 命名样式
    class WebUIFrame
    {
        public int fn_index { get; set; }
        public string session_hash { get; set; }
        public object[] data { get; set; }
    }

    class WebUIResponse
    {
        public object[] data { get; set; }
    }
#pragma warning restore IDE1006 // 命名样式
}