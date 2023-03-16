using ExtractorForWebUI.Data;
using ExtractorForWebUI.RTMP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class RTMPService : IGetImageResult
{
    public RTMPService(ServiceSharedData sharedData)
    {
        this.sharedData = sharedData;
    }
    ServiceSharedData sharedData;

    public async Task Run()
    {
        while (true)
        {
            Tick();
            await Task.Delay(1);
        }
    }

    string gethex = "0123456789ABCDEF";
    string GetHex(byte[] data)
    {
        Span<char> chars = stackalloc char[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            chars[i * 2] = gethex[(data[i] & 0xf0) >> 4];
            chars[i * 2 + 1] = gethex[data[i] & 0xf];
        }
        return chars.ToString();
    }

    void Tick()
    {
        while (sharedData.RTMPRequests.TryDequeue(out var request))
        {
            if (sessions.TryGetValue(request.prompt, out var session))
            {
                session.lastActivate = Stopwatch.GetTimestamp();
            }
            else
            {
                var hash = MD5.HashData(Encoding.UTF8.GetBytes(request.prompt));
                string md5str = GetHex(hash);
                session = new RTMPSession()
                {
                    url = request.url.ToString(),
                    lastActivate = Stopwatch.GetTimestamp(),
                    size = (1024, 512),
                    presentSize = (2048, 1024),
                    prompt = request.prompt,
                    md5 = md5str,
                    saveDir = "live/" + md5str
                };
                var info = new DirectoryInfo(session.saveDir);
                if (info.Exists)
                {
                    var files = info.GetFiles("*.jpg");

                    if (files.Length > 0)
                    {
                        for (int i = 0; i < 4; i++)
                            session.queue.Enqueue(File.ReadAllBytes(files[Random.Shared.Next(0, files.Length)].FullName));
                    }
                }

                sessions.Add(request.prompt, session);
                _ = session.Run();
            }
        }

        foreach ((string a, var session) in sessions)
        {
            if (session.lastActivate + Stopwatch.Frequency * 600 < Stopwatch.GetTimestamp())
            {
                sessions.Remove(a);
                session.Dispose();
            }
        }
        while (rtmpQueue.TryDequeue(out var result))
        {
            if (result.imageData.Length > 1024 * 10)
                results.Add(result);
        }
        if (sharedData.imageGenerateRequests.Count < sessions.Count)
        {
            foreach ((string a, var session) in sessions)
                sharedData.imageGenerateRequests.Enqueue(new Data.ImageGenerateRequest()
                {
                    prompt = session.prompt,
                    width = session.size.Item1,
                    height = session.size.Item2,
                    negativePrompt = negativePrompt,
                    cfgScale = cfgScale,
                    imageCount = 1,
                    step = 30,
                    sampleMethod = "DDIM",
                    saveDirectory = session.saveDir
                });
        }

        foreach ((string a, var session) in sessions)
        {
            foreach (var img in results)
            {
                if (session.prompt == img.prompt)
                {
                    session.queue.Enqueue(img.imageData);
                }
            }
        }
        sharedData.rtmpLive = sessions.Count;
        results.Clear();
    }

    public void OnGetImageResult(ImageGenerateResult result)
    {
        if (sharedData.rtmpLive > 0)
            rtmpQueue.Enqueue(result);
    }

    ConcurrentQueue<ImageGenerateResult> rtmpQueue = new();

    List<ImageGenerateResult> results = new();

    string negativePrompt = "nsfw, lowres, bad anatomy, bad hands, text, error, missing fingers, extra digit, fewer digits, cropped, worst quality, low quality, normal quality, jpeg artifacts, signature, watermark, username, blurry, ugly, 3D game, bad art, bad shadow, long neck,";

    int cfgScale = 7;

    public Dictionary<string, RTMPSession> sessions = new();
}
