using ExtractorForWebUI.Data.Config;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ExtractorForWebUI.SDConnection;

public class GradioFillData
{
    public int fn_index { get; private set; }

    List<string> inputList;
    List<object> defaultValueList;

    public void InputOutput(int fn_index, ConfigDataDependency dependency, IReadOnlyDictionary<int, ConfigComponent> configComponentsMap)
    {
        this.fn_index = fn_index;
        inputList = new List<string>();
        defaultValueList = new List<object>();
        foreach (int id in dependency.inputs)
        {
            var component = configComponentsMap[id];
            if (component.props.elem_id != null)
                inputList.Add(component.extraPrefix + component.props.elem_id);
            else
                inputList.Add(component.extraPrefix + component.props.label);

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
                    {
                        var objs = new object[element.GetArrayLength()];
                        int i = 0;
                        foreach (var item in element.EnumerateArray())
                        {
                            objs[i] = TranslateJsonObject(item);
                            i++;
                        }
                        return objs;
                    }
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


    public object[] FillDatas(IReadOnlyDictionary<string, object> dict)
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
