namespace ExtractorForWebUI.SDConnection;

public class WebUIServerConfig
{
    public string Name { get; set; }
    public int BatchSize { get; set; } = 1;

    public string URL { get; set; }
    public bool Activate { get; set; } = true;

    public SSHConfig SSHConfig { get; set; }
}

public class SSHConfig
{
    public int Port { get; set; } = 22;
    public string UserName { get; set; } = "root";
    public string HostName { get; set; }
    public int? Forwarding { get; set; }
    public int LocalPort { get; set; }

    public string PrivateKeyFile { get; set; }

    string _name;
    public string GetName()
    {
        if (_name == null)
        {
            _name = string.Format($"{UserName}@{HostName}:{Port}/{Forwarding}");
        }
        return _name;
    }
}
