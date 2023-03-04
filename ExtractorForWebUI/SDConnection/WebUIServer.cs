using ExtractorForWebUI.Data.Config;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace ExtractorForWebUI.SDConnection;

public class WebUIServer
{
    public int fn_index = -1;
    public int imageBatchSize = 1;

    public bool activate;
    public bool canUse;
    public int failCount;
    public Uri URL;
    public long timestamp;
    public bool isSSHConnect;

    public bool windows;

    public string internalName;

    public string viewName;

    public static WebUIServer FromConfig(WebUIServerConfig config)
    {
        return new WebUIServer
        {
            URL = (config.URL != null) ? new Uri(config.URL) : null,
            windows = config.Windows,
            activate = config.Activate,
            canUse = true,
            imageBatchSize = config.BatchSize,
            isSSHConnect = config.SSHConfig != null,
            timestamp = Stopwatch.GetTimestamp(),
            internalName = config.Name,
            viewName = config.Name,
        };
    }

    public ConfigData configData;

    Dictionary<int, ConfigComponent> configComponentsMap;
    List<string> inputList;
    List<object> inputValueList;

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
                fn_index = i;
                InputOutput(t);
            }
        }
        if (fn_index > -1)
        {
            Console.WriteLine("Server {0} Configured.", viewName);
        }

    }

    void InputOutput(ConfigDataDependency dependency)
    {
        inputList = new List<string>();
        inputValueList = new List<object>();
        foreach (int id in dependency.inputs)
        {
            var component = configComponentsMap[id];
            inputList.Add(component.props.elem_id);

            var obj = TranslateJsonObject(component.props.value);
            if (obj != null)
            {

            }
            else
            {
                if (component.type == "dropdown")
                    obj = new object[0];
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
            inputValueList.Add(obj);
        }
        foreach (int id in dependency.outputs)
        {
            var component = configComponentsMap[id];
            var obj = TranslateJsonObject(component.props.value);
            if (obj != null)
            {

            }
            else
            {
                if (component.type == "dropdown")
                    obj = new object[0];
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
            inputValueList.Add(obj);
        }
        inputList[0] = "Dummy";
        inputValueList[0] = "task(123456789)";
        //foreach(var t in inputList)
        //{
        //    Console.WriteLine(t);
        //}
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
                    return new object[0];
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
    public object[] FillDatas(IReadOnlyDictionary<string, object> dict)
    {
        object[] values = new object[inputValueList.Count];
        for (int i = 0; i < inputList.Count; i++)
        {
            if (inputList[i] == null || !dict.TryGetValue(inputList[i], out values[i]))
            {
                values[i] = inputValueList[i];
            }
        }
        for(int i= inputList.Count; i < inputValueList.Count; i++)
        {
            values[i] = inputValueList[i];
        }
        return values;
    }
}
