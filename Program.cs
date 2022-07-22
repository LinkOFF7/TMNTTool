using System;
using System.IO;
using System.Text;
using static TMNTTool.Compression;

namespace TMNTTool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if(args.Length < 2)
            {
                PrintUsage();
                return;
            }
            Console.WriteLine("Processing: {0}", Path.GetFileName(args[1]));
            if (args[0] == "loc")
            {
                string ext = Path.GetExtension(args[1]);
                switch (ext)
                {
                    case ".zpbn":
                        {
                            Locale.Extract(args[1]);
                            break;
                        }
                    case ".txt":
                        {
                            Locale.ImportText(args[1]);
                            break;
                        }
                    default: break;
                }
            }
            else if(args[0] == "dec")
            {
                File.WriteAllBytes(args[1] + ".dec", DecompressZlib(File.ReadAllBytes(args[1])));
            }
            else if (args[0] == "comp")
            {
                File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[1]), CompressZlib(File.ReadAllBytes(args[1])));
            }
            else if (args[0] == "font")
            {
                FontSettings font = new FontSettings();
                string ext = Path.GetExtension(args[1]);
                switch (ext)
                {
                    case ".json":
                        {
                            font.Deserialize(args[1]);
                            font.WriteSettingFile(Path.GetFileNameWithoutExtension(args[1]));
                            break;
                        }
                    case ".zpbn":
                        {
                            font.inputSettingFile = args[1];
                            font.Serialize();
                            break;
                        }
                }
                
            }
            else if (args[0] == "tex")
            {
                Texture2D tex = new Texture2D();
                string ext = Path.GetExtension(args[1]);
                switch (ext)
                {
                    case ".zxnb":
                        {
                            tex.inputFile = args[1];
                            tex.Convert2Image();
                            break;
                        }
                    case ".png":
                        {
                            tex.Convert2XNB(args[1]);
                            break;
                        }
                    case ".dds":
                        {
                            Console.WriteLine("Mode: RAW DDS PIXEL INSERTING");
                            BinaryReader reader = new BinaryReader(File.OpenRead(args[1]));
                            reader.BaseStream.Position = 0xC;
                            tex.Height = reader.ReadInt32();
                            tex.Width = reader.ReadInt32();
                            reader.BaseStream.Position = 0x54;
                            tex.PixelFormat = Encoding.ASCII.GetString(reader.ReadBytes(4));
                            if (tex.PixelFormat == "DXT3")
                                reader.BaseStream.Position = 0x80;
                            else if (tex.PixelFormat == "DX10")
                                reader.BaseStream.Position = 0x94;
                            else
                            {
                                Console.WriteLine("Pixel format {0} is not supported yet!\nPlease provide Linear DXT3 or sRGB DX10+", tex.PixelFormat);
                                break;
                            }
                            Console.WriteLine("\nFormat: {0}\nWidth: {1}\nHeight: {2}", tex.PixelFormat, tex.Width, tex.Height);
                            byte[] pixels = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                            reader.Close();
                            tex.WriteToXNB(pixels, args[1].Split('.')[0] + ".zxnb");
                            break;
                        }
                }
            }
        }


        static void PrintUsage()
        {
            Console.WriteLine("Teenage Mutant Ninja Turtles: Shredder's Revenge Translation Tool v1.1");
            Console.WriteLine("by LinkOFF");
            Console.WriteLine("");
            Console.WriteLine("Usage: TMNTTool.exe <argument> <inputFile>");
            Console.WriteLine("Arguments:");
            Console.WriteLine("  loc\t\tWork with locale file (Content/Global/Loc).");
            Console.WriteLine("  tex\t\tConverting .zxnb <=> .png");
            Console.WriteLine("  tex\t\tConverting .dds to .zxnb (experimental)");
            Console.WriteLine("  font\t\tConverting .zpbn <=> .json");
            Console.WriteLine("  dec\t\tDecompress deflate-compressed files (.zpbn/.zxnb).");
            Console.WriteLine("");
        }
    }
}
