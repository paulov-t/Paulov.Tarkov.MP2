using Comfort.Common;
using CustomPlayerLoopSystem;
using EFT;
using EFT.Game.Spawning;
using EFT.UI;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using Paulov.Tarkov.MP2.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Paulov.Tarkov.MP2;

/// <summary>
/// Represents a Harmony patch for creating a networked game in the Tarkov application.
/// </summary>
/// <remarks>This class is responsible for identifying and patching the appropriate method in the Tarkov
/// application to enable the creation of a networked game. It overrides the original method's behavior to initialize
/// network game components, establish connections, and configure game settings.</remarks>
public sealed class CreateNetworkGamePatch : NullPaulovHarmonyPatch
{
    public override IEnumerable<MethodBase> GetMethodsToPatch()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        Type classType = typeof(TarkovApplication);

        MethodInfo method = classType.GetMethods(flags).FirstOrDefault(
            m =>
                m.GetParameters().Length >= 5 &&
                m.GetParameters()[0].Name == "profileStatus"
                && m.GetParameters()[1].Name == "savageProfile"
                && m.GetParameters()[2].ParameterType == typeof(ClientGameWorld)
                && m.GetParameters()[2].Name == "gameWorld"

            );

        if (method is null) throw new NullReferenceException($"{nameof(CreateNetworkGamePatch)}.{nameof(GetMethodsToPatch)} could not find method!");


        Plugin.Logger.LogDebug($"{nameof(CreateNetworkGamePatch)}.{nameof(GetMethodsToPatch)}:{method.Name}");

        yield return method;
    }

    public override HarmonyMethod GetPrefixMethod()
    {
        return new HarmonyMethod(this.GetType().GetMethod(nameof(PrefixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
    }

    public override HarmonyMethod GetPostfixMethod()
    {
        return new HarmonyMethod(this.GetType().GetMethod(nameof(PostfixOverrideMethod), BindingFlags.Public | BindingFlags.Static));
    }

    public static bool PrefixOverrideMethod(
        TarkovApplication __instance
        , ProfileStatusClass profileStatus
        , Profile savageProfile
        , ClientGameWorld gameWorld
        , GClass2514 metricsEvents
        , MetricsConfigClass metricsConfig
        , RaidSettings ____raidSettings
        , EFT.InputSystem.InputTree ____inputTree
        )
    {
        Plugin.Logger.LogDebug($"{nameof(CreateNetworkGamePatch)}.{nameof(PrefixOverrideMethod)} called");
        return false;
    }


    public static async void PostfixOverrideMethod(
      TarkovApplication __instance
      , ProfileStatusClass profileStatus
      , Profile savageProfile
      , ClientGameWorld gameWorld
      , GClass2514 metricsEvents
      , MetricsConfigClass metricsConfig
      , RaidSettings ____raidSettings
      , EFT.InputSystem.InputTree ____inputTree
      )
    {
        Plugin.Logger.LogDebug($"{nameof(CreateNetworkGamePatch)}.{nameof(PostfixOverrideMethod)} called");


        Plugin.Logger.LogDebug($"{nameof(CreateNetworkGamePatch)}.{nameof(gameWorld)}.{gameWorld}");

        // Keep the original method's functionality of Logging
        __instance.Logger.LogDebug("TRACE-NetworkGameCreate 3");
        __instance.Logger.LogDebug(string.Format("TRACE-NetworkGameCreate profileStatus: '{0}'", profileStatus.ToString()));

        // Create the NetworkGame and connections here
        var gameObject = new GameObject("NetworkGame");
        gameObject.AddComponent<GameServer>();
        Plugin.Logger.LogDebug("Created and added GameServer");

        var gameClient = gameObject.AddComponent<GameClient>();
        gameClient.ConnectToIpAndPortAndStart(profileStatus.ip, profileStatus.port);
        Plugin.Logger.LogDebug("Created and added GameClient");

        // Paulov: This is the first attempt to create a network game using BSG's code.
        LocationSettingsClass.Location location = ____raidSettings.SelectedLocation;
        TimeSpan sessionTime = TimeSpan.FromMinutes(location.EscapeTimeLimit);
        EftNetworkGame game = EftNetworkGame.Create(
            Plugin.BackEndSession
            , gameWorld
            , profileStatus
            , ____raidSettings
            , savageProfile
            , Plugin.BackEndSession.InsuranceCompany
            , ____inputTree
            , MonoBehaviourSingleton<GameUI>.Instance, metricsEvents, new GClass2503(metricsConfig, __instance)
            , EUpdateQueue.Update
            , sessionTime
            , delegate
            {
                Plugin.Logger.LogDebug("TRACE-NetworkGameCreate 6");
                ScreenManager.Instance.CloseAllScreensForced();
                UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
                //_menuOperation?.Unsubscribe();
                gameWorld.OnGameStarted();
            }, delegate (Result<ExitStatus, TimeSpan, MetricsClass> result)
            {
                __instance.method_52(profileStatus.profileid, savageProfile, location, result);
            });
        Singleton<AbstractGame>.Create(game);

        __instance.Logger.LogDebug("TRACE-NetworkGameCreate 4");

        var myProfile = __instance.GetClientBackEndSession().Profile;
        gameClient.LoadProfiles.Add(myProfile);

        await Task.Run(async () =>
        {
            while (!gameClient.LoadedProfiles.Any(x => x == myProfile.Id))
            {
                // Wait for the profile to be loaded
                await Task.Delay(1000);
                Plugin.Logger.LogDebug($"Waiting for Profile to Load {myProfile.Info.Nickname}.");
            }

            Plugin.Logger.LogDebug($"{myProfile.Info.Nickname}::Load Complete.");
        });

        var networkGameSession = PaulovNetworkGameSession.Create(game, myProfile.Id, myProfile.Id, null, () =>
        {
            Plugin.Logger.LogDebug($"PaulovNetworkGameSession created for {myProfile.Info.Nickname}");
        });

        metricsEvents.SetGameCreated();

        __instance.Logger.LogDebug("TRACE-NetworkGameCreate 5", Array.Empty<object>());

        MonoBehaviourSingleton<PreloaderUI>.Instance.SetSessionId(profileStatus.shortId);

        Plugin.Logger.LogDebug($"--> Run");
        await networkGameSession.OnAcceptGamePacket(new OnAcceptResponseSettingsPacket(), game);

        var tasks = new List<Task>();
        // This requires game.Run to be called first, which is done above.
        var worldSpawnNetworkMessage = new WorldSpawnPacket().ToNetworkMessage();
        Plugin.Logger.LogDebug($"--> WorldSpawn");
        var worldSpawnTask = ((Interface10)game).WorldSpawn(worldSpawnNetworkMessage);
        await Task.Yield(); // Yield to allow the world spawn to process

        Plugin.Logger.LogDebug($"--> WorldSpawnLoot");
        ((Interface10)game).WorldSpawnLoot(new WorldSpawnLootPacket().ToNetworkMessage());

        //await worldSpawnTask;

        Plugin.Logger.LogDebug($"--> Game Spawn");
        ((Interface10)game).Spawn();

        SpawnSystemClass2 spawnPoints = SpawnSystemClass2.CreateFromScene(GClass1507.LocalDateTimeFromUnixTime(location.UnixDateTime), location.SpawnPointParams);
        int spawnSafeDistance = ((location.SpawnSafeDistanceMeters > 0) ? location.SpawnSafeDistanceMeters : 100);
        SpawnSystemSettings settings = new SpawnSystemSettings(location.MinDistToFreePoint, location.MaxDistToFreePoint, location.MaxBotPerZone, spawnSafeDistance, location.NoGroupSpawn, location.OneTimeSpawn);
        ISpawnSystem1 spawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, () => Time.time, gameWorld, new BotsController(), spawnPoints);
        ISpawnPoint spawnPoint = spawnSystem.SelectSpawnPoint(ESpawnCategory.Player, myProfile.Info.Side, null, null, null, null, myProfile.Id);
        // After the world spawn, we can send the player spawn packet.
        var playerSpawnPacket = new PlayerSpawnPacket(myProfile, spawnPoint.Position).ToArraySegment();
        Plugin.Logger.LogDebug($"--> PlayerSpawn");
        ((Interface10)game).PlayerSpawn(new BSGNetworkReader(playerSpawnPacket), async (x) =>
        {


        });

        Plugin.Logger.LogDebug($"--> GameSpawned");
        await ((Interface10)game).GameSpawned();

        Plugin.Logger.LogDebug($"--> GameStarting");
        game.GameStarting(10);
        await Task.Delay(10 * 1000);
        Plugin.Logger.LogDebug($"--> GameStarted");
        game.GameStarted(45 * 60, 45 * 60);
    }

    internal class PaulovNetworkGameSession : EFT.NetworkGameSession
    {
        public static PaulovNetworkGameSession Create(Interface10 game, string profileId, string token, AbstractLogger logger, Action onConnected = null)
        {
            UNetUpdate.OnUpdate += BSGNetworkManager.Default.Update;
            string text = "Play session(" + profileId + ")";
            PaulovNetworkGameSession networkGameSession = AbstractGameSession.Create<PaulovNetworkGameSession>(game.Transform, text, profileId, token);
            BSGNetworkManager.Default.AddMessageListener(81, networkGameSession.method_3);
            BSGNetworkManager.Default.AddMessageListener(82, networkGameSession.method_4);
            BSGNetworkManager.Default.AddMessageListener(83, networkGameSession.method_9);
            BSGNetworkManager.Default.AddMessageListener(84, networkGameSession.method_5);
            BSGNetworkManager.Default.AddMessageListener(91, networkGameSession.method_8);
            //networkGameSession.method_18();
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<GInvokedEvent>(networkGameSession.method_16));
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<InvokedEvent>(networkGameSession.method_17));
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3418>(networkGameSession.method_15));
            //networkGameSession.class1551_0 = Class1551.smethod_0(networkGameSession);
            //networkGameSession.interface10_0 = game;
            //networkGameSession.action_1 = onConnected;
            //networkGameSession.class400_0 = new Class400(LoggerMode.Add);
            //networkGameSession.ObserveOnly = false;
            //networkGameSession.gclass703_0 = logger;
            typeof(EFT.NetworkGameSession).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => f.FieldType == typeof(Interface10))
                .SetValue(networkGameSession, game);
            return networkGameSession;
        }

        public async Task OnAcceptGamePacket(OnAcceptResponseSettingsPacket packet, Interface10 game)
        {
            string text = "";// SimpleZlib.Decompress(response.byte_0);
            ResourceKey[] lootArray = Array.Empty<ResourceKey>();// GClass843.ParseJsonTo<ResourceKey[]>(text, Array.Empty<JsonConverter>());
            string text2 = "";// SimpleZlib.Decompress(response.byte_1);
            string[] customizationArray = Array.Empty<string>();// GClass843.ParseJsonTo<string[]>(text2, Array.Empty<JsonConverter>());
            Plugin.Logger.LogInfo(string.Format("{0}::Resources ({1}):\n{2}", "OnAcceptResponse", lootArray.Length, text));
            Plugin.Logger.LogInfo(string.Format("{0}::Customization ids ({1}):\n{2}", "OnAcceptResponse", customizationArray.Length, text2));
            WeatherClass[] weathers = new WeatherClass[1] { new WeatherClass() };// GClass843.ParseJsonTo<WeatherClass[]>(SimpleZlib.Decompress(response.byte_2), Array.Empty<JsonConverter>());
            float fixedDeltaTime = 0.066f;
            Dictionary<string, int> interactables = new Dictionary<string, int>();// GClass843.ParseJsonTo<Dictionary<string, int>>(SimpleZlib.Decompress(response.byte_3), Array.Empty<JsonConverter>());
            //GClass1636.SetupPositionQuantizer(response.bounds_0);
            //base.NetworkCryptography = new GClass2934(response.bool_0, response.bool_1);
            await game.Run(
                session: this
                , canRestart: false
                , new GameDateTime(DateTime.Now, DateTime.Now, 7, false)
                , interactables
                , prefabs: lootArray
                , customizations: customizationArray
                , weathers: weathers
                , season: ESeason.Summer // response.eseason_0
                , new SeasonsSettings1() { SpringSnowFactor = Vector3.zero } // response.gclass2477_0
                , fixedDeltaTime
                , speedLimitsEnabled: true // response.bool_3
                , new GClass2005.Config() { DefaultPlayerStateLimits = new GClass2005.PlayerStateLimits() } // response.config_0
                , GClass2104.Default // response.gclass2104_0
                );
        }

        public new void Update()
        {

        }
    }
}

