using ExtractorForWebUI.Data.Config;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ExtractorForWebUI.SDConnection;

public class SDWebUIConfig
{
    public int txt2img_fn_index = -1;
    public int img2img_fn_index = -1;
    public ConfigData configData;

    Dictionary<int, ConfigComponent> configComponentsMap;
    List<string> inputList;
    List<object> defaultValueList;

    public void Config(ConfigData configData)
    {
        this.configData = configData;

        configComponentsMap = new Dictionary<int, ConfigComponent>();
        foreach (var component in configData.components)
        {
            configComponentsMap[component.id] = component;
        }

        for (int i = 0; i < configData.dependencies.Length; i++)
        {
            ConfigDataDependency t = configData.dependencies[i];
            if (t.trigger == "click" && t.js == "submit")
            {
                txt2img_fn_index = i;
                (inputList, defaultValueList) = InputOutput(t);
            }
            else if (t.trigger == "click" && t.js == "submit_img2img")
            {
                img2img_fn_index = i;

                List<string> inputList1;
                List<object> defaultValueList1;
                (inputList1, defaultValueList1) = InputOutput(t);
            }
        }
    }

    (List<string>, List<object>) InputOutput(ConfigDataDependency dependency)
    {
        var inputList = new List<string>();
        var defaultValueList = new List<object>();
        foreach (int id in dependency.inputs)
        {
            var component = configComponentsMap[id];
            if (component.props.elem_id != null)
                inputList.Add(component.props.elem_id);
            else
                inputList.Add(component.props.label);

            var obj = TranslateJsonObject(component.props.value);
            obj = ObjectReplace(obj, component);

            defaultValueList.Add(obj);
        }
        foreach (int id in dependency.outputs)
        {
            var component = configComponentsMap[id];

            var obj = TranslateJsonObject(component.props.value);
            obj = ObjectReplace(obj, component);

            defaultValueList.Add(obj);
        }
        inputList[0] = "Dummy";
        defaultValueList[0] = "task(123456789)";

        return (inputList, defaultValueList);
    }

    static object ObjectReplace(object obj, ConfigComponent component)
    {
        if (obj != null)
        {

        }
        else
        {
            if (component.type == "dropdown")
            {
                obj = Array.Empty<object>();
            }
            else if (component.type == "state")
            {

            }
            else if (component.type == "image")
            {

            }
            else if (component.type == "video")
            {

            }
            else
            {

            }
        }
        return obj;
    }

    static object TranslateJsonObject(object obj)
    {
        if (obj == null)
        {
            return null;
        }
        else if (obj is JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    if (element.TryGetInt32(out int a))
                    {
                        return a;
                    }
                    else
                    {
                        return element.GetDouble();
                    }
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Object:
                    return obj;
                case JsonValueKind.Array:
                    return Array.Empty<object>();
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.Null:
                    return null;
                case JsonValueKind.Undefined:
                    return null;
            }
            return null;
        }
        else
        {
            return obj;
        }
    }

    // "txt2img_override_settings"= [];
    // "state" = null
    // "image" = null
    // "video_0" = null
    // 65 = []
    // 66 = ""
    // 67 = ""
    // 68 = ""
    public object[] FillDatasTxt2Img(IReadOnlyDictionary<string, object> dict)
    {
        object[] values = new object[defaultValueList.Count];
        for (int i = 0; i < inputList.Count; i++)
        {
            if (inputList[i] == null || !dict.TryGetValue(inputList[i], out values[i]))
            {
                values[i] = defaultValueList[i];
            }
        }
        for (int i = inputList.Count; i < defaultValueList.Count; i++)
        {
            values[i] = defaultValueList[i];
        }
        return values;
    }
}
