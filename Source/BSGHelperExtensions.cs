using System;
using System.IO;
using System.Text;

namespace Paulov.Tarkov.MP2
{
    public static class BSGHelperExtensions
    {
        static UTF8Encoding utf8Encoding_0 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

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

        public static void WriteBSGString(this BinaryWriter writer, string value)
        {
            var s = new BSGSerializer();
            s.WriteString(value);
            var bytes = s.ToArray();
            writer.Write(bytes);
        }
    }
}
