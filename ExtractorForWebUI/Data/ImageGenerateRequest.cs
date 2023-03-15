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

    public bool restoreFaces { get; set; } = false;
    public bool tiling { get; set; } = false;
    public bool highresFix { get; set; } = false;

    public string saveDirectory { get; set; }
    public string saveFileName { get; set; }

    public string img2imgFile { get; set; }

    public string img2imgFileData;

    public ControlNetParameters controlNet { get; set; }

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
               restoreFaces == request.restoreFaces &&
               tiling == request.tiling &&
               highresFix == request.highresFix &&
               controlNet == request.controlNet &&
               saveDirectory == request.saveDirectory;
    }
}

public sealed class ControlNetParameters
{
    //public bool enable { get; set; }
    public string preprocessor { get; set; } = "canny";
    public string model { get; set; }

    public string resizeMode { get; set; } = "Scale to Fit (Inner Fit)";
    public float weight { get; set; } = 1;
    public bool invertColor { get; set; } = false;
    public bool lowVram { get; set; } = true;
    public bool guessMode { get; set; } = false;

    public float guidanceStart { get; set; } = 0;
    public float guidanceEnd { get; set; } = 1;


    public int thresholdA { get; set; } = 100;
    public int thresholdB { get; set; } = 200;
}
