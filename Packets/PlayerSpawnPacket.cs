using ComponentAce.Compression.Libs.zlib;
using EFT;
using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using System.IO;
using UnityEngine;

namespace Paulov.Tarkov.MP2.Packets
{
    public sealed class PlayerSpawnPacket : IBSGPacketMethods
    {

        private Profile _profile;
        private Vector3 _position;

        public PlayerSpawnPacket(Profile profile, Vector3 position)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile), "Profile cannot be null");
            _position = position;
        }

        public ArraySegment<byte> ToArraySegment()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(1);
            writer.Write(_position);
            // OnDeserializeInitialState
            writer.Write(true); // isAlive
            writer.Write(_position);
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
            writerSerializer.WriteEFTProfileDescriptor(new CompleteProfileDescriptorClass(_profile, GClass2069.Instance));
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
            return arraySegment;
        }

        public byte[] ToBytes()
        {
            if (_profile == null)
            {
                throw new InvalidOperationException("Profile must be set before calling ToBytes.");
            }
            return ToBytes();
        }
    }
}
