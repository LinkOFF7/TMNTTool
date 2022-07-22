using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static TMNTTool.Compression;

namespace TMNTTool
{
    internal class Locale
    {
        internal static List<List<string>> list;
        public static void Extract(string inputFile)
        {
            List<List<string>> list = Read(inputFile);
            var ids = new List<string>();
            var text = new List<string>();
            for (int i = 0; i < list.Count; i++)
            {
                ids.Add(list[i][0]);
                text.Add(list[i][1].Replace("\n", "{CL}").Replace("\r", "{RF}"));
            }
            File.WriteAllLines(inputFile + ".ids", ids);
            File.WriteAllLines(inputFile + ".txt", text);
        }

        public static void ImportText(string inputText)
        {
            string idsFile = Path.GetFileNameWithoutExtension(inputText) + ".ids";
            if (!File.Exists(idsFile))
            {
                Console.WriteLine("{0} doesn't not exist! Aborting...", idsFile);
                return;
            }
            list = new List<List<string>>();
            List<string> ids = File.ReadAllLines(idsFile).ToList();
            List<string> lines = File.ReadAllLines(inputText).Select(l => l.Replace("{RF}", "\r").Replace("{CL}", "\n").Replace("\\n", "\n")).ToList();
            if (lines.Count != ids.Count)
            {
                Console.WriteLine("Lines count not equal ID count! Aborting...");
                return;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                List<string> list2 = new List<string>();
                list2.Add(ids[i]);
                list2.Add(lines[i]);
                list.Add(list2);
            }
            Write(inputText.Split('.')[0] + "_new.zpbn");
        }

        private static void Write(string outFile)
        {
            if (list == null) return;
            using (MemoryStream outBuffer = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(outBuffer))
            {
                string type = "ParisSerializer.ListListStringSerializer";
                writer.Write(type);
                writer.Write(list.Count);
                for (int i = 0; i < list.Count; i++)
                {
                    writer.Write(list[i].Count);
                    for (int j = 0; j < list[i].Count; j++)
                    {
                        writer.Write(list[i][j]);
                    }
                }
                File.WriteAllBytes(outFile, CompressZlib(outBuffer.ToArray()));
            }
        }

        private static List<List<string>> Read(string inputFile)
        {
            byte[] decompressed = DecompressZlib(File.ReadAllBytes(inputFile));
            MemoryStream decompressedBuffer = new MemoryStream(decompressed);
            using (BinaryReader reader = new BinaryReader(decompressedBuffer))
            {
                string type = reader.ReadString();
                int num = reader.ReadInt32();
                List<List<string>> list = new List<List<string>>();
                for (int i = 0; i < num; i++)
                {
                    List<string> list2 = new List<string>();
                    int num2 = reader.ReadInt32();
                    for (int j = 0; j < num2; j++)
                    {
                        list2.Add(reader.ReadString());
                    }
                    list.Add(list2);
                }
                return list;
            }
        }
    }
}
