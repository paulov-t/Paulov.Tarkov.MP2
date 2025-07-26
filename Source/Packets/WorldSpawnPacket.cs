using EFT;
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
        public ArraySegment<byte> ToArraySegment()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            // ExfiltrationControllerClass.ReadStates
            writer.Write(0); // Number of exfiltration points - TODO: Implement exfiltration points logic 
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
            writer.Write(syncWriterBytes.Length); // Length of the sync data
            writer.Write(syncWriterBytes); // Sync data

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
