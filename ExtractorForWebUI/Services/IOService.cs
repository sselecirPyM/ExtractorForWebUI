using ExtractorForWebUI.Data;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Services;

public class IOService : IGetImageResult
{
    ConcurrentQueue<ImageGenerateResult> imageGenerateResults = new();

    public async Task Run()
    {
        while (true)
        {
            Tick();
            await Task.Delay(1);
        }
    }

    public void Tick()
    {
        int count = imageGenerateResults.Count;

        Span<char> t = stackalloc char[1024];
        for (int i = 0; i < count; i++)
        {
            imageGenerateResults.TryDequeue(out var result);
            string path1 = path;
            if (result.saveDirectory != null)
                path1 = Environment.ExpandEnvironmentVariables(result.saveDirectory);
            var imagesDir = new DirectoryInfo(path1);

            if (!imagesDir.Exists)
                imagesDir.Create();

            if (result.fileName != null)
            {
                _ = File.WriteAllBytesAsync(Path.Combine(path1, result.fileName + result.fileFormat), result.imageData);
            }
            else
            {
                var spanWriter = new SpanWriter<char>(t);
                spanWriter.Write(DateTime.Now.ToString("MMddHHmmssff"));
                spanWriter.Write((char)((f) % 10 + '0'));
                spanWriter.Write((char)((f / 10) % 10 + '0'));
                spanWriter.Write(result.fileFormat);
                f = (f + 1) % 100;

                if (result.imageData.Length > result.width * result.height / 51.2)
                    _ = File.WriteAllBytesAsync(Path.Combine(path1, t[..spanWriter.Count].ToString()), result.imageData);
                else
                    Console.WriteLine("Ignore 1 black image.");
            }
        }
    }

    public void OnGetImageResult(ImageGenerateResult result)
    {
        imageGenerateResults.Enqueue(result);
    }

    int f = 0;

    public string path = Path.GetFullPath("Images/");

    public IOService(ServiceSharedData serviceSharedData)
    {
        this.sharedData = serviceSharedData;
    }
    ServiceSharedData sharedData;

}
