using ExtractorForWebUI.SSH;
using System;
using System.Diagnostics;

namespace ExtractorForWebUI.SDConnection;

public class WebUIServer
{
    public int txt2img_fn_index { get => SDWebUIConfig.txt2img_fn_index; }
    public int img2img_fn_index { get => SDWebUIConfig.img2img_fn_index; }
    public int imageBatchSize = 1;

    public bool activate;
    public Uri URL;
    public long timestamp;

    public string viewName;

    public WebUIServerState state;
    public long retryAt;

    public SSHConfig sshConfig;
    public SSHRemoteLink sshLink;


    public static WebUIServer FromConfig(WebUIServerConfig config)
    {
        return new WebUIServer
        {
            URL = (config.URL != null) ? new Uri(config.URL) : null,
            activate = config.Activate,
            imageBatchSize = config.BatchSize,
            timestamp = Stopwatch.GetTimestamp(),
            viewName = config.Name,
            sshConfig = config.SSHConfig,
        };
    }

    public void Update()
    {
        switch (state)
        {
            case WebUIServerState.None:
                break;
            case WebUIServerState.Running:
                break;
        }
    }

    public SDWebUIConfig SDWebUIConfig = new();

    public void Dispose()
    {
        sshLink?.Dispose();
    }
}
