using System;
using System.Net;

namespace ExtractorForWebUI.WebServices;

internal class OpenStreamService : BaseWebService
{
    public override void Process(WebServiceContext context)
    {
        var prompt = context.Request.QueryString["p"];
        if (prompt == null)
        {
            context.Response.StatusCode = 404;
            return;
        }
        var req = new Data.RTMPRequest()
        {
            url = new Uri(new Uri(context.SharedData.AppConfig.RTMPBaseURL), WebUtility.UrlEncode(prompt)),
            prompt = prompt
        };
        context.SharedData.RTMPRequests.Enqueue(req);

        context.Response.StatusCode = 302;
        context.Response.Redirect(new Uri(new Uri(context.SharedData.AppConfig.RedirectBaseURL), WebUtility.UrlEncode(prompt) + ".m3u8").ToString());
    }
}
