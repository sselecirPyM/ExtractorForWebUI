using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Data;

public sealed class ImageGenerateRequest 
{
    public string prompt { get; set; }
    public string negativePrompt { get; set; }
    public string sampleMethod { get; set; } = "Euler a";
    public float cfgScale { get; set; } = 12;
    public int seed { get; set; } = -1;
    public int subSeed { get; set; } = -1;
    public float subSeedStrength { get; set; }
    public int imageCount { get; set; } = 1;
    public int step { get; set; } = 28;

    public int width { get; set; } = 512;
    public int height { get; set; } = 512;
    public float denoiseStrenth { get; set; } = 0.7f;

    public bool restore_faces { get; set; } = false;
    public bool tiling { get; set; } = false;

    public string saveDirectory { get; set; }

    public ImageGenerateRequest Clone()
    {
        return (ImageGenerateRequest)MemberwiseClone();
    }

    public bool IsSameRequest(ImageGenerateRequest anotherRequest)
    {
        var request = anotherRequest;
        return 
               prompt == request.prompt &&
               negativePrompt == request.negativePrompt &&
               sampleMethod == request.sampleMethod &&
               cfgScale == request.cfgScale &&
               seed == request.seed &&
               subSeed == request.subSeed &&
               subSeedStrength == request.subSeedStrength &&
               //imageCount == request.imageCount &&
               step == request.step &&
               width == request.width &&
               height == request.height &&
               denoiseStrenth == request.denoiseStrenth &&
               restore_faces == request.restore_faces &&
               tiling == request.tiling &&
               saveDirectory == request.saveDirectory;
    }
}
