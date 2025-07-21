using Paulov.Bepinex.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Paulov.Tarkov.MP2;

public class PaulovTarkovMP2Provider : IPatchProvider
{
    public IEnumerable<IPaulovHarmonyPatch> GetPatches()
    {
        IOrderedEnumerable<Type> assemblyTypes = GetType().Assembly.GetTypes().OrderBy(x => x.Name);
        foreach (Type type in assemblyTypes)
        {
            if (type.GetInterface(nameof(IPaulovHarmonyPatch)) is null) continue;
            yield return (IPaulovHarmonyPatch)Activator.CreateInstance(type);
        }
    }
}