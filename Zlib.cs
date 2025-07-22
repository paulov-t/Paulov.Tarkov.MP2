using ComponentAce.Compression.Libs.zlib;

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
    public static byte[] Compress(byte[] data, ZlibCompression level = ZlibCompression.Normal)
    {
        return SimpleZlib.CompressToBytes(bytes: data, length: data.Length, compressLevel: (int)level);
    }
}
