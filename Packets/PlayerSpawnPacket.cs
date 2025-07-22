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

        private Profile Profile;

        public PlayerSpawnPacket()
        {
        }

        public PlayerSpawnPacket(Profile profile)
        {
            Profile = profile ?? throw new ArgumentNullException(nameof(profile), "Profile cannot be null");
        }

        public ArraySegment<byte> ToArraySegment(Profile profile)
        {
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
            writerSerializer.WriteEFTProfileDescriptor(new CompleteProfileDescriptorClass(profile, GClass2069.Instance));
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

        public ArraySegment<byte> ToArraySegment()
        {
            if (Profile == null)
            {
                throw new InvalidOperationException("Profile must be set before calling ToArraySegment.");
            }
            return ToArraySegment(Profile);
        }

        public byte[] ToBytes(Profile profile)
        {
            return ToArraySegment(profile).ToArray();
        }

        public byte[] ToBytes()
        {
            if (Profile == null)
            {
                throw new InvalidOperationException("Profile must be set before calling ToBytes.");
            }
            return ToBytes(Profile);
        }
    }
}
