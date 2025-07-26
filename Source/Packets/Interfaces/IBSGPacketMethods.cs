using System;

namespace Paulov.Tarkov.MP2.Packets.Interfaces
{
    internal interface IBSGPacketMethods
    {
        public ArraySegment<byte> ToArraySegment();
        public byte[] ToBytes();

    }
}
