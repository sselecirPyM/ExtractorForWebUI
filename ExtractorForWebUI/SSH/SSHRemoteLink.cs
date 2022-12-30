using Renci.SshNet;
using System;

namespace ExtractorForWebUI.SSH;

public class SSHRemoteLink : IDisposable
{
    SshClient sshClient;
    public string hostname;
    public string username;
    public int port = 22;

    public uint localForward;
    public uint remoteForward;

    public string internalName;

    public string privateKeyFile;

    public bool IsConnected => sshClient.IsConnected;

    public void Connect()
    {
        string path = Environment.ExpandEnvironmentVariables(privateKeyFile);
        ConnectionInfo connectionInfo = new ConnectionInfo(hostname, port, username, new AuthenticationMethod[1]
        {
             new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(path))
            //new PasswordAuthenticationMethod(username, "")
        });

        sshClient = new SshClient(connectionInfo);
        sshClient.HostKeyReceived += (sender, e) =>
        {

        };
        sshClient.KeepAliveInterval = new TimeSpan(0, 0, 30);
        sshClient.Connect();
        sshClient.AddForwardedPort(new ForwardedPortLocal("127.0.0.1", localForward, "127.0.0.1", remoteForward));
        foreach (var forwarding in sshClient.ForwardedPorts)
            forwarding.Start();
    }

    public void Disconnect()
    {
        sshClient.Disconnect();
        sshClient.Dispose();
    }

    public void Dispose()
    {
        sshClient?.Dispose();
    }
}
