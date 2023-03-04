namespace ExtractorForWebUI.Data.Config;


public class ConfigData
{
    public string version { get; set; }
    public string title { get; set; }
    public ConfigComponent[] components { get; set; }
    public ConfigDataDependency[] dependencies { get; set; }
}

public class ConfigComponent
{
    public int id { get; set; }
    public string type { get; set; }
    public ConfigComponentProps props { get; set; }
}

public class ConfigComponentProps
{
    public string type { get; set; }
    public string label { get; set; }
    public object value { get; set; }
    public string elem_id { get; set; }
}

public class ConfigDataDependency
{
    public string trigger { get; set; }
    public string js { get; set; }

    public int[] inputs { get; set; }
    public int[] outputs { get; set; }
}
