using BepInEx.Logging;
using EFT;
using EFT.Network;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paulov.Tarkov.MP2.Patches
{
    internal sealed class AbstractSessionProcessDataEncryptionRemovalPatch : NullPaulovHarmonyPatch
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(nameof(AbstractSessionProcessDataEncryptionRemovalPatch));

        private static FieldInfo fieldInfo_dictionary_0 =
            typeof(AbstractSession).GetField("dictionary_0", BindingFlags.NonPublic | BindingFlags.Instance);

        public override IEnumerable<MethodBase> GetMethodsToPatch()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            Type classType = typeof(AbstractSession);

            MethodInfo method = classType.GetMethods(flags).FirstOrDefault(
                m =>
                    //m.GetParameters().Length >= 4 &&
                    //m.GetParameters()[0].Name == "channel"
                    //&& m.GetParameters()[1].Name == "messageType"
                    //&& m.GetParameters()[2].Name == "buffer"
                    //&& m.GetParameters()[3].Name == "bufferCount"
                    m.Name == "method_0"
                );

            if (method is null) throw new NullReferenceException($"{nameof(AbstractSessionProcessDataEncryptionRemovalPatch)}.{nameof(GetMethodsToPatch)} could not find method!");


            Logger.LogDebug($"{nameof(AbstractSessionProcessDataEncryptionRemovalPatch)}.{nameof(GetMethodsToPatch)}:{method.Name}");

            yield return method;
        }

        public override HarmonyMethod GetPrefixMethod()
        {
            return new HarmonyMethod(this.GetType().GetMethod(nameof(PrefixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
        }

        public static bool PrefixOverrideMethod(AbstractSession __instance, NetworkChannel channel, short messageType, byte[] buffer, int bufferCount)
        {
            var dictionary_0 = fieldInfo_dictionary_0.GetValue(__instance) as Dictionary<short, Action<NetworkChannel, short, byte[], int>>;
            if (dictionary_0.TryGetValue(messageType, out var value))
            {
                ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);
                value(channel, messageType, arraySegment.Array, arraySegment.Count);
            }

            return false; // Prevent the original method from executing
        }
    }
}
