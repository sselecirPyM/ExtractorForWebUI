using ExtractorForWebUI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class IOService
{
    public void Tick()
    {
        int count = sharedData.imageGenerateResults.Count;

        Span<char> t = stackalloc char[1024];
        for (int i = 0; i < count; i++)
        {
            sharedData.imageGenerateResults.TryDequeue(out var result);
            string path1 = path;
            if (result.request.saveDirectory != null)
                path1 = Environment.ExpandEnvironmentVariables(result.request.saveDirectory);
            var imagesDir = new DirectoryInfo(path1);

            if (!imagesDir.Exists)
                imagesDir.Create();

            var spanWriter = new SpanWriter<char>(t);
            spanWriter.Write(DateTime.Now.ToString("yyyyMMddhhmmssff"));
            spanWriter.Write((char)((f) % 10 + '0'));
            spanWriter.Write((char)((f / 10) % 10 + '0'));
            spanWriter.Write(".png");
            f = (f + 1) % 100;
            var task = File.WriteAllBytesAsync(Path.Combine(path1, t[..spanWriter.Count].ToString()), result.imageData);
        }
    }

    int f = 0;

    public string path = Path.GetFullPath("Images/");

    public IOService(ServiceSharedData serviceSharedData)
    {
        this.sharedData = serviceSharedData;
    }
    ServiceSharedData sharedData;

}
