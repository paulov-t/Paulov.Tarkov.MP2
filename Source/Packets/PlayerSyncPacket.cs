using Paulov.Tarkov.MP2.Packets.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace Paulov.Tarkov.MP2.Packets
{
    /// <summary>
    /// Custom packet implementation for SelfPlayerInfo synchronization in EFT multiplayer.
    /// </summary>
    internal sealed class PlayerSyncPacket : IBSGPacketMethods
    {
        Client2ServerPacket _client2ServerPacket = default(Client2ServerPacket);

        public PlayerSyncPacket(Client2ServerPacket client2ServerPacket)
        {
            _client2ServerPacket = client2ServerPacket;
        }

        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(ToBytes());
        }

        public byte[] ToBytes()
        {
            BinaryWriter writer = new BinaryWriter(new MemoryStream());
            writer.Write(true); // rtt stuff

            writer.Write((ushort)_client2ServerPacket.RTT); // rtt
            writer.Write((int)1); // ServerFixedUpdate
            writer.Write((int)1); // ServerTime

            writer.Write(1000f); // selfPlayerInfo.ServerWorldTime
            writer.Write(false); // num / hasHitInfo
            writer.Write(false); // flag
            writer.Write(false); // flag2
            writer.Write(false); // flag3
            writer.Write(false); // flag4
            writer.Write(_client2ServerPacket.InventoryCommandPackets.Any()); // flag5


            // flag5
            if (_client2ServerPacket.InventoryCommandPackets.Any())
            {
                Plugin.Logger.LogDebug("PlayerSyncPacket: Writing InventoryCommandPackets");

                writer.Write(_client2ServerPacket.InventoryCommandPackets.Count); // InventoryCommandPackets
                foreach (var packet in _client2ServerPacket.InventoryCommandPackets)
                {
                    byte[] buffer = new byte[10000];
                    var serializer = new GClass1361(buffer);
                    packet.Serialize(serializer);
                    var bytes = serializer.Buffer.Take(serializer.BytesWritten).ToArray();

                    writer.Write(bytes.Length); // Size of the packet
                    writer.Write(bytes); // Packet data
                }
            }

            writer.Write(true); // hasCommonPacket
            //new GClass2073();
            writer.Write(false); // SyncPositionPacket
            writer.Write(false); // SwitchRenderersPacket
            writer.Write(false); // ChangeSkillLevelPacket
            writer.Write(false); // ChangeMasteringLevelPacket

            writer.Write(false); // hasSyncHealthPacket
            writer.Write(false); // hasAcceptHitDebugDataPacket
            writer.Write(false); // hasQuestConditionValueChangedPacket
            writer.Write(false); // hasAchievementConditionValueChangedPacket 
            writer.Write(false); // hasPrestigeConditionValueChangedPacket
            writer.Write(false); // hasShowStatNotificationPacket
            writer.Write(false); // hasPlayerDiedPacket
            writer.Write(false); // hasClientConfirmCallbackPacket
            writer.Write(false); // hasWeaponOverheatPacket
            writer.Write(false); // hasChangeSkillExperiencePacket
            writer.Write(false); // hasChangeMasteringExperiencePacket
            writer.Write(false); // hasTradersInfoPacket
            writer.Write(false); // hasStringNotificationPacket
            writer.Write(false); // hasRadioTransmitterPacket
            writer.Write(false); // hasLighthouseTraderZoneDataPacket 
            writer.Write(false); // hasLighthouseTraderZoneDebugToolPacket
            writer.Write(false); // hasInteractWithBtrPacket
            writer.Write(false); // hasInteractWithTripwirePacket

            writer.Write(true); // critical packets
            writer.Write(1); // num of critical packets


            return ((MemoryStream)writer.BaseStream).ToArray();
        }
    }
}
