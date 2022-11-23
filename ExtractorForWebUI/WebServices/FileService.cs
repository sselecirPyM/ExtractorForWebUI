using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.WebServices;

public class FileService : BaseWebService
{
    public override void Process(WebServiceContext context)
    {
        var response = context.Response;
        var url = context.Request.Url;
        if (url.Segments.Length > 2)
        {
            string fileName = string.Concat(url.Segments[2..]);

            response.ContentType = "text/html";
            response.OutputStream.Write(context.GetFileData(fileName));
            response.StatusCode = 200;
        }
        else
        {
            response.StatusCode = 404;
        }
    }
}
