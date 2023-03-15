using ExtractorForWebUI.Data.Config;
using System.Collections.Generic;

namespace ExtractorForWebUI.SDConnection;

public class SDWebUIConfig
{
    public ConfigData configData;

    Dictionary<int, ConfigComponent> configComponentsMap;

    public GradioFillData txt2img;
    public GradioFillData img2img;

    public void Config(ConfigData configData)
    {
        this.configData = configData;
        configComponentsMap = new Dictionary<int, ConfigComponent>();
        foreach (var component in configData.components)
        {
            configComponentsMap[component.id] = component;
        }
        AddPrefix(configData);


        for (int i = 0; i < configData.dependencies.Length; i++)
        {
            ConfigDataDependency dependency = configData.dependencies[i];
            if (dependency.trigger == "click" && dependency.js == "submit")
            {
                txt2img = new GradioFillData();
                txt2img.InputOutput(i, dependency, configComponentsMap);
            }
            else if (dependency.trigger == "click" && dependency.js == "submit_img2img")
            {
                img2img = new GradioFillData();
                img2img.InputOutput(i, dependency, configComponentsMap);
            }
        }
    }

    void AddPrefix(ConfigData configData)
    {
        FindTargetComponent(configData.layout);
    }

    void FindTargetComponent(ConfigLayout layout)
    {
        if (layout.children == null)
        {
            return;
        }
        if (configComponentsMap.TryGetValue(layout.id, out var component))
        {
            switch (component.props.elem_id)
            {
                case "controlnet":
                    foreach (var child in layout.children)
                    {
                        ComponentAddPrefix(child, "ControlNet_");
                    }
                    break;
                default:
                    foreach (var child in layout.children)
                    {
                        FindTargetComponent(child);
                    }
                    break;
            }
        }
        else
        {
            foreach (var child in layout.children)
            {
                FindTargetComponent(child);
            }
        }
    }

    void ComponentAddPrefix(ConfigLayout layout, string prefix)
    {
        if (configComponentsMap.TryGetValue(layout.id, out var component))
        {
            component.extraPrefix = prefix;
        }

        if (layout.children != null)
            foreach (var child in layout.children)
            {
                ComponentAddPrefix(child, prefix);
            }
    }
}
