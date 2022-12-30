using ExtractorForWebUI.WebServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class WebService : IDisposable
{
    HttpListener httpListener;

    int port;

    int t0 = 10;


    public WebService(ServiceSharedData sharedData)
    {
        this.sharedData = sharedData;
    }
    ServiceSharedData sharedData;

    public bool launchBrowserOnStart;

    bool initialized = false;

    void Initialize(int port)
    {
        var url = string.Format("http://127.0.0.1:{0}/", port);
        httpListener = new HttpListener();
        httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
        httpListener.Prefixes.Add(url);
        this.port = port;
        httpListener.Start();

        if (launchBrowserOnStart)
            LaunchBrowser(url);

        //Console.WriteLine(url);
    }

    void Tick()
    {
        if (!initialized)
        {
            Initialize(sharedData.AppConfig.Port);
            initialized = true;
        }
        if (t0 > 0)
        {
            t0--;
            httpListener.BeginGetContext(ProcessReceive, null);
        }
    }

    public async Task Run()
    {
        while (!disposed)
        {
            Tick();
            await Task.Delay(1);
        }
    }
#if DEBUG
    public string workdir = Path.GetFullPath("../../../Static");
#else
    public string workdir = Path.GetFullPath("Static");
#endif
    void ProcessReceive(IAsyncResult ar)
    {
        try
        {
            Dispatcher(httpListener.EndGetContext(ar));
        }
        catch (Exception ex)
        {

        }
        t0++;
    }

    byte[] ReadFile(string path)
    {
        string fullPath = Path.GetFullPath(path, workdir);
        if (!Path.IsPathFullyQualified(fullPath) || !fullPath.StartsWith(workdir))
        {
            throw new NotImplementedException(fullPath);
        }
        return File.ReadAllBytes(fullPath);
    }

    void Dispatcher(HttpListenerContext context)
    {
        var request = context.Request;

        var response = context.Response;
        response.StatusCode = 200;

        Dictionary<string, string[]> record = new Dictionary<string, string[]>();
        foreach (var name in request.Headers.AllKeys)
        {
            record[name] = request.Headers.GetValues(name);
        }

        var url = request.Url;

        if (url.LocalPath == "/favicon.ico")
        {
            response.OutputStream.Write(ReadFile("favicon.ico"));
        }
        else if (url.Segments.Length > 1)
        {
            //response.StatusCode = 404;
            WebServiceContext webServiceContext = new WebServiceContext()
            {
                Uri = url,
                Request = request,
                Response = response,
                BasePath = workdir,
                SharedData = sharedData,
            };
            try
            {
                BaseWebService service = url.Segments[1].ToLower() switch
                {
                    "addtask/" => new AddTaskService(),
                    "stream/" => new OpenStreamService(),
                    _ => new FileService()
                };
                service.Process(webServiceContext);
            }
            catch
            {
                response.StatusCode = 404;
            }
        }

        try
        {
            if (response.StatusCode == 404)
            {
                response.ContentType = "text/html";
                response.OutputStream.Write(ReadFile("404.html"));
            }
        }
        catch
        {

        }

        response.Close();
    }

    void LaunchBrowser(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true,
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("open", url);
        }
    }

    public bool disposed = false;

    public void Dispose()
    {
        disposed = true;
        httpListener.Stop();
    }
}
