﻿using ExtractorForWebUI.Data;
using ExtractorForWebUI.SDConnection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExtractorForWebUI.Services;

public class ServiceSharedData
{
    public ConcurrentQueue<ImageGenerateRequest> imageGenerateRequests = new();

    public ConcurrentDictionary<string, WebUIServer> webUIServers = new();

    public ConcurrentQueue<RTMPRequest> RTMPRequests = new();

    public List<IGetImageResult> getImageResults = new();

    public int rtmpLive = 0;

    public AppConfig AppConfig;

    public void AddResult(ImageGenerateResult result)
    {
        foreach (var item in getImageResults)
        {
            item.OnGetImageResult(result);
        }
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
    }

    public void AddTasks(TaskConfig taskConfig)
    {
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
