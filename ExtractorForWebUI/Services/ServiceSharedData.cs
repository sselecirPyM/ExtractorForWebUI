using ExtractorForWebUI.Data;
using ExtractorForWebUI.SDConnection;
using System;
using System.Collections.Concurrent;

namespace ExtractorForWebUI.Services;

public class ServiceSharedData
{
    public ConcurrentQueue<ImageGenerateResult> imageGenerateResults = new();

    public ConcurrentQueue<ImageGenerateRequest> imageGenerateRequests = new();

    public ConcurrentDictionary<string, WebUIServer> webUIServers = new();

    public ConcurrentQueue<RTMPRequest> RTMPRequests = new();

    public ConcurrentQueue<ImageGenerateResult> rtmpQueue = new();

    public int rtmpLive = 0;

    public AppConfig AppConfig;

    public TaskConfig taskConfig;

    public void AddResult(ImageGenerateResult result)
    {
        imageGenerateResults.Enqueue(result);
        if (rtmpLive > 0)
            rtmpQueue.Enqueue(result);
    }

    public void ServersInit()
    {
        AppConfig.Check();
        foreach (var config in AppConfig.WebUIServers)
        {
            webUIServers[config.Name] = WebUIServer.FromConfig(config);
            if (!config.Activate)
            {
                webUIServers[config.Name].state = WebUIServerState.Disable;
            }
            else if (config.SSHConfig != null)
            {
                webUIServers[config.Name].state = WebUIServerState.SSHNotConnected;
            }
            else
            {
                webUIServers[config.Name].state = WebUIServerState.NotConfigured;
            }
        }
        if (taskConfig != null && taskConfig.requests != null && taskConfig.requests.Length > 0)
        {
            foreach (var r in taskConfig.requests)
            {
                imageGenerateRequests.Enqueue(r);
            }
            Console.WriteLine("{0} tasks added.", taskConfig.requests.Length);
        }
    }
}
