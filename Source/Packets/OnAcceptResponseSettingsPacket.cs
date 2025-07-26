using EFT;
using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using System.IO;

namespace Paulov.Tarkov.MP2.Packets
{
    internal class OnAcceptResponseSettingsPacket : IBSGPacketMethods
    {
        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(ToBytes());
        }

        public byte[] ToBytes()
        {
            //BinaryWriter writer = new BinaryWriter(new MemoryStream());
            //writer.Write(false);
            //writer.Write(false);

            //var gameDateTime = new GameDateTime(DateTime.Now, DateTime.Now, 7, false);
            //var bsgSerializer = new BSGSerializer();
            //gameDateTime.Serialize(bsgSerializer, true);
            //writer.Write(bsgSerializer.ToArray());

            BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
            var bsgSerializer = new BSGSerializer();
            new AbstractGameSession.Class1009().Serialize(bsgSerializer);
            binaryWriter.WriteSizeAndBytes(bsgSerializer.ToArray());
            return (binaryWriter.BaseStream as MemoryStream).ToArray();
        }

        public NetworkMessage ToNetworkMessage()
        {
            return new NetworkMessage
            {
                MessageType = (short)NetworkMessageType.MsgTypeAccept,
                buffer = ToArraySegment(),
                Channel = EFT.Network.NetworkChannel.Reliable,
            };
        }
    }
}
