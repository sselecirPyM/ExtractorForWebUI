namespace ExtractorForWebUI.Data;

public class ImageGenerateResult
{
    public int imageCount;
    public string saveDirectory;
    public string prompt;
    public byte[] imageData;
    public string fileFormat;

    public int width;
    public int height;
}
