using ExtractorForWebUI.Magic;
using ExtractorForWebUI.SDConnection;
using ExtractorForWebUI.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExtractorForWebUI;

public class MyApp
{
    public string[] args;
    dynamic s = new ExpandoObject();
    void Init()
    {

        AppConfig appConfig = ReadJson<AppConfig>("appconfig.json");

        dynamic m = new MagicObject();
        var t = ((WebService webService,
            DataService dataService,
            SSHConnectService sshConnectService,
            IOService ioService,
            ServiceSharedData sharedData))m;

        t.sharedData.AppConfig = appConfig;
        t.sharedData.taskConfig = ReadJson<TaskConfig>("tasks.json");
        t.sharedData.ServersInit();
        t.webService.Initialize(appConfig.Port);

        s.webService = t.webService;
        s.dataService = t.dataService;
        s.sshConnectService = t.sshConnectService;
        s.ioService = t.ioService;
        Console.WriteLine("Running. No server connected.");
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
