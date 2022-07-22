using System.IO;
using System.IO.Compression;

namespace TMNTTool
{
    internal class Compression
    {
        internal static byte[] DecompressZlib(byte[] input)
        {
            using (MemoryStream inms = new MemoryStream(input))
            using (MemoryStream output = new MemoryStream())
            {
                DeflateStream zlib = new DeflateStream(inms, CompressionMode.Decompress);
                zlib.CopyTo(output);
                return output.ToArray();
            }
        }

        internal static byte[] CompressZlib(byte[] data)
        {
            MemoryStream output = new MemoryStream();
            using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }
    }
}
