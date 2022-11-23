using ExtractorForWebUI.WebServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class WebService : IDisposable
{
    HttpListener httpListener;

    int port;

    int t0 = 10;


    public WebService(ServiceSharedData serviceSharedData)
    {
        this.serviceSharedData = serviceSharedData;
    }
    ServiceSharedData serviceSharedData;

    public bool launchBrowserOnStart;

    public void Initialize(int port)
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

    public void Tick()
    {
        if (t0 > 0)
        {
            t0--;
            Receive();
        }
    }
#if DEBUG
    public string workdir = Path.GetFullPath("../../../Files");
#else
    public string workdir = Path.GetFullPath("Files");
#endif
    void Receive()
    {
        httpListener.BeginGetContext(ProcessReceive, null);
    }
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
        long length = request.ContentLength64;
        StreamReader streamReader = new StreamReader(request.InputStream);
        string s = streamReader.ReadToEnd();

        var url = request.Url;

        if (url.LocalPath == "/favicon.ico")
        {
            response.OutputStream.Write(ReadFile("favicon.ico"));
        }
        else
        {
            response.StatusCode = 404;
            WebServiceContext webServiceContext = new WebServiceContext()
            {
                Uri = url,
                Request = request,
                Response = response,
                BasePath = workdir,
            };
            try
            {
                BaseWebService service = url.LocalPath.ToLower() switch
                {
                    _ => new FileService()
                };
                service.Process(webServiceContext);
            }
            catch
            {
                response.StatusCode = 404;
            }
        }
        if (url.Segments.Length >= 2)
        {

        }

        if (response.StatusCode == 404)
        {
            response.ContentType = "text/html";
            response.OutputStream.Write(ReadFile("404.html"));
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

    public void Dispose()
    {
        httpListener.Stop();
    }
}
