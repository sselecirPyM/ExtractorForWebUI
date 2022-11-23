using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Magic;

public class MagicObject : DynamicObject
{
    public override bool TryConvert(ConvertBinder binder, out object result)
    {
        var returnType = binder.ReturnType;
        var isGeneric = returnType.IsGenericType;
        var args = returnType.GenericTypeArguments;
        Dictionary<Type, Type[]> infos = new();
        Dictionary<Type, List<int>> invertInfos = new();
        Dictionary<Type, int> parameterIndex = new();
        object[] parameters = new object[args.Length];
        int[] invertCount = new int[args.Length];
        Stack<Type> stack = new();

        for (int i = 0; i < args.Length; i++)
        {
            Type arg = args[i];
            parameterIndex[arg] = i;
        }
        for (int i = 0; i < args.Length; i++)
        {
            Type arg = args[i];
            var constructors = arg.GetConstructors();
            var parameters1 = constructors[0].GetParameters().Select(u => u.ParameterType).ToArray();
            infos[arg] = parameters1;
            invertCount[i] = parameters1.Length;
            foreach (var parameter in parameters1)
            {
                if (!invertInfos.TryGetValue(parameter, out var ints))
                {
                    ints = invertInfos[parameter] = new List<int>();
                }
                ints.Add(i);
            }
            if (parameters1.Length == 0)
            {
                stack.Push(arg);
            }
        }

        Debug.Assert(stack.Count > 0);
        while (stack.TryPop(out var t))
        {
            var info = infos[t];
            object[] parameters1 = new object[info.Length];
            for (int i = 0; i < info.Length; i++)
            {
                var param = info[i];
                int pindex = parameterIndex[param];
                parameters1[i] = parameters[pindex];
            }
            if (invertInfos.TryGetValue(t, out var counts))
                foreach (var k in counts)
                {
                    invertCount[k]--;
                    if (invertCount[k] == 0)
                    {
                        stack.Push(args[k]);
                    }
                }

            int thisIndex = parameterIndex[t];
            parameters[thisIndex] = Activator.CreateInstance(t, parameters1);
        }
        foreach (var param in parameters)
        {
            Debug.Assert(param != null);
        }

        result = Activator.CreateInstance(returnType, parameters);
        return true;
    }
}
