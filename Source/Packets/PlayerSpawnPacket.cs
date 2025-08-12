using ComponentAce.Compression.Libs.zlib;
using EFT;
using EFT.InventoryLogic;
using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Paulov.Tarkov.MP2.Packets
{
    public sealed class PlayerSpawnPacket : IBSGPacketMethods, ISerializedReadable
    {

        private Profile _profile;
        private Vector3 _position;
        private Quaternion _rotation;

        public PlayerSpawnPacket(Profile profile, Vector3 position, Quaternion rotation)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile), "Profile cannot be null");
            _position = position;
            _rotation = rotation;
        }

        public ArraySegment<byte> ToArraySegment()
        {
            var arraySegment = new ArraySegment<byte>(ToBytes());
            return arraySegment;
        }

        public byte[] ToBytes()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(1);
            writer.Write(_position);
            // OnDeserializeInitialState
            writer.Write(true); // isAlive
            writer.Write(_position);
            writer.Write(_rotation);
            writer.Write(false); // isInPronePose
            writer.Write(1f); // poseLevel
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
            var compressedProfileData = SimpleZlib.CompressToBytes(bytes: writerSerializer.ToArray(), length: writerSerializer.ToArray().Length, compressLevel: (int)ZlibCompression.Normal);
            Plugin.Logger.LogDebug($"compressedProfileData Length: {compressedProfileData.Length}");
            Plugin.Logger.LogDebug($"compressedProfileData Position: {writer.BaseStream.Position}");
            writer.Write(compressedProfileData.Length + 1); // length of the serialized profile descriptor (i dont know why BSG adds a -1 to its sizing??)
            writer.Write(compressedProfileData); // serialized profile descriptor
            //writer.WriteMongoId(MongoID.Generate(true)); // firstId
            writer.Write((int)0); // firstId
            writer.Write((ulong)1); // firstId 2
            writer.Write((ushort)1337u); // firstOperationId

            writer.Write(false); // unk
            writer.Write((int)0); // ScavExfilMask

            byte[] healthData = CreateHealthData();
            Plugin.Logger.LogDebug($"HealthState Length: {healthData.Length}");
            Plugin.Logger.LogDebug($"HealthState Position: {writer.BaseStream.Position}");
            writer.WriteSizeAndBytes(healthData); // HealthState Data
            //writer.Write(healthData.Length); // HealthState Length
            //writer.Write(healthData); // HealthState Data
            //writer.Write((uint)0);

            writer.Write((int)0); // AnimationVariant
            writer.Write((byte)EHandsControllerType.Knife); // eHandsControllerType
            writer.Write(true); // hasItemId. This is needed if using anything but "Empty" Hands Controller
            var itemId = _profile.Inventory.GetItemsInSlots(new[] { EquipmentSlot.Scabbard }).FirstOrDefault()?.Id ?? "Empty";
            Plugin.Logger.LogDebug($"ItemId: {itemId}");
            writer.WriteBSGString(itemId); // ItemId
            writer.Write((byte)0); // secret exfils
            return (writer.BaseStream as MemoryStream).ToArray();
        }

        public void AssertData()
        {
            AssertData(new BSGNetworkReader(ToBytes()));
        }

        public static void AssertData(BSGNetworkReader reader)
        {
            // PlayerSpawn
            Plugin.Logger.LogDebug($"Read Int at Position: {reader.Position}");
            int id = BSGNetworkReaderExtensions.ReadInt(reader);
            Plugin.Logger.LogDebug($"Read Vector3 at Position: {reader.Position}");
            Vector3 position = BSGNetworkReaderExtensions.ReadVector3(reader);
            Plugin.Logger.LogDebug($"Read Vector3 position:{position}");

            // OnDeserializeInitialState
            Plugin.Logger.LogDebug($"Read bool at Position: {reader.Position}");
            bool isAlive = BSGNetworkReaderExtensions.ReadBool(reader);
            Plugin.Logger.LogDebug($"Read isAlive:{isAlive}");

            Plugin.Logger.LogDebug($"Read Vector3 at Position: {reader.Position}");
            Vector3 position2 = BSGNetworkReaderExtensions.ReadVector3(reader);
            Plugin.Logger.LogDebug($"Read Vector3 position2:{position2}");
            Plugin.Logger.LogDebug($"Read Quaternion at Position: {reader.Position}");
            Quaternion rotation = BSGNetworkReaderExtensions.ReadQuaternion(reader);
            Plugin.Logger.LogDebug($"Read Quaternion rotation:{rotation}");
            Plugin.Logger.LogDebug($"Read bool at Position: {reader.Position}");
            bool isInPronePose = BSGNetworkReaderExtensions.ReadBool(reader);
            Plugin.Logger.LogDebug($"Read isInPronePose:{isInPronePose}");
            Plugin.Logger.LogDebug($"Read float at Position: {reader.Position}");
            float poseLevel = BSGNetworkReaderExtensions.ReadFloat(reader);
            Plugin.Logger.LogDebug($"Read poseLevel:{poseLevel}");
            Plugin.Logger.LogDebug($"Read byte at Position: {reader.Position}");
            byte voipState = reader.ReadByte();
            Plugin.Logger.LogDebug($"Read voipState:{voipState}");
            Plugin.Logger.LogDebug($"Read bool isInBufferZone at Position: {reader.Position}");
            bool isInBufferZone = BSGNetworkReaderExtensions.ReadBool(reader);
            Plugin.Logger.LogDebug($"Read isInBufferZone:{isInBufferZone}");
            Plugin.Logger.LogDebug($"Read int bufferZoneUsageTimeLeft at Position: {reader.Position}");
            int bufferZoneUsageTimeLeft = BSGNetworkReaderExtensions.ReadInt(reader);
            Plugin.Logger.LogDebug($"Read bufferZoneUsageTimeLeft:{bufferZoneUsageTimeLeft}");
            Plugin.Logger.LogDebug($"Read int _seed at Position: {reader.Position}");
            var _seed = BSGNetworkReaderExtensions.ReadInt(reader);
            Plugin.Logger.LogDebug($"Read _seed:{_seed}");
            Plugin.Logger.LogDebug($"Read int _nextSeed at Position: {reader.Position}");
            var _nextSeed = BSGNetworkReaderExtensions.ReadInt(reader);
            Plugin.Logger.LogDebug($"Read _nextSeed:{_nextSeed}");
            Plugin.Logger.LogDebug($"Read bool leftstance at Position: {reader.Position}");
            bool leftStance = BSGNetworkReaderExtensions.ReadBool(reader);
            Plugin.Logger.LogDebug($"Read leftStance:{leftStance}");

            // method_177
            byte[] healthState = null;
            string itemId = null;
            EHandsControllerType eHandsControllerType = EHandsControllerType.None;
            bool isInSpawnOperation = true;
            bool flag = false;
            Vector2 stationaryRotation = Vector2.zero;
            Quaternion identity = Quaternion.identity;
            int animationVariant = 0;
            Plugin.Logger.LogDebug($"Read byte array profileZip at Position: {reader.Position}");
            byte[] profileZip = BSGNetworkReaderExtensions.ReadBytesAndSize(reader);
            Plugin.Logger.LogDebug($"Read number of bytes for profileZip:{profileZip.Length}");
            Plugin.Logger.LogDebug($"Read ReadMongoId firstId at Position: {reader.Position}");
            MongoID firstId = GClass1895.ReadMongoId(reader);
            Plugin.Logger.LogDebug($"Read mongoid firstId:{firstId}");
            Plugin.Logger.LogDebug($"Read ushort firstOperationId at Position: {reader.Position}");
            ushort firstOperationId = BSGNetworkReaderExtensions.ReadUShort(reader);
            Plugin.Logger.LogDebug($"Read ushort firstOperationId:{firstOperationId}");
            if (true)
            {
                Plugin.Logger.LogDebug($"Read bool at Position: {reader.Position}");
                BSGNetworkReaderExtensions.ReadBool(reader);
                Plugin.Logger.LogDebug($"Read int ScavExfilMask at Position: {reader.Position}");
                var ScavExfilMask = BSGNetworkReaderExtensions.ReadInt(reader);
                Plugin.Logger.LogDebug($"Read byte[] healthState at Position: {reader.Position}");
                healthState = BSGNetworkReaderExtensions.ReadBytesAndSize(reader);
                Plugin.Logger.LogDebug($"Read int animationVariant at Position: {reader.Position}");
                animationVariant = BSGNetworkReaderExtensions.ReadInt(reader);
                Plugin.Logger.LogDebug($"animationVariant:{animationVariant}");
                Plugin.Logger.LogDebug($"Read byte eHandsControllerType at Position: {reader.Position}");
                eHandsControllerType = (EHandsControllerType)reader.ReadByte();
                Plugin.Logger.LogDebug($"eHandsControllerType:{eHandsControllerType}");
                if (BSGNetworkReaderExtensions.ReadBool(reader))
                {
                    itemId = BSGNetworkReaderExtensions.ReadString(reader);
                }
                if (eHandsControllerType == EHandsControllerType.Firearm)
                {
                    isInSpawnOperation = BSGNetworkReaderExtensions.ReadBool(reader);
                    flag = BSGNetworkReaderExtensions.ReadBool(reader);
                    if (flag)
                    {
                        stationaryRotation = BSGNetworkReaderExtensions.ReadVector2(reader);
                        identity.y = BSGNetworkReaderExtensions.ReadFloat(reader);
                        identity.w = BSGNetworkReaderExtensions.ReadFloat(reader);
                    }
                }
                if (eHandsControllerType == EHandsControllerType.None)
                {
                    Plugin.Logger.LogError("No hands controllers");
                }
                byte b = reader.ReadByte();
                ExfiltrationControllerClass instance = ExfiltrationControllerClass.Instance;
                for (int i = 0; i < b; i++)
                {
                    string pointName = BSGNetworkReaderExtensions.ReadString(reader);
                    //SecretExfiltrationPoint secretExfiltrationPoint = instance.SecretExfiltrationPoints.FirstOrDefault((SecretExfiltrationPoint x) => x.Settings.Name.Equals(pointName));
                    //AddDiscoveredSecretExit(secretExfiltrationPoint);
                }
            }

        }

        private byte[] CreateHealthData()
        {
            BinaryWriter healthWriter = new BinaryWriter(new MemoryStream());
            healthWriter.Write(_profile.Health.Energy.Current); // Energy current
            healthWriter.Write(_profile.Health.Energy.Maximum); // Energy max
            healthWriter.Write(_profile.Health.Energy.Minimum); // Energy min

            healthWriter.Write(_profile.Health.Hydration.Current); // Hydration current
            healthWriter.Write(_profile.Health.Hydration.Maximum); // Hydration max
            healthWriter.Write(_profile.Health.Hydration.Minimum); // Hydration min

            healthWriter.Write(_profile.Health.Temperature.Current); // Temperature current
            healthWriter.Write(99f); // Energy max
            healthWriter.Write(0f); // Energy min

            healthWriter.Write(_profile.Health.Poison.Current); // Poison current
            healthWriter.Write(99f); // Energy max
            healthWriter.Write(0f); // Energy min

            healthWriter.Write(0f); // 
            healthWriter.Write(0f); // 

            healthWriter.Write(0f); // Damage Multiplier
            healthWriter.Write(-1f); // Energy Rate
            healthWriter.Write(-1f); // Hydration Rate
            healthWriter.Write(0f); // Temperature Rate
            healthWriter.Write(0f); // Damage Rate
            healthWriter.Write(0f); // Stamina Rate

            //foreach (var bodyPart in _profile.Health.BodyParts)
            foreach (var eBodyPart in GClass2925.RealBodyParts)
            {
                var bodyPart = _profile.Health.BodyParts[eBodyPart];
                healthWriter.Write(bodyPart.Health.Current == bodyPart.Health.Minimum); // Body part IsDestroyed
                healthWriter.Write(bodyPart.Health.Current); // Body part current health
                healthWriter.Write(bodyPart.Health.Maximum); // Body part max health
            }

            healthWriter.Write((short)0); // Injury count

            healthWriter.Write((byte)0); // Unknown byte

            var healthData = (healthWriter.BaseStream as MemoryStream).ToArray();
            return healthData;
        }

        public void ReadFromBytes(BinaryReader reader, byte[] data)
        {
            throw new NotImplementedException();
        }
    }
}
