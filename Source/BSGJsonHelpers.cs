using EFT;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Paulov.Tarkov.MP2
{
    public static class BSGJsonHelpers
    {
        public static JsonConverter[] GetJsonConvertersBSG()
        {
            var tarkovTypes = typeof(TarkovApplication).Assembly.DefinedTypes;
            var convertersType = tarkovTypes.FirstOrDefault(x => x.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Any(p => p.Name == "Converters"));
            List<Newtonsoft.Json.JsonConverter> converters = new List<Newtonsoft.Json.JsonConverter>();
            if (convertersType != null)
                converters.AddRange((Newtonsoft.Json.JsonConverter[])convertersType.GetField("Converters", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).GetValue(null));
            converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            return converters.ToArray();
        }

    }
}
