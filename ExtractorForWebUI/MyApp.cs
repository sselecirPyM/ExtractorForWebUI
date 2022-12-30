using ExtractorForWebUI.Magic;
using ExtractorForWebUI.Services;
using System;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ExtractorForWebUI;

public class MyApp
{
    public string[] args;
    dynamic s = new ExpandoObject();
    void Init()
    {
        var sharedData = new ServiceSharedData();
        sharedData.AppConfig = ReadJson<AppConfig>("appconfig.json");
        sharedData.taskConfig = ReadJson<TaskConfig>("tasks.json");
        sharedData.ServersInit();

        var magicObject = new MagicObject();
        magicObject.Insert(sharedData);

        dynamic m = magicObject;
        var t = ((WebService webService,
            WebUIConfigService configService,
            DataService dataService,
            SSHConnectService sshConnectService,
            RTMPService rtmpService,
            IOService ioService))m;

        s.configService = t.configService;
        s.dataService = t.dataService;

        Console.WriteLine("Running. No server connected.");
        _ = t.ioService.Run();
        _ = t.webService.Run();
        _ = t.sshConnectService.Run();
        _ = t.rtmpService.Run();
    }

    public void Run()
    {
        Init();
        while (true)
        {
            foreach (var t in s)
            {
                t.Value.Tick();
            }
            Thread.Sleep(1);
        }
    }

    T ReadJson<T>(string fileName) where T : new()
    {
        T t = new T();
        try
        {
            using var stream = File.OpenRead(fileName);
            var config1 = JsonSerializer.Deserialize<T>(stream);
            t = config1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return t;
    }
}
