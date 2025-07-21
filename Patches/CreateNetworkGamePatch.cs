using Comfort.Common;
using ComponentAce.Compression.Libs.zlib;
using EFT;
using EFT.UI;
using HarmonyLib;
using Paulov.Bepinex.Framework.Patches;
using System;
using System.Collections.Generic;
using System.IO;
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

        // Create the NetworkGame and connections here
        var gameObject = new GameObject("NetworkGame");
        gameObject.AddComponent<GameServer>();
        var gameClient = gameObject.AddComponent<GameClient>();
        gameClient.ConnectToIpAndPortAndStart(profileStatus.ip, profileStatus.port);

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

        //
        BinaryWriter writer = new BinaryWriter(new MemoryStream());
        writer.Write(0);
        writer.Write(new Vector3());
        // OnDeserializeInitialState
        writer.Write(false); // isAlive
        writer.Write(new Vector3());
        writer.Write(new Quaternion());
        writer.Write(false);
        writer.Write(1f);
        writer.Write((byte)Player.EVoipState.Available); // voipState
        writer.Write(false); // isInBufferZone
        writer.Write(0); // bufferZoneUsageTimeLeft
                         // MalfRandoms.Deserialize
        writer.Write(5336); // _seed
        writer.Write(53362); // _nextSeed
        writer.Write(false); // leftStance
                             // ClientPlayer.method_177
        BSGSerializer writerSerializer = new BSGSerializer();
        writerSerializer.WriteEFTProfileDescriptor(new CompleteProfileDescriptorClass(__instance.GetClientBackEndSession().Profile, GClass2069.Instance));
        var compressedData = SimpleZlib.CompressToBytes(bytes: writerSerializer.ToArray(), length: writerSerializer.ToArray().Length, compressLevel: (int)ZlibCompression.Normal);
        writer.Write(compressedData.Length); // length of the serialized profile descriptor
        writer.Write(compressedData); // serialized profile descriptor
        writer.WriteMongoId(MongoID.Generate(true)); // firstId
        writer.Write((ushort)1337u); // firstOperationId
        writer.Write(true); // unk
        writer.Write(0); // ScavExfilMask
        writer.Write(0u); // HealthState
        writer.Write(0); // AnimationVariant
        writer.Write((byte)EHandsControllerType.Knife); // eHandsControllerType
        writer.Write(false); // hasItemId
        writer.Write((byte)0); // secret exfils

        var arraySegment = new ArraySegment<byte>((writer.BaseStream as MemoryStream).ToArray());

        File.WriteAllBytes("create_network_game_patch.bin", arraySegment.Array);

        //
        // Calls PlayerSpawn
        // -> Requires Id (int) and Initial Location (SpawnPoint Vector3)
        // then Calls ClientPlayer.OnDeserializeInitialState
        // 
        Plugin.Logger.LogDebug($"--> PlayerSpawn");
        ((Interface10)game).PlayerSpawn(new BSGNetworkReader(arraySegment), (x) =>
        {
            game.GameStarting(5);
        });


    }
}

