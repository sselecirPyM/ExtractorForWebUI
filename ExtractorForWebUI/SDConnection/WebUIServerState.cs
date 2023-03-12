namespace ExtractorForWebUI.SDConnection;

public enum WebUIServerState
{
    None,
    Disable,
    SSHNotConnected,
    SSHConnecting,
    SSHConnected,
    NotConfigured,
    Configuring,
    Configured,
    RunningError,
    Running,
}
