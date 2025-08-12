using BepInEx.Logging;
using Comfort.Common;
using ComponentAce.Compression.Libs.zlib;
using EFT;
using EFT.Game.Spawning;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paulov.Bepinex.Framework.Patches;
using Paulov.Tarkov.MP2.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("CreateNetworkGamePatch");

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
      , MainMenuControllerClass ____menuOperation
      )
    {
        try
        {
            var matchmakerController = ____menuOperation.MatchmakerPlayersController;

            // Get Server Information
            matchmakerController.UpdateMatchingStatus("Generating Location...");
            var locationSettings = await GetServerLocationSettings(profileStatus, savageProfile, ____raidSettings);

            // Keep the original method's functionality of Logging
            __instance.Logger.LogDebug("TRACE-NetworkGameCreate 3");
            __instance.Logger.LogDebug(string.Format("TRACE-NetworkGameCreate profileStatus: '{0}'", profileStatus.ToString()));

            matchmakerController.UpdateMatchingStatus("Creating P2P connections");

            // Create the NetworkGame and connections here
            var gameObject = new GameObject("NetworkGame");
            //gameObject.AddComponent<GameServer>();
            //Plugin.Logger.LogDebug("Created and added GameServer");

            var gameClient = gameObject.AddComponent<GameClient>();
            //gameClient.ConnectToIpAndPortAndStart(profileStatus.ip, profileStatus.port);
            //Plugin.Logger.LogDebug("Created and added GameClient");

            //var micn = new Dissonance.Integrations.MirrorIgnorance.MirrorIgnoranceCommsNetwork()
            //{
            //};
            //gameObject.AddComponent<MirrorIgnoranceCommsNetwork>();
            //DissonanceServer dissonanceServer = new DissonanceServer(micn);

            CreateListenServer(profileStatus);

            // Paulov: This is the first attempt to create a network game using BSG's code.
            LocationSettingsClass.Location location = locationSettings;// ____raidSettings.SelectedLocation;
            TimeSpan sessionTime = TimeSpan.FromMinutes(location.EscapeTimeLimit);
            matchmakerController.UpdateMatchingStatus("Creating EftNetworkGame");
            var game = PaulovMPGameServer.Create(
                Plugin.BackEndSession
                , gameWorld
                , profileStatus
                , ____raidSettings
                , savageProfile
                , Plugin.BackEndSession.InsuranceCompany
                , ____inputTree
                , MonoBehaviourSingleton<GameUI>.Instance
                , metricsEvents
                , new GClass2503(metricsConfig, __instance)
                , EUpdateQueue.Update
                , sessionTime
                , delegate
                {
                    Plugin.Logger.LogDebug("TRACE-NetworkGameCreate 6");
                    ScreenManager.Instance.CloseAllScreensForced();
                    UnityEngine.Object.DestroyImmediate(MonoBehaviourSingleton<MenuUI>.Instance.gameObject);
                    ____menuOperation?.Unsubscribe();
                    gameWorld.OnGameStarted();
                }, delegate (Result<ExitStatus, TimeSpan, MetricsClass> result)
                {
                    __instance.method_52(profileStatus.profileid, savageProfile, location, result);
                });
            Singleton<AbstractGame>.Create(game);

            __instance.Logger.LogDebug("TRACE-NetworkGameCreate 4");

            matchmakerController.UpdateMatchingStatus("Loading Profile");
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

            matchmakerController.UpdateMatchingStatus("Loaded Profile. Creating Game Session.");


            var networkGameSession = PaulovNetworkGameSession.Create(game, myProfile.Id, myProfile.Id, null);

            metricsEvents.SetGameCreated();

            Configuration1 clientConfiguration = new Configuration1
            {
                ConnectionLimit = 1,
                WaitTimeout = 3000u,
                DisconnectTimeout = 12000u,
                PingInterval = 500u
            };
            __instance.Logger.LogDebug("TRACE-NetworkGameCreate 5", Array.Empty<object>());
            networkGameSession.method_1(clientConfiguration, profileStatus.ip, profileStatus.port + 1);

            MonoBehaviourSingleton<PreloaderUI>.Instance.SetSessionId(profileStatus.shortId);

            matchmakerController.UpdateMatchingStatus("Attempting to start the game.");

            Plugin.Logger.LogDebug($"--> Run");
            await networkGameSession.OnAcceptGamePacket(new OnAcceptResponseSettingsPacket(), game, location);

            var tasks = new List<Task>();
            // This requires game.Run to be called first, which is done above.
            var worldSpawnNetworkMessage = new WorldSpawnPacket(location).ToNetworkMessage();
            Plugin.Logger.LogDebug($"--> WorldSpawn");
            var worldSpawnTask = ((Interface10)game).WorldSpawn(worldSpawnNetworkMessage);
            await Task.Yield();
            // TODO: This is a hack to allow the world to enable the extract system.
            for (var iExit = 0; iExit < location.exits.Length; iExit++)
            {
                var name = location.exits[iExit].Name;
                Logger.LogDebug($"Enabling Exfiltration Point: {name}");
                ExfiltrationPoint[] source = ExfiltrationControllerClass.Instance.ExfiltrationPoints.Concat(ExfiltrationControllerClass.Instance.SecretExfiltrationPoints).ToArray();
                ExfiltrationPoint exfiltrationPoint = source.FirstOrDefault((ExfiltrationPoint x) => x.Settings.Name == name);
                if (exfiltrationPoint != null)
                {
                    exfiltrationPoint.Status = EExfiltrationStatus.RegularMode;
                    Logger.LogDebug($"Enabled Exfiltration Point: {name}");

                }
            }
            await Task.Yield(); // Yield to allow the world spawn to process
            matchmakerController.UpdateMatchingStatus("Spawned World");
            await Task.Delay(1000); // Display the message for a second

            Plugin.Logger.LogDebug($"--> WorldSpawnLoot");
            ((Interface10)game).WorldSpawnLoot(new WorldSpawnLootPacket(location).ToNetworkMessage());
            matchmakerController.UpdateMatchingStatus("Spawned World Loot");
            await Task.Delay(1000); // Display the message for a second

            Plugin.Logger.LogDebug($"--> Game Spawn");
            ((Interface10)game).Spawn();
            matchmakerController.UpdateMatchingStatus("Spawned Game");

            var spawnPoints = SpawnSystemClass2.CreateFromScene(GClass1507.LocalDateTimeFromUnixTime(location.UnixDateTime), location.SpawnPointParams);
            int spawnSafeDistance = ((location.SpawnSafeDistanceMeters > 0) ? location.SpawnSafeDistanceMeters : 100);
            var settings = new SpawnSystemSettings(location.MinDistToFreePoint, location.MaxDistToFreePoint, location.MaxBotPerZone, spawnSafeDistance, location.NoGroupSpawn, location.OneTimeSpawn);
            var spawnSystem = SpawnSystemFactory.CreateSpawnSystem(settings, () => Time.time, gameWorld, game.BotsController, spawnPoints);
            var spawnPoint = spawnSystem.SelectSpawnPoint(ESpawnCategory.Player, myProfile.Info.Side, null, null, null, null, myProfile.Id);
            // After the world spawn, we can send the player spawn packet.
            var playerSpawnPacket = new PlayerSpawnPacket(myProfile, spawnPoint.Position, spawnPoint.Rotation);
            var playerSpawnPacketArray = playerSpawnPacket.ToArraySegment();
            Plugin.Logger.LogDebug($"--> PlayerSpawn");

            ((Interface10)game).PlayerSpawn(new BSGNetworkReader(playerSpawnPacketArray), async (x) =>
            {
                matchmakerController.UpdateMatchingStatus("Spawned Player");

                Plugin.Logger.LogDebug($"--> GameSpawned");
                await ((Interface10)game).GameSpawned();

                Plugin.Logger.LogDebug($"--> GameStarting");
                game.GameStarting(6);

                await Task.Delay(6 * 1000);
                Plugin.Logger.LogDebug($"--> GameStarted");
                //game.GameStarted(45 * 60, 45 * 60);
                game.GameStarted(0, 45 * 60);
            });

        }
        catch (Exception ex)
        {
            // Log the exception
            Logger.LogError($"{nameof(CreateNetworkGamePatch)}.{nameof(PostfixOverrideMethod)}: An error occurred while creating the network game: {ex.Message}");
            Logger.LogError(ex);
            __instance.Logger.LogError($"{nameof(CreateNetworkGamePatch)}.{nameof(PostfixOverrideMethod)}: An error occurred while creating the network game: {ex.Message}");
            __instance.Logger.LogError(ex.ToString());
            // Optionally, you can handle the error gracefully, e.g., by showing a message to the user
            {
                Logger.LogError($"{nameof(CreateNetworkGamePatch)}.{nameof(PostfixOverrideMethod)}: An error occurred while creating the network game. Please check the logs for more details.");
                __instance.Logger.LogError($"{nameof(CreateNetworkGamePatch)}.{nameof(PostfixOverrideMethod)}: An error occurred while creating the network game.");
                //__instance.method_52(profileStatus.profileid, savageProfile, __, new Result<ExitStatus, TimeSpan, MetricsClass>(ExitStatus.Survived, TimeSpan.Zero, null));

                return;
            }

        }


    }

    private static void CreateListenServer(ProfileStatusClass profileStatus)
    {
        // TODO: This should only be created if the user is the Server.

        PaulovNetworkListenServer networkListenServer = new PaulovNetworkListenServer(profileStatus.port + 1);

    }

    private static async Task<LocationSettingsClass.Location> GetServerLocationSettings(ProfileStatusClass profileStatus, Profile savageProfile, RaidSettings ____raidSettings)
    {
        // TODO: We have not checked whether this is the Client or Server, so we assume it is the Server here.

        HttpClient httpClient = new HttpClient();
        string serverUrl = BackendUrlAndTokenRetriever.GetBackendUrlAsHttps();
        Plugin.Logger.LogDebug($"Server URL: {serverUrl}");
        httpClient.BaseAddress = new Uri(serverUrl);
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Paulov Tarkov MP2 Client");
        httpClient.DefaultRequestHeaders.Add("PHPSESSID", BackendUrlAndTokenRetriever.GetUserId());
        var dataToSend = new
        {
            profileId = profileStatus.profileid,
            savageProfileId = savageProfile.Id,
            location = ____raidSettings.SelectedLocation.Id,
            sessionId = profileStatus.profileid
        };
        httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var response = await httpClient.PostAsync("/client/match/local/start/"
            , new StringContent(dataToSend.ToJson()
            , encoding: System.Text.Encoding.UTF8, "application/json"));
        response.EnsureSuccessStatusCode();
        var responseContent = await response.Content.ReadAsByteArrayAsync();
        //Plugin.Logger.LogDebug($"Response Content Length: {responseContent.Length}");
        var responseText = SimpleZlib.Decompress(responseContent);
        //Plugin.Logger.LogDebug($"Response Content: {responseText}");
        var responseObject = JObject.Parse(responseText);
        var data = responseObject["data"]["locationLoot"].ToString();

        LocationSettingsClass.Location locationSettings = JsonConvert.DeserializeObject<LocationSettingsClass.Location>(data, new JsonSerializerSettings() { Converters = BSGJsonHelpers.GetJsonConvertersBSG() });
        Plugin.Logger.LogDebug($"{JsonConvert.SerializeObject(locationSettings, Formatting.Indented, settings: new JsonSerializerSettings() { Converters = BSGJsonHelpers.GetJsonConvertersBSG() })}");
        return locationSettings;
    }
}
