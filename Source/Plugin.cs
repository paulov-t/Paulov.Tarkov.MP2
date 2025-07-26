using BepInEx;
using BepInEx.Logging;
using Comfort.Common;
using EFT;
using Paulov.Bepinex.Framework;
using Paulov.Tarkov.Matchmaking;

namespace Paulov.Tarkov.MP2;

[BepInDependency("Paulov.Bepinex.Framework", BepInDependency.DependencyFlags.HardDependency)]
[BepInPlugin("Paulov.Tarkov.MP2", "Paulov.Tarkov.MP2", "2025.7.19")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private static ISession _backEndSession;
    public static ISession BackEndSession
    {
        get
        {
            if (_backEndSession == null && Singleton<TarkovApplication>.Instantiated)
            {
                _backEndSession = Singleton<TarkovApplication>.Instance.GetClientBackEndSession();
            }

            if (_backEndSession == null && Singleton<ClientApplication<ISession>>.Instantiated)
            {
                _backEndSession = Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession();
            }

            return _backEndSession;
        }
    }

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogDebug("Paulov's Tarkov MP2 Plugin Awake");



        LocalGameMethodLog.PatchAllMethods();

        HarmonyPatchManager hpm = new("Paulov's MP2 Harmony Manager", new PaulovTarkovMP2Provider());
        hpm.EnableAll();
    }
}
