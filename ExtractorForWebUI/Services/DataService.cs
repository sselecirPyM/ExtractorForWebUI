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

    List<GenerateTaskPack> generateTasks = new();
    List<GetImageTaskPack> getImageTasks = new();
    List<WebUIServer> servers = new();

    bool noTask;

    public void Tick()
    {
        if (!noTask && sharedData.imageGenerateRequests.Count == 0)
        {
            noTask = true;
            Console.WriteLine("No legacy tasks.");
        }

        servers.Clear();
        foreach (var (key, d) in sharedData.webUIServers)
        {
            if (d.state == WebUIServerState.RunningError && d.retryAt < Stopwatch.GetTimestamp())
            {
                d.state = WebUIServerState.Configured;
            }

            if (d.state == WebUIServerState.Configured)
            {
                servers.Add(d);
            }
        }
        foreach (var s in servers)
        {
            if (sharedData.imageGenerateRequests.TryDequeue(out var r))
            {
                SendPost(r, s);
                if (s.imageBatchSize < r.imageCount)
                {
                    r.imageCount -= s.imageBatchSize;
                    sharedData.imageGenerateRequests.Enqueue(r);
                }
            }
            else
            {
                break;
            }
        }

        generateTasks.RemoveAll(GenerateTaskDone);
        getImageTasks.RemoveAll(ImageTaskDone);
    }

    bool GenerateTaskDone(GenerateTaskPack taskPack)
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
                Console.WriteLine((int)result.StatusCode + content.ReadAsStringAsync().Result);
                RetryTask(taskPack);
                return true;
            }
            if (content.Headers.ContentType.MediaType == "application/json")
            {
                GenerateTaskJson(taskPack);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            RetryTask(taskPack);
        }
        return true;
    }

    bool ImageTaskDone(GetImageTaskPack taskPack)
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
                Console.WriteLine((int)result.StatusCode + content.ReadAsStringAsync().Result);
                RetryTask(taskPack);
                return true;
            }

            var imageData = content.ReadAsByteArrayAsync().Result;
            var request = taskPack.request;
            ResultOutput(new ImageGenerateResult
            {
                imageCount = 1,
                imageData = imageData,
                saveDirectory = request.saveDirectory,
                prompt = request.prompt,
                width = request.width,
                height = request.height,
                fileFormat = Path.GetExtension(result.RequestMessage.RequestUri.LocalPath),
                fileName = request.saveFileName,
            });
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
            int imgCount = taskPack.request.imageCount;
            int start = imgCount > 1 ? 1 : 0;

            int length = element.GetArrayLength();
            length = imgCount > 1 ? imgCount + 1 : imgCount;

            for (int i = start; i < length; i++)
            {
                if (!element[i].TryGetProperty("name", out var o))
                    continue;

                var path = o.GetString();
                var getTask = HttpClient.GetAsync(new Uri(server.URL, string.Format("file={0}", path)));
                var request = taskPack.request.Clone();
                request.imageCount = 1;
                getImageTasks.Add(new GetImageTaskPack(getTask, request));
            }
            server.state = WebUIServerState.Configured;
        }
        else
        {
            RetryTask(taskPack);
        }
        return true;
    }

    void ResultOutput(ImageGenerateResult result)
    {
        sharedData.AddResult(result);
    }

    void RetryTask(GenerateTaskPack pack)
    {
        AddRequest(pack.request);
        Retry(pack.server);
    }

    void RetryTask(GetImageTaskPack pack)
    {
        AddRequest(pack.request);
        //Retry(pack.server);
    }

    void AddRequest(ImageGenerateRequest addRequest)
    {
        if (sharedData.imageGenerateRequests.TryPeek(out var request) && request.IsSameRequest(addRequest))
        {
            request.imageCount += addRequest.imageCount;
        }
        else
        {
            sharedData.imageGenerateRequests.Enqueue(addRequest);
        }
    }

    void Retry(WebUIServer server)
    {
        server.retryAt = Stopwatch.GetTimestamp() + 30L * Stopwatch.Frequency;
        server.state = WebUIServerState.RunningError;
    }

    void SendPost(ImageGenerateRequest request, WebUIServer server)
    {
        int batchCount = 1;
        int batchSize = Math.Min(request.imageCount, server.imageBatchSize);

        request = request.Clone();
        request.imageCount = batchSize;

        Dictionary<string, object> data1;
        WebUITxt2ImgFrame frame;
        Uri uri;
        JsonContent content;
        GradioFillData taskType;
        object[] data;
        if (!string.IsNullOrEmpty(request.img2imgFile))
        {
            taskType = server.SDWebUIConfig.img2img;
            string imageType = Path.GetExtension(request.img2imgFile).ToLower() switch
            {
                ".png" => "png",
                ".jpg" => "jpeg",
                ".jpeg" => "jpeg",
                ".webp" => "webp",
                _ => ""
            };
            if (imageType == "")
                return;
            if (request.img2imgFileData == null)
                request.img2imgFileData = string.Format("\"data:image/{1};base64,{0}\"", Convert.ToBase64String(File.ReadAllBytes(request.img2imgFile)), imageType);

            data1 = new Dictionary<string, object>()
            {
                ["img2img_prompt"] = request.prompt,
                ["img2img_neg_prompt"] = request.negativePrompt,
                ["img2img_steps"] = request.step,
                ["img2img_sampling"] = request.sampleMethod,
                ["img2img_restore_faces"] = request.restoreFaces,
                ["img2img_batch_count"] = batchCount,
                ["img2img_batch_size"] = batchSize,
                ["img2img_seed"] = request.seed,
                ["img2img_subseed"] = request.subSeed,
                ["img2img_subseed_strength"] = request.subSeedStrength,
                ["img2img_height"] = request.height,
                ["img2img_width"] = request.width,
                ["img2img_image"] = request.img2imgFileData
            };
            AddControlNetParameter(data1, request.controlNet);

            data = taskType.FillDatas(data1);
            data[1] = 0;
        }
        else
        {
            taskType = server.SDWebUIConfig.txt2img;
            data1 = new Dictionary<string, object>()
            {
                ["txt2img_prompt"] = request.prompt,
                ["txt2img_neg_prompt"] = request.negativePrompt,
                ["txt2img_steps"] = request.step,
                ["txt2img_sampling"] = request.sampleMethod,
                ["txt2img_restore_faces"] = request.restoreFaces,
                ["txt2img_batch_count"] = batchCount,
                ["txt2img_batch_size"] = batchSize,
                ["txt2img_seed"] = request.seed,
                ["txt2img_subseed"] = request.subSeed,
                ["txt2img_subseed_strength"] = request.subSeedStrength,
                ["txt2img_height"] = request.height,
                ["txt2img_width"] = request.width,
            };
            data = taskType.FillDatas(data1);
        }

        frame = new WebUITxt2ImgFrame()
        {
            fn_index = taskType.fn_index,
            session_hash = "spider",
            data = data
        };
        uri = new Uri(server.URL, "/run/predict/");
        content = JsonContent.Create(frame);
        var task = HttpClient.PostAsync(uri, content);
        generateTasks.Add(new GenerateTaskPack(task, server, request));
        server.state = WebUIServerState.Running;
    }

    void AddControlNetParameter(Dictionary<string, object> parameters, ControlNetParameters controlNetParameters)
    {
        if (controlNetParameters == null)
            return;
        parameters["ControlNet_Enable"] = true;
        parameters["ControlNet_Preprocessor"] = controlNetParameters.preprocessor;
        parameters["ControlNet_Model"] = controlNetParameters.model;
        parameters["ControlNet_Weight"] = controlNetParameters.weight;
        parameters["ControlNet_Invert Input Color"] = controlNetParameters.invertColor;
        parameters["ControlNet_Resize Mode"] = controlNetParameters.resizeMode;
        parameters["ControlNet_Low VRAM"] = controlNetParameters.lowVram;
        parameters["ControlNet_Threshold A"] = controlNetParameters.thresholdA;
        parameters["ControlNet_Threshold B"] = controlNetParameters.thresholdB;
        parameters["ControlNet_Guidance Start (T)"] = controlNetParameters.guidanceStart;
        parameters["ControlNet_Guidance End (T)"] = controlNetParameters.guidanceStart;
        parameters["ControlNet_Guess Mode"] = controlNetParameters.guessMode;
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
        public ImageGenerateRequest request;

        public GenerateTaskPack(Task<HttpResponseMessage> task, WebUIServer server, ImageGenerateRequest request)
        {
            this.task = task;
            this.server = server;
            this.request = request;
        }
    }

    class GetImageTaskPack
    {
        public Task<HttpResponseMessage> task;
        public ImageGenerateRequest request;

        public GetImageTaskPack(Task<HttpResponseMessage> task, ImageGenerateRequest request)
        {
            this.task = task;
            this.request = request;
        }
    }

#pragma warning disable IDE1006 // 命名样式
    class WebUITxt2ImgFrame
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