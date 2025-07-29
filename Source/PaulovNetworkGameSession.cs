using Comfort.Common;
using CustomPlayerLoopSystem;
using EFT;
using EFT.Network.Transport;
using Paulov.Tarkov.MP2.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace Paulov.Tarkov.MP2
{
    internal class PaulovNetworkGameSession : EFT.NetworkGameSession, Class1551.Interface9
    {

        void Class1551.Interface9.OnConnect(GClass2938 connection)
        {
            Plugin.Logger.LogDebug("ConnectionClient.ISession.OnConnect");
            base.Connection = connection;
            //GClass2170.GClass2938_0 = connection;
            base.method_2();
        }

        void Class1551.Interface9.OnDisconnect(GClass2938 connection)
        {
            Plugin.Logger.LogDebug("ConnectionClient.ISession.OnDisconnect");
            base.Connection = null;
            GStruct411 disconnectionReason = connection.GetDisconnectionReason();
            if (disconnectionReason.Reason == SystemReasonDisconnection.Timeout)
            {
                //TasksExtensions.HandleExceptions(interface10_0.BackEndSession.SendDisconnectEvent(base.ProfileId, GClass1642.GetHostIP(), disconnectionReason.Address));
            }

            BSGNetworkManager.Default.RemoveMessageListener();
            BSGNetworkManager.Default.Shutdown();
        }



        public static PaulovNetworkGameSession Create(Interface10 game, string profileId, string token, AbstractLogger logger, Action onConnected = null)
        {
            UNetUpdate.OnUpdate += BSGNetworkManager.Default.Update;
            string text = "Play session(" + profileId + ")";
            PaulovNetworkGameSession networkGameSession = AbstractGameSession.Create<PaulovNetworkGameSession>(game.Transform, text, profileId, token);
            //BSGNetworkManager.Default.AddMessageListener(81, networkGameSession.method_3);
            //BSGNetworkManager.Default.AddMessageListener(82, networkGameSession.method_4);
            //BSGNetworkManager.Default.AddMessageListener(83, networkGameSession.method_9);
            //BSGNetworkManager.Default.AddMessageListener(84, networkGameSession.method_5);
            //BSGNetworkManager.Default.AddMessageListener(91, networkGameSession.method_8);
            networkGameSession.method_18();
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<GInvokedEvent>(networkGameSession.method_16));
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<InvokedEvent>(networkGameSession.method_17));
            //networkGameSession.gclass859_0.AddDisposable(GlobalEventHandlerClass.Instance.SubscribeOnEvent<GClass3418>(networkGameSession.method_15));
            //networkGameSession.class1551_0 = Class1551.smethod_0(networkGameSession);
            typeof(EFT.NetworkGameSession).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
               .FirstOrDefault(f => f.FieldType == typeof(Class1551))
               .SetValue(networkGameSession, Class1551.smethod_0(networkGameSession));
            //networkGameSession.interface10_0 = game;
            typeof(EFT.NetworkGameSession).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
             .FirstOrDefault(f => f.FieldType == typeof(Interface10))
             .SetValue(networkGameSession, game);
            //networkGameSession.action_1 = onConnected;
            //networkGameSession.class400_0 = new Class400(LoggerMode.Add);
            //networkGameSession.ObserveOnly = false;
            //networkGameSession.gclass703_0 = logger;

            return networkGameSession;
        }

        public async Task OnAcceptGamePacket(OnAcceptResponseSettingsPacket packet, Interface10 game)
        {
            Plugin.Logger.LogDebug($"{nameof(PaulovNetworkGameSession)}.{nameof(OnAcceptGamePacket)} called");

            var configSettingsClass = Singleton<BackendConfigSettingsClass>.Instance;


            string text = "";// SimpleZlib.Decompress(response.byte_0);
            ResourceKey[] lootArray = Array.Empty<ResourceKey>();// GClass843.ParseJsonTo<ResourceKey[]>(text, Array.Empty<JsonConverter>());
            string text2 = "";// SimpleZlib.Decompress(response.byte_1);
            string[] customizationArray = Array.Empty<string>();// GClass843.ParseJsonTo<string[]>(text2, Array.Empty<JsonConverter>());
            Plugin.Logger.LogInfo(string.Format("{0}::Resources ({1}):\n{2}", "OnAcceptResponse", lootArray.Length, text));
            Plugin.Logger.LogInfo(string.Format("{0}::Customization ids ({1}):\n{2}", "OnAcceptResponse", customizationArray.Length, text2));
            WeatherClass[] weathers = new WeatherClass[1] { new WeatherClass() };// GClass843.ParseJsonTo<WeatherClass[]>(SimpleZlib.Decompress(response.byte_2), Array.Empty<JsonConverter>());
            float fixedDeltaTime = 100.0f;
            Dictionary<string, int> interactables = new Dictionary<string, int>();// GClass843.ParseJsonTo<Dictionary<string, int>>(SimpleZlib.Decompress(response.byte_3), Array.Empty<JsonConverter>());
            GClass1636.SetupPositionQuantizer(new Bounds(Vector3.zero, Vector3.positiveInfinity));
            //base.NetworkCryptography = new GClass2934(response.bool_0, response.bool_1);

            var psl = new GClass2005.PlayerStateLimits() { MinSpeed = 0, MaxSpeed = 100.0f };

            await game.Run(
                session: this
                , canRestart: false
                , new GameDateTime(DateTime.Now, DateTime.Now, 7, false)
                , interactables
                , prefabs: lootArray
                , customizations: customizationArray
                , weathers: weathers
                , season: ESeason.Summer // response.eseason_0
                , new SeasonsSettings() { SpringSnowFactor = Vector3.zero } // response.gclass2477_0
                , fixedDeltaTime
                , speedLimitsEnabled: false // response.bool_3
                , new GClass2005.Config() { PlayerStateLimits = new Dictionary<EPlayerState, GClass2005.PlayerStateLimits>() { { EPlayerState.ProneMove, psl } }, DefaultPlayerStateLimits = psl } // response.config_0
                , new() { VoipEnabled = true, MicrophoneChecked = true, PushToTalkSettings = new() { SpeakingSecondsLimit = 10, SpeakingSecondsInterval = 1, AbuseTraceSeconds = 60, ActivationsLimit = 0, BlockingTime = 0, HearingDistance = 100, AlertDistanceMeters = 100, ActivationsInterval = 1 }, VoipQualitySettings = new() { AudioQuality = Dissonance.AudioQuality.High, ForwardErrorCorrection = false, FrameSize = Dissonance.FrameSize.Large, NoiseSuppression = Dissonance.Audio.Capture.NoiseSuppressionLevels.Disabled, SensitivityLevels = Dissonance.Audio.Capture.VadSensitivityLevels.HighSensitivity } } // response.gclass2104_0
                );
        }

        public new void method_7(string serverIp)
        {
            Plugin.Logger.LogDebug($"{nameof(PaulovNetworkGameSession)}.{nameof(method_7)} called");
        }

        public new void Update()
        {

        }
    }
}
