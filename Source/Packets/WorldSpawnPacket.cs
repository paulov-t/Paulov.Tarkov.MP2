using EFT;
using EFT.Interactive;
using System;
using System.IO;

namespace Paulov.Tarkov.MP2.Packets
{
    /// <summary>
    /// The WorldSpawnPacket class represents a packet that is sent when the world spawns in the game.
    /// This packet must be Compressed using SimpleZlib and sent to the client.
    /// </summary>
    internal sealed class WorldSpawnPacket
    {
        LocationSettingsClass.Location _location;

        public WorldSpawnPacket(LocationSettingsClass.Location location)
        {
            _location = location;
        }

        public ArraySegment<byte> ToArraySegment()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            // ExfiltrationControllerClass.ReadStates
            if (_location.exits == null || _location.exits.Length == 0)
            {
                writer.Write((short)0); // Number of exfiltration points - TODO: Implement exfiltration points logic 
                Plugin.Logger.LogError("WorldSpawnPacket: No exfiltration points found for location " + _location.Id + ". This may cause issues in the game.");
            }
            else
            {
                writer.Write((short)_location.exits.Length); // Number of exfiltration points - TODO: Implement exfiltration points logic 
                for (var iExit = 0; iExit < _location.exits.Length; iExit++)
                {
                    writer.Write(_location.exits[iExit].Id); // name
                    writer.Write((byte)EExfiltrationStatus.RegularMode); // eExfiltrationStatus 
                    writer.Write((int)0); // startTime 
                    writer.Write((short)0); // transfer item list count 
                }
            }

            // BufferZoneControllerClass.ReadStates
            writer.Write(false); // BufferZone availability - TODO: Implement buffer zones logic
            // EFT.ClientWorld.method_18 
            writer.Write(0); // Number of smoke grenades??? - TODO: Implement smoke grenades logic
            // EFT.ClientWorld.method_19
            writer.Write(0); // Number of door states - TODO: Implement door states logic
            // EFT.ClientWorld.method_20
            writer.Write(0); // Number of lamp states - TODO: Implement lamp states logic
            // EFT.ClientWorld.method_21
            writer.Write(0); // Number of window states - TODO: Implement window states logic
            // EFT.ClientWorld.method_22
            writer.Write((ushort)0); // Number of sync object states - TODO: Implement sync object states logic
            // EFT.ClientWorld.smethod_1
            writer.Write(false); // Transfer of objects BTR - TODO: Implement object transfer logic
            writer.Write(false); // Transfer of objects Transit - TODO: Implement object transfer logic

            // SyncModule - TODO: Implement sync module logic
            BinaryWriter syncWriter = new BinaryWriter(new MemoryStream());
            syncWriter.Write(0);
            var syncWriterBytes = (syncWriter.BaseStream as MemoryStream).ToArray();
            writer.WriteSizeAndBytes(syncWriterBytes);

            /// This packet must be Compressed using SimpleZlib
            var compressedData = Zlib.Compress((writer.BaseStream as MemoryStream).ToArray(), ZlibCompression.Normal);
            BinaryWriter writerZlib = new BinaryWriter(new MemoryStream());
            writerZlib.Write(compressedData.Length);
            writerZlib.Write(compressedData);
            var resultingBytes = (writerZlib.BaseStream as MemoryStream).ToArray();
            return new ArraySegment<byte>(resultingBytes);
        }
        public byte[] ToBytes()
        {
            return ToArraySegment().ToArray();
        }

        public NetworkMessage ToNetworkMessage()
        {
            var message = new NetworkMessage
            {
                MessageType = (short)NetworkMessageType.MsgWorldSpawn,
                buffer = ToArraySegment(),
                Channel = EFT.Network.NetworkChannel.Reliable,
                Connection = new(1, 1, "", 17000)
            };
            return message;
        }
    }
}
