using ExtractorForWebUI.SDConnection;
using ExtractorForWebUI.SSH;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class SSHConnectService : IDisposable
{
    long lastUpdate;

    long interval = 30 * Stopwatch.Frequency;

    public async Task Run()
    {
        Debug.Assert(!disposed);
        while (!disposed)
        {
            Tick();
            await Task.Delay(1);
        }
    }

    void Tick()
    {
        long timestamp = Stopwatch.GetTimestamp();
        if (lastUpdate + interval < timestamp)
        {
            lastUpdate = timestamp;
        }
        else
        {
            return;
        }

        foreach ((var key, var server) in sharedData.webUIServers)
        {
            if (!server.activate)
                continue;

            var link = server.sshLink;
            if (link != null)
            {
                if (!link.IsConnected)
                {
                    link.Dispose();
                    server.sshLink = null;
                    server.state = WebUIServerState.SSHNotConnected;
                    Console.WriteLine("Connection invalid. " + link.internalName);
                }
                continue;
            }

            var sshConfig = server.sshConfig;
            if (sshConfig == null)
                continue;

            try
            {
                string connectionName = sshConfig.GetName();
                SSHRemoteLink remoteLink = new SSHRemoteLink
                {
                    port = sshConfig.Port,
                    hostname = sshConfig.HostName,
                    username = sshConfig.UserName,
                    remoteForward = (uint)sshConfig.Forwarding,
                    localForward = sshConfig.LocalPort == 0 ? (uint)Random.Shared.Next(20000, 60000) : (uint)sshConfig.LocalPort,
                    privateKeyFile = sshConfig.PrivateKeyFile,
                    internalName = connectionName,
                };
                server.state = WebUIServerState.SSHConnecting;
                remoteLink.Connect();

                AfterConnect1(remoteLink, server);
                Console.WriteLine("SSH: {0} port:{1}", connectionName, remoteLink.localForward);
            }
            catch (Exception ex)
            {
                server.state = WebUIServerState.SSHNotConnected;
            }
        }
    }

    void AfterConnect1(SSHRemoteLink remoteLink, WebUIServer server)
    {
        server.URL = new Uri(string.Format("http://127.0.0.1:{0}/", remoteLink.localForward));
        server.sshLink = remoteLink;
        server.state = WebUIServerState.SSHConnected;
    }

    public SSHConnectService(ServiceSharedData serviceSharedData)
    {
        this.sharedData = serviceSharedData;
    }
    ServiceSharedData sharedData;

    bool disposed = false;

    public void Dispose()
    {
        disposed = true; ;
    }
}
