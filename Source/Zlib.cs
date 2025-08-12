using ComponentAce.Compression.Libs.zlib;
using System.Text;

namespace Paulov.Tarkov.MP2;
public enum ZlibCompression
{
    Store = 0,
    Fastest = 1,
    Fast = 3,
    Normal = 5,
    Ultra = 7,
    Maximum = 9
}

public static class Zlib
{
    public static string Compress(string data)
    {
        return Encoding.UTF8.GetString(Compress(Encoding.UTF8.GetBytes(data), level: ZlibCompression.Normal));
    }

    public static byte[] Compress(byte[] data, ZlibCompression level = ZlibCompression.Maximum)
    {
        return SimpleZlib.CompressToBytes(bytes: data, length: data.Length, compressLevel: (int)level);
    }

    public static string Decompress(string data)
    {
        return SimpleZlib.Decompress(Encoding.UTF8.GetBytes(data));
    }
}
