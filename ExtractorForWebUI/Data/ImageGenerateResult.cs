namespace ExtractorForWebUI.Data;

public sealed class ImageGenerateResult
{
    public int imageCount;
    public string saveDirectory;
    public string prompt;
    public byte[] imageData;
    public string fileFormat;

    public string fileName;

    public int width;
    public int height;
}

public interface IGetImageResult
{
    public void OnGetImageResult(ImageGenerateResult result);
}