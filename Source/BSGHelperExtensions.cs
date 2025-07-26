using System;
using System.IO;

namespace Paulov.Tarkov.MP2
{
    public static class BSGHelperExtensions
    {
        public static void WriteSizeAndBytes(this BinaryWriter writer, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "Data cannot be null");
            }
            if (data.Length > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(data), "Data length exceeds maximum allowed size");
            }
            writer.Write((uint)data.Length + 1); // Write the size of the data plus one for some reason
            writer.Write(data);
        }
    }
}
