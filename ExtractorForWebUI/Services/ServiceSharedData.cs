using ExtractorForWebUI.Data;
using ExtractorForWebUI.SDConnection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ExtractorForWebUI.Services;

public class ServiceSharedData
{
    public ConcurrentQueue<ImageGenerateResult> imageGenerateResults = new();

    public ConcurrentQueue<ImageGenerateRequest> imageGenerateRequests = new();

    public ConcurrentDictionary<string, WebUIServer> webUIServers = new();

    public Dictionary<string, WebUIServerConfig> ssh2Server = new();

    public ConcurrentQueue<RTMPRequest> RTMPRequests = new();

    public ConcurrentQueue<ImageGenerateResult> rtmpQueue = new();

    public int rtmpLive = 0;

    public bool UseSSH = true;

    public AppConfig AppConfig;

    public TaskConfig taskConfig;

    public List<SSHConfig> sshConfigs;

    public void AddResult(ImageGenerateResult result)
    {
        imageGenerateResults.Enqueue(result);
        if (rtmpLive > 0)
            rtmpQueue.Enqueue(result);
    }

    public void ServersInit()
    {
        AppConfig.Check();
        ssh2Server = new();
        foreach (var config in AppConfig.WebUIServers)
        {
            if (config.SSHConfig != null)
            {
                ssh2Server[config.SSHConfig.GetName()] = config;
            }
            else
            {
                webUIServers[config.Name] = WebUIServer.FromConfig(config);
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

        sshConfigs = new();
        foreach (var servers in AppConfig.WebUIServers)
        {
            if (servers.SSHConfig != null && servers.Activate)
                sshConfigs.Add(servers.SSHConfig);
        }
    }
}
