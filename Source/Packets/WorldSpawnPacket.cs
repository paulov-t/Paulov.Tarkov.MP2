using EFT;
using System;
using System.IO;
using System.Text;

namespace Paulov.Tarkov.MP2.Packets
{
    /// <summary>
    /// The WorldSpawnPacket class represents a packet that is sent when the world spawns in the game.
    /// This packet must be Compressed using SimpleZlib and sent to the client.
    /// This packet is deserialized in ClientWorld.Deserialize method.
    /// <remarks>
    /// This packet is deserialized in ClientWorld.Deserialize method.
    /// </remarks>
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
            return new ArraySegment<byte>(ToBytes());
        }

        UTF8Encoding utf8Encoding_0 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        public byte[] ToBytes()
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
                writer.Write((short)0); // Number of exfiltration points - TODO: Implement exfiltration points logic 
                //writer.Write((short)_location.exits.Length); // Number of exfiltration points - TODO: Implement exfiltration points logic 
                //for (var iExit = 0; iExit < _location.exits.Length; iExit++)
                //{
                //    writer.Write(_location.exits[iExit].Id); // name
                //    writer.Write((byte)EExfiltrationStatus.RegularMode); // eExfiltrationStatus 
                //    writer.Write((int)0); // startTime 
                //    writer.Write((short)0); // transfer item list count 
                //}
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
            writerZlib.WriteSizeAndBytes(compressedData);
            //writerZlib.Write(compressedData.Length + 1);
            //writerZlib.Write(compressedData);
            var resultingBytes = (writerZlib.BaseStream as MemoryStream).ToArray();
            return resultingBytes;
        }

        public NetworkMessage ToNetworkMessage()
        {
            var compressedData = ToBytes();
            if (compressedData == null || compressedData.Length == 0)
            {
                throw new InvalidOperationException("Compressed data cannot be null or empty");
            }
            AssertData(compressedData);

            var message = new NetworkMessage
            {
                MessageType = (short)NetworkMessageType.MsgWorldSpawn,
                buffer = ToArraySegment(),
                Channel = EFT.Network.NetworkChannel.Reliable,
                Connection = new(1, 1, "127.0.0.1", 17000)
            };
            return message;
        }

        public void AssertData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null or empty");
            }
            if (data.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Data length exceeds maximum allowed size");
            }

            //var reader = new BSGNetworkReader(data);
            //var reader1 = new BSGNetworkReader(SimpleZlib.DecompressToBytes(BSGNetworkReaderExtensions.ReadBytesAndSize(reader)));

            //ExfiltrationControllerClass.Instance.ReadStates(reader1);
            //BufferZoneControllerClass.Instance.ReadStates(reader);
        }
    }
}
