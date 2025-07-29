using EFT;
using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using UnityEngine;

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

            //BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
            var writer = new BSGSerializer();
            //var sessionSettings = new AbstractGameSession.Class1009();
            //sessionSettings.Serialize(writer);

            BSGNetworkWriterExtensions.WriteBool(writer, false);
            BSGNetworkWriterExtensions.WriteBool(writer, false);
            new GameDateTime(DateTime.Now, DateTime.Now, 7f).Serialize(writer, gameOnly: true);
            BSGNetworkWriterExtensions.WriteBytesAndSize(writer, null);
            BSGNetworkWriterExtensions.WriteBytesAndSize(writer, null);
            BSGNetworkWriterExtensions.WriteBytesAndSize(writer, null);
            writer.WriteByte((byte)ESeason.Summer);
            new SeasonsSettings().Serialize(writer);
            BSGNetworkWriterExtensions.WriteBool(writer, false);
            BSGNetworkWriterExtensions.WriteInt(writer, (int)EMemberCategory.Default);
            BSGNetworkWriterExtensions.WriteFloat(writer, 1f);
            BSGNetworkWriterExtensions.WriteBytesAndSize(writer, null);
            BSGNetworkWriterExtensions.WriteBytesAndSize(writer, null);
            BSGNetworkWriterExtensions.WriteVector3(writer, Vector3.negativeInfinity);
            BSGNetworkWriterExtensions.WriteVector3(writer, Vector3.positiveInfinity);
            BSGNetworkWriterExtensions.WriteUShort(writer, 0);
            writer.WriteByte((byte)ENetLogsLevel.Maximun);
            new GClass634().Serialize(writer);
            BSGNetworkWriterExtensions.WriteBool(writer, true);
            new GClass2005.Config().Serialize(writer);

            BSGNetworkWriterExtensions.WriteBool(writer, true);
            new GClass2104() { VoipEnabled = true, MicrophoneChecked = true, PushToTalkSettings = new GClass2106() { SpeakingSecondsLimit = 100 } }
            .Serialize(writer);

            //binaryWriter.WriteSizeAndBytes(writer.ToArray());
            return writer.ToArray();
            //return (binaryWriter.BaseStream as MemoryStream).ToArray();
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
