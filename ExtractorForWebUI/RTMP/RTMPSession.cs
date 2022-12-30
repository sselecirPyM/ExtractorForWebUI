using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading.Tasks;

namespace ExtractorForWebUI.RTMP;

public class RTMPSession : IDisposable
{
    public Process process;

    public NamedPipeServerStream pipe;

    public (int, int) size;

    public (int, int) presentSize;

    public string url;

    public long lastActivate;

    public float fps = 30;

    public string prompt;

    public string md5;

    public string saveDir;

    bool running;

    public async Task Run()
    {
        Debug.Assert(!running);
        Debug.Assert(pipe == null);

        running = true;

        string pipeName = Path.GetRandomFileName();
        pipe = new NamedPipeServerStream(pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.None, 0, 1024 * 1024);
        _ = Task.Run(() =>
        {
            string[] args =
            {
                        "-re",
                        "-i",
                        @"\\.\pipe\" + pipeName,
                        //"-r",
                        //fps.ToString(),
                        //"-vf",
                        //"fps="+fps.ToString(),
                        "-c:v",
                        "libx264",
                        "-s",
                        presentSize.Item1 + "X" + presentSize.Item2,
                        "-pix_fmt",
                        "yuv420p",
                        //"-threads",
                        //"1",
                        "-preset:v",
                        //"ultrafast",
                        //"superfast",
                        "faster",
                        "-tune:v",
                        "stillimage",
                        //"zerolatency",
                        "-f",
                        "flv",
                        url,
            };
            var processStartInfo = new ProcessStartInfo();
            processStartInfo.FileName = "ffmpeg";
            foreach (var arg in args)
                processStartInfo.ArgumentList.Add(arg);
            process = Process.Start(processStartInfo);
        });
        pipe.WaitForConnection();

        var fonts = new FontCollection();
        fonts.AddSystemFonts();

        font = new Font(fonts.Families.First(), 40, FontStyle.Bold);
        _ = Run1();
        while (running)
        {
            await Tick();
            await Task.Delay(1);
        }
    }

    async Task Run1()
    {
        while (running)
        {
            await Tick1();
            await Task.Delay(1);
        }
    }

    async Task Tick1()
    {
        if (queue.Count >= 4)
        {
            var (width, height) = presentSize;
            if (image1 == null)
            {
                image1 = new byte[width * height * 3 + 54];
                memoryStream = new MemoryStream();
            }
            Image<Rgb24>[] images = new Image<Rgb24>[4];
            for (int i = 0; i < 4; i++)
            {
                queue.TryDequeue(out var imgData);
                images[i] = Image.Load<Rgb24>(imgData);
            }
            //this.image = image1;
            Image<Rgb24> img = Image.WrapMemory<Rgb24>(image1, width, height);
            img.Mutate(x =>
            x.DrawImage(images[0], new Point(0, 0), 1)
            .DrawImage(images[1], new Point(0, 512), 1)
            .DrawImage(images[2], new Point(1024, 0), 1)
            .DrawImage(images[3], new Point(1024, 512), 1)
            .DrawText(DateTime.Now + "Prompt: " + prompt, font, Color.Cyan, new PointF(20, 20))
            .DrawText("Count: " + imageCount, font, Color.White, new PointF(20, 100)));
            imageCount++;

            memoryStream.Seek(0, SeekOrigin.Begin);
            img.SaveAsBmp(memoryStream);
            this.image = memoryStream.ToArray();
            memoryStream.Seek(0, SeekOrigin.Begin);
            img.Dispose();
        }
    }
    async Task Tick()
    {
        if (image != null)
        {
            var image2 = image;

            await pipe.WriteAsync(image2);
        }
    }

    public ConcurrentQueue<byte[]> queue = new();

    byte[] image = null;
    byte[] image1 = null;

    MemoryStream memoryStream;

    Font font;

    int imageCount = 0;

    public void Dispose()
    {
        running = false;
        pipe.Dispose();

    }
}
