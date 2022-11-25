using ExtractorForWebUI.Services;
using System;
using System.Text.Json;

namespace ExtractorForWebUI.WebServices;

public class AddTaskService : BaseWebService
{
    public override void Process(WebServiceContext context)
    {
        var Response = context.Response;
        var Request = context.Request;
        if (Request.ContentType == "application/json" && Request.HttpMethod == "POST")
        {
            try
            {
                var taskConfig = JsonSerializer.Deserialize<TaskConfig>(Request.InputStream);
                var generateRequests = ServiceSharedData.SharedData.imageGenerateRequests;
                foreach (var request in taskConfig.requests)
                {
                    generateRequests.Enqueue(request);
                }
                Console.WriteLine("Add {0} tasks to the list.", taskConfig.requests.Length);
                Response.StatusCode = 201;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Response.StatusCode = 400;
            }
        }
        else
        {
            Response.StatusCode = 405;
        }
    }
}
