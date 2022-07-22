using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using static TMNTTool.Compression;
using TMNTTool;

namespace TMNTTool
{
    internal class Texture2D
    {
        public enum Platform : int
        {
            PC = 0x00,
            NSW = 0x01
        }

        public string inputFile;
        private byte[] Magic = {0x58, 0x4E, 0x42, 0x77, 0x05, 0x01};
        public int FileSize { get; set; }
        public byte ReadBool { get; set; }
        public string Type = "Microsoft.Xna.Framework.Content.Texture2DReader";
        public int Width { get; set; }
        public int Height { get; set; }
        public int Count { get; set; }
        public int PixeldataSize { get; set; }
        public string PixelFormat { get; set; }

        public Platform platform { get; set; }

        public void Convert2XNB(string png)
        {
            Bitmap bmp = new Bitmap(png);
            Width = bmp.Width;
            Height = bmp.Height;
            WriteToXNB(BitmapToByteArray(bmp), png.Split('.')[0] + "_new.zxnb");
        }

        public static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }

        }
        public void WriteToXNB(byte[] rawData, string outputFile)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(Magic);
            writer.Write(rawData.Length + 0x55);
            writer.Write((byte)1);
            writer.Write(Type);
            writer.Write(new byte[5]);
            writer.Write(1);
            writer.Write((byte)0);
            writer.Write(Width);
            writer.Write(Height);
            writer.Write(1);
            writer.Write(rawData.Length);
            writer.Write(rawData);
            File.WriteAllBytes(outputFile, CompressZlib(ms.ToArray()));
        }
        public void Convert2Image()
        {
            byte[] decompressed = DecompressZlib(File.ReadAllBytes(inputFile));
            MemoryStream ms = new MemoryStream(decompressed);
            BinaryReader reader = new BinaryReader(ms);
            reader.BaseStream.Position += 6;
            FileSize = reader.ReadInt32();
            ReadBool = reader.ReadByte();
            Type = reader.ReadString();
            reader.BaseStream.Position += 10;
            Width = reader.ReadInt32();
            Height = reader.ReadInt32();
            Count = reader.ReadInt32();
            if(Count > 1)
            {
                Console.WriteLine("Textures more than 1!");
                Console.ReadKey();
                return;
            }
            PixeldataSize = reader.ReadInt32();
            byte[] pixelData = reader.ReadBytes(PixeldataSize);
            reader.Close();
            if (pixelData.Length == Width * Height * 4)
            {
                Console.WriteLine("Pixel Format: RGBA32 (PC)");
                platform = Platform.PC;
            }
            else if (pixelData.Length == Width * Height || pixelData.Length - 0x3000 == Width * Height) 
            { 
                Console.WriteLine("Pixel Format: DXT3/BC2 (Nintendo Switch)");
                platform = Platform.NSW;
            }
            if(pixelData.Length != Width * Height * 4 && pixelData.Length != Width * Height && pixelData.Length - 0x3000 == Width * Height)
            {
                Console.WriteLine("Pixel data format is not supported yet!\nPress any key...");
                Console.ReadKey();
            }
            if(platform == Platform.PC)
            {
                Bitmap bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr pNative = bmData.Scan0;
                Marshal.Copy(pixelData, 0, pNative, pixelData.Length);
                bitmap.UnlockBits(bmData);
                bitmap.Save(inputFile + ".png", ImageFormat.Png);
            }
            else if(platform == Platform.NSW)
            {
                byte[] buffer = new byte[Width * Height * 4];
                DxtDecoder.DecompressDXT3(pixelData, Width, Height, buffer);
                Bitmap bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                BitmapData bmData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                IntPtr pNative = bmData.Scan0;
                Marshal.Copy(buffer, 0, pNative, buffer.Length);
                bitmap.UnlockBits(bmData);
                bitmap.Save(inputFile + ".png", ImageFormat.Png);
            }
        }
    }
}
