using ExtractorForWebUI.SDConnection;
using System.Collections.Generic;

namespace ExtractorForWebUI;

public class AppConfig
{
    public List<WebUIServerConfig> WebUIServers { get; set; }
    public int Port { get; set; } = 19198;
    public bool launchBrowserOnStart = true;
    public string PrivateKeyFile { get; set; }

    public string RTMPBaseURL { get; set; }

    public string RedirectBaseURL { get; set; }

    public void Check()
    {
        foreach (var serverConfig in WebUIServers)
        {
            if (serverConfig.Name == null)
            {
                var sshConfig = serverConfig.SSHConfig;
                serverConfig.Name = sshConfig.GetName();
            }
        }
    }
}
