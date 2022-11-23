using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExtractorForWebUI.SDConnection;

public class WebUIServer
{
    public int fn_index = -1;
    public int imageBatchSize = 1;

    public bool activate;
    public bool canUse;
    public int failCount;
    public Uri URL;
    public long timestamp;
    public bool isSSHConnect;

    public bool windows;

    public string internalName;

    public static WebUIServer FromConfig(WebUIServerConfig config)
    {
        return new WebUIServer
        {
            URL = (config.URL != null) ? new Uri(config.URL) : null,
            windows = config.Windows,
            activate = config.Activate,
            canUse = true,
            imageBatchSize = config.BatchSize,
            isSSHConnect = config.SSHConfig != null,
            timestamp = Stopwatch.GetTimestamp(),
            internalName = config.Name,
        };
    }
}
