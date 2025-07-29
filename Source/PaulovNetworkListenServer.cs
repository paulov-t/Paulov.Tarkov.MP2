using BepInEx.Logging;
using EFT;
using EFT.Network;
using Paulov.Tarkov.MP2.Packets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Paulov.Tarkov.MP2
{
    internal sealed class PaulovNetworkListenServer : BSGNetworkListenServer
    {
        public Dictionary<int, int> ConnectionToPlayerId { get; } = new Dictionary<int, int>();

        public ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("PaulovNetworkListenServer");

        private Dictionary<int, GClass2938> dictionary_0 =>
            typeof(BSGNetworkListenServer).GetField("dictionary_0", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this) as Dictionary<int, GClass2938>;


        public PaulovNetworkListenServer(int port)
        {
            Configuration1 serverConfiguration = new Configuration1
            {
                ConnectionLimit = 16,
                WaitTimeout = 3000u,
                DisconnectTimeout = 12000u,
                PingInterval = 500u,
                ConnectingTimeout = 10000u,
                PacketSize = 10240,
            };
            this.AddMessageListener((short)NetworkMessageType.Connect, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.Disconnect, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.MsgTypeAccept, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.MsgTypeCmdGameStarted, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.CommonEvent, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.MsgTypeWorldSynchronization, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
            });
            AddMessageListener((short)NetworkMessageType.MsgTypePlayerSynchronization, PlayerSynchronizationHandler);
            AddMessageListener((short)NetworkMessageType.MsgTypeFastPlayerSynchronization, (NetworkMessage message) =>
            {
                Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");
                GStruct201 gStruct = default(GStruct201);
                BSGNetworkReader1 reader = new BSGNetworkReader1(message.buffer.Array);
                gStruct.Deserialize(reader);

                foreach (var kvp in dictionary_0)
                {
                    kvp.Value.Send(NetworkChannel.Reliable, (short)NetworkMessageType.MsgTypeFastPlayerSynchronization, message.buffer.Array, message.buffer.Offset, message.buffer.Count);
                }
            });

            Configure(serverConfiguration);
            _ = Task.Run(async () =>
            {
                while (!Active)
                {
                    Plugin.Logger.LogDebug("Waiting for NetworkListenServer to start...");
                    Task.Delay(1000).Wait();
                }
                while (Active)
                {
                    try
                    {
                        Update();
                    }
                    catch (Exception ex)
                    {
                        Plugin.Logger.LogError($"Failed to start NetworkListenServer: {ex.Message}");
                        Plugin.Logger.LogError(ex);
                    }
                    await Task.Delay(1); // Wait for a ms
                }
            });
            Listen(port);
        }

        private Client2ServerPacket PrevFrame = default(Client2ServerPacket);

        private void PlayerSynchronizationHandler(NetworkMessage message)
        {
            if (message.buffer == null || message.buffer.Array == null)
            {
                Logger.LogError("Received MsgTypePlayerSynchronization with null buffer");
                return;
            }

            if (message.buffer.Count == 0)
            {
                Logger.LogError("Received MsgTypePlayerSynchronization with empty buffer");
                return;
            }

            try
            {

                //Logger.LogInfo($"Received message of type {(NetworkMessageType)message.MessageType} on server listen");

                BSGNetworkReader1 reader = new BSGNetworkReader1(message.buffer.Array);
                var size = reader.ReadLimitedInt32(0, 127);
                if (size < 0 || size > 127)
                {
                    Logger.LogError($"Invalid size {size} for MsgTypePlayerSynchronization");
                    return;
                }
                Logger.LogDebug($"size: {size}");
                for (var index = 0; index < size; index++)
                {
                    try
                    {
                        Client2ServerPacket client2ServerPacket = default(Client2ServerPacket);
                        //Client2ServerPacket.DeserializeDiffUsing(reader, ref client2ServerPacket, PrevFrame);
                        PrevFrame = client2ServerPacket;

                        //Logger.LogDebug(JsonConvert.SerializeObject(client2ServerPacket, Formatting.Indented));

                        var returnBytes = new PlayerSyncPacket(PrevFrame).ToBytes(); // This is just to ensure the packet is created and serialized correctly

                        foreach (var kvp in dictionary_0)
                        {
                            kvp.Value.Send(NetworkChannel.Reliable, (short)NetworkMessageType.MsgTypePlayerSynchronization, returnBytes, 0, returnBytes.Length);
                        }
                    }
                    catch
                    {

                    }
                }

                //byte[] buffer = new byte[10000];
                //var writer = new GClass1361(buffer);

                //Client2ServerPacket server2Client = default(Client2ServerPacket);
                //server2Client.MovementInfoPacket = default(MovementInfoPacket);
                //server2Client.MovementInfoPacket.LeftStance = true;
                //Client2ServerPacket.SerializeDiffUsing(writer, ref server2Client, PrevFrame);
                //PrevFrame = client2ServerPacket;

                //ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, writer.BytesWritten);
                //var returnBytes = segment.ToArray();
                //var returnBytes = new PlayerSyncPacket().ToBytes(); // This is just to ensure the packet is created and serialized correctly

                //foreach (var kvp in dictionary_0)
                //{
                //    kvp.Value.Send(NetworkChannel.Reliable, (short)NetworkMessageType.MsgTypePlayerSynchronization, returnBytes, 0, returnBytes.Length);
                //}
            }
            catch
            {

            }
        }

        public override void OnConnect(int connectionIndex)
        {
            base.OnConnect(connectionIndex);
        }

        public override void OnData(int connectionIndex, NetworkChannel channel, byte[] buffer, int bufferCount)
        {
            base.OnData(connectionIndex, channel, buffer, bufferCount);
        }

        public override void OnDisconnect(int connectionIndex)
        {
            base.OnDisconnect(connectionIndex);
        }
    }
}
