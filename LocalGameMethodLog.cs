using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;
using System.Reflection;

namespace Paulov.Tarkov.Matchmaking
{
    public sealed class LocalGameMethodLog
    {
        private static ManualLogSource logger = BepInEx.Logging.Logger.CreateLogSource(nameof(LocalGameMethodLog));

        public static void PatchAllMethods()
        {
            // Get the type of TarkovApplication
            Type type = typeof(EFT.LocalGame);

            // Get all methods in TarkovApplication
            var methods = type
                .GetMethods(
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public)
                .Where(x => x.DeclaringType == type) // Ensure we only get methods declared in LocalGame
                .Where(x => !x.Name.Equals("LateUpdate") && !x.Name.Equals("Update") && !x.Name.Equals("FixedUpdate")) // Exclude common Unity methods
                .Where(x => !x.Name.StartsWith("get_") && !x.Name.StartsWith("set_") && !x.Name.StartsWith("StartCoroutine"));

            // Iterate through each method and patch it
            foreach (var method in methods)
            {
                // Create Harmony instance for the method
                var harmony = new Harmony($"{type.GetType()}.{method.Name}");

                // Create a Harmony patch for the method
                var harmonyMethod = new HarmonyMethod(typeof(LocalGameMethodLog).GetMethod("Prefix"), debug: false);
                try
                {
                    // Patch the method with the prefix
                    harmony.Patch(method, harmonyMethod, null, null, null, null);

                    logger.LogInfo($"Patching {method.Name}");
                }
                catch
                {
                    harmony.Unpatch(method, HarmonyPatchType.All);

                    //Plugin.Logger.LogError($"Failed to patch {method.Name}");
                }
            }
        }

        public static void Prefix(MethodBase __originalMethod)
        {
            logger.LogInfo($"{nameof(EFT.LocalGame)}.{__originalMethod.Name}");
        }

    }
}
