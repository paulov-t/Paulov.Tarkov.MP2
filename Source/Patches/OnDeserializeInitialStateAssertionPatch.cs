//using BepInEx.Logging;
//using Comfort.Common;
//using HarmonyLib;
//using Paulov.Bepinex.Framework.Patches;
//using Paulov.Tarkov.MP2.Packets;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace Paulov.Tarkov.MP2.Patches
//{
//    internal class OnDeserializeInitialStateAssertionPatch : NullPaulovHarmonyPatch
//    {
//        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("AbstractSessionSendEncryptionRemovalPatch");

//        public override IEnumerable<MethodBase> GetMethodsToPatch()
//        {
//            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

//            Type classType = typeof(EFT.ClientPlayer);

//            MethodInfo method = classType.GetMethods(flags).FirstOrDefault(
//                m => m.Name == "OnDeserializeInitialState"

//                );

//            if (method is null) throw new NullReferenceException($"{nameof(OnDeserializeInitialStateAssertionPatch)}.{nameof(GetMethodsToPatch)} could not find method!");


//            Logger.LogDebug($"{nameof(OnDeserializeInitialStateAssertionPatch)}.{nameof(GetMethodsToPatch)}:{method.Name}");

//            yield return method;
//        }

//        public override HarmonyMethod GetPrefixMethod()
//        {
//            return new HarmonyMethod(this.GetType().GetMethod(nameof(PrefixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
//        }

//        public static bool PrefixOverrideMethod(BSGNetworkReader networkReader, Callback callback)
//        {
//            Logger.LogDebug($"{nameof(OnDeserializeInitialStateAssertionPatch)}.{nameof(PrefixOverrideMethod)}");

//            var positionBeforeAssertion = networkReader.Position;
//            networkReader.Position = 0;
//            PlayerSpawnPacket.AssertData(networkReader);

//            networkReader.Position = positionBeforeAssertion;
//            return true;
//        }
//    }
//}
