using System.IO;

namespace Paulov.Tarkov.MP2.Packets.Interfaces
{
    public interface ISerializedReadable
    {
        public void ReadFromBytes(BinaryReader reader, byte[] data);
    }
}
