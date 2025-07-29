using EFT;
using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using System.IO;

namespace Paulov.Tarkov.MP2.Packets
{
    internal class WorldSpawnLootPacket : IBSGPacketMethods
    {
        private LocationSettingsClass.Location _location;

        public WorldSpawnLootPacket(LocationSettingsClass.Location location)
        {
            _location = location;
        }

        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(ToBytes());
        }

        public byte[] ToBytes()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(false); // isLootEnabled
            writer.Write(1); // loot
            writer.Write(new byte[1] { 0x0 });
            return (writer.BaseStream as MemoryStream).ToArray();
        }

        public NetworkMessage ToNetworkMessage()
        {
            return new NetworkMessage
            {
                MessageType = (short)NetworkMessageType.MsgWorldSpawnLoot,
                buffer = ToArraySegment(),
                Channel = EFT.Network.NetworkChannel.Reliable,
            };
        }
    }
}
