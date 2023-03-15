using ExtractorForWebUI.SSH;
using System;

namespace ExtractorForWebUI.SDConnection;

public class WebUIServer
{
    public int imageBatchSize = 1;

    public bool activate;
    public WebUIServerState state;
    public long retryAt;

    public Uri URL;
    public string viewName;

    public SSHConfig sshConfig;
    public SSHRemoteLink sshLink;


    public static WebUIServer FromConfig(WebUIServerConfig config)
    {
        return new WebUIServer
        {
            URL = (config.URL != null) ? new Uri(config.URL) : null,
            activate = config.Activate,
            imageBatchSize = config.BatchSize,
            viewName = config.Name,
            sshConfig = config.SSHConfig,
        };
    }

    public SDWebUIConfig SDWebUIConfig = new();

    public void Dispose()
    {
        sshLink?.Dispose();
    }
}
