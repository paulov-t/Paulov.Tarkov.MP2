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
    public class AbstractSessionSendEncryptionRemovalPatch : NullPaulovHarmonyPatch
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("AbstractSessionSendEncryptionRemovalPatch");

        public override IEnumerable<MethodBase> GetMethodsToPatch()
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            Type classType = typeof(AbstractSession);

            MethodInfo method = classType.GetMethods(flags).FirstOrDefault(
                m =>
                    m.GetParameters().Length >= 5 &&
                    m.GetParameters()[0].Name == "channel"
                    && m.GetParameters()[1].Name == "messageType"
                    && m.GetParameters()[2].Name == "buffer"
                    && m.GetParameters()[3].Name == "bufferOffset"
                    && m.GetParameters()[4].Name == "bufferCount"

                );

            if (method is null) throw new NullReferenceException($"{nameof(AbstractSessionSendEncryptionRemovalPatch)}.{nameof(GetMethodsToPatch)} could not find method!");


            Logger.LogDebug($"{nameof(AbstractSessionSendEncryptionRemovalPatch)}.{nameof(GetMethodsToPatch)}:{method.Name}");

            yield return method;
        }

        public override HarmonyMethod GetPrefixMethod()
        {
            return new HarmonyMethod(this.GetType().GetMethod(nameof(PrefixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
        }

        public static bool PrefixOverrideMethod(AbstractSession __instance, NetworkChannel channel, NetworkMessageType messageType, byte[] buffer, int bufferOffset, int bufferCount)
        {
            if (__instance.Connection != null)
            {
                //Logger.LogDebug($"{nameof(AbstractSessionSendEncryptionRemovalPatch)}.{nameof(PrefixOverrideMethod)}: Sending message of type {messageType} on channel {channel}");
                ArraySegment<byte> arraySegment = new ArraySegment<byte>(buffer);
                __instance.Connection.Send(channel, (short)messageType, arraySegment.Array, arraySegment.Offset, arraySegment.Count);
            }

            return false; // Prevent the original method from executing
        }
    }
}
