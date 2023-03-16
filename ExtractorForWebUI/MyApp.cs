using ExtractorForWebUI.Data;
using ExtractorForWebUI.Magic;
using ExtractorForWebUI.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.Json;
using System.Threading;

namespace ExtractorForWebUI;

public class MyApp
{
    public string[] args;
    dynamic s = new ExpandoObject();
    void Init(LaunchOption launchOption)
    {

        var sharedData = new ServiceSharedData();
        sharedData.AppConfig = ReadJson<AppConfig>("appconfig.json");
        sharedData.AddTasks(ReadJson<TaskConfig>("tasks.json"));
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

        sharedData.getImageResults.Add(t.ioService);
        sharedData.getImageResults.Add(t.rtmpService);

        Console.WriteLine("Running. No server connected.");
        _ = t.ioService.Run();
        _ = t.webService.Run();
        _ = t.sshConnectService.Run();
        _ = t.rtmpService.Run();

        if (launchOption != null && launchOption.img2imgrequest != null)
        {
            ImageGenerateRequest template = ReadJson<ImageGenerateRequest>(launchOption.img2imgrequest);

            List<ImageGenerateRequest> requests = new List<ImageGenerateRequest>();
            var files = new DirectoryInfo(template.img2imgFile).GetFiles();
            Array.Sort(files, new FileComparer());
            foreach (var file in files)
            {
                var request = template.Clone();
                request.img2imgFile = file.FullName;
                request.saveFileName = Path.GetFileNameWithoutExtension(file.Name);
                requests.Add(request);
            }
            sharedData.AddTasks(new TaskConfig() { requests = requests.ToArray() });
        }
    }

    public void Run()
    {
        var launchOption = CommandLine.Parser.Default.ParseArguments<LaunchOption>(args).Value;

        Init(launchOption);
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


    public class FileComparer : IComparer<FileInfo>
    {
        public int Compare(FileInfo x1, FileInfo y1)
        {
            string x = x1.Name;
            string y = y1.Name;
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int lx = x.Length, ly = y.Length;

            for (int mx = 0, my = 0; mx < lx && my < ly; mx++, my++)
            {
                if (char.IsDigit(x[mx]) && char.IsDigit(y[my]))
                {
                    long vx = 0, vy = 0;

                    for (; mx < lx && char.IsDigit(x[mx]); mx++)
                        vx = vx * 10 + x[mx] - '0';

                    for (; my < ly && char.IsDigit(y[my]); my++)
                        vy = vy * 10 + y[my] - '0';

                    if (vx != vy)
                        return vx > vy ? 1 : -1;
                }

                if (mx < lx && my < ly && x[mx] != y[my])
                    return x[mx] > y[my] ? 1 : -1;
            }

            return lx - ly;
        }
    }
}
