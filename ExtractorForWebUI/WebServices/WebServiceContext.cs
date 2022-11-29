using ExtractorForWebUI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.WebServices;

public class WebServiceContext
{
    public HttpListenerRequest Request;
    public HttpListenerResponse Response;

    public Uri Uri;

    public string BasePath;

    public ServiceSharedData SharedData;

    public byte[] GetFileData(string path)
    {
        string fullPath = Path.GetFullPath(path, BasePath);
        if (!Path.IsPathFullyQualified(fullPath) || !fullPath.StartsWith(BasePath))
        {
            throw new NotImplementedException("GetFileData");
        }
        return File.ReadAllBytes(fullPath);
    }
}
