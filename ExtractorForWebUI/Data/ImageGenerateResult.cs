using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Data;

public class ImageGenerateResult
{
    public int imageCount;
    public ImageGenerateRequest request;
    public byte[] imageData;
    public string fileFormat;
}
