using ExtractorForWebUI.SDConnection;
using ExtractorForWebUI.SSH;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class SSHConnectService : IDisposable
{
    IReadOnlyList<SSHConfig> sshConfigs;

    public Dictionary<string, SSHRemoteLink> liveLinks = new();

    long timestamp;
    long lastUpdate;

    long interval = 30 * Stopwatch.Frequency;

    public async Task Run()
    {
        Debug.Assert(!disposed);
        while (!disposed)
        {
            CoreTick();
            await Task.Delay(1);
        }
    }

    void CoreTick()
    {
        timestamp = Stopwatch.GetTimestamp();
        if (lastUpdate + interval < timestamp)
        {
            lastUpdate = timestamp;
        }
        else
        {
            return;
        }

        if (!sharedData.UseSSH)
            return;
        sshConfigs = sharedData.sshConfigs;
        foreach (var sshConfig in sshConfigs)
        {
            string connectionName = sshConfig.GetName();
            if (liveLinks.TryGetValue(connectionName, out var link))
            {
                if (!link.IsConnected)
                {
                    link.Dispose();
                    liveLinks.Remove(connectionName);
                    sharedData.webUIServers.TryRemove(connectionName, out _);
                    Console.WriteLine("Connection invalid. " + connectionName);
                }
                continue;
            }

            try
            {
                SSHRemoteLink remoteLink = new SSHRemoteLink();
                remoteLink.port = sshConfig.Port;
                remoteLink.hostname = sshConfig.HostName;
                remoteLink.username = sshConfig.UserName;
                remoteLink.remoteForward = (uint)sshConfig.Forwarding;
                remoteLink.localForward = sshConfig.LocalPort == 0 ? (uint)Random.Shared.Next(20000, 60000) : (uint)sshConfig.LocalPort;
                remoteLink.internalName = connectionName;
                remoteLink.privateKeyFile = sharedData.AppConfig.PrivateKeyFile;
                remoteLink.Connect();

                Console.WriteLine("SSH: {0} port:{1}", connectionName, remoteLink.localForward);
                liveLinks[connectionName] = remoteLink;
                AfterConnect(remoteLink);
            }
            catch (Exception ex)
            {

            }
        }
    }

    void AfterConnect(SSHRemoteLink remoteLink)
    {
        var serverConfig = sharedData.ssh2Server[remoteLink.internalName];
        var server = WebUIServer.FromConfig(serverConfig);
        server.URL = new Uri(string.Format("http://127.0.0.1:{0}/", remoteLink.localForward));
        sharedData.webUIServers[remoteLink.internalName] = server;
    }

    public SSHConnectService(ServiceSharedData serviceSharedData)
    {
        this.sharedData = serviceSharedData;
    }
    ServiceSharedData sharedData;

    bool disposed = false;

    public void Dispose()
    {
        disposed = true;
        foreach (var link in liveLinks.Values)
        {
            link.Disconnect();
        }
        liveLinks.Clear();
    }
}
