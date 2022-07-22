using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using static TMNTTool.Compression;
using System.Drawing;

namespace TMNTTool
{
    public class FontSettings
    {
        internal string inputSettingFile;
        internal string fontName;

        [JsonProperty]
        public GameFont gameFont;
        public struct GameFont
        {
            public string Type;
            public int CharacterSpacing;
            public int FontSize;
            public int GlyphCount;
            public List<GlyphInfo> glyphs;
            public int SpaceSize;
            public string TextureFilename;
        }

        public struct GlyphInfo
        {
            public char Key;
            public int Kerning;
            public int MainTextureID;
            public int OffsetY;
            public int OutlineTextureID;
            public Rectangle OutlineZone;
            public int ShaderTextureID;
            public Rectangle ShaderZone;
            public Rectangle Zone;
        }

        internal Dictionary<string, int> ReadSettingIni(string iniFile)
        {
            Dictionary<string, int> set = new Dictionary<string, int>();
            string[] t = File.ReadAllLines(iniFile);
            foreach(var text in t)
            {
                if (text == "" || text.StartsWith("#"))
                    continue;
                else if (text.StartsWith("@"))
                {
                    fontName = text.Substring(text.IndexOf('=') + 1);
                    continue;
                } 
                set.Add(text.Split('=')[0], int.Parse(text.Split('=')[1]));
            }
            return set;
        }

        public void Serialize()
        {
            ReadSettingFile();
            var json = JsonConvert.SerializeObject(gameFont, Formatting.Indented);
            File.WriteAllText(inputSettingFile + ".json", json);
        }

        public void Deserialize(string inputJson)
        {
            gameFont = JsonConvert.DeserializeObject<GameFont>(File.ReadAllText(inputJson));
        }

        public void WriteSettingFile(string outputFile)
        {
            Console.WriteLine("Writing...");
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(gameFont.Type);
            writer.Write(gameFont.CharacterSpacing);
            writer.Write(gameFont.FontSize);
            writer.Write(gameFont.GlyphCount);
            foreach(var glyph in gameFont.glyphs)
            {
                writer.Write((int)glyph.Key);
                writer.Write(glyph.Kerning);
                writer.Write(glyph.MainTextureID);
                writer.Write(glyph.OffsetY);
                writer.Write(glyph.OutlineTextureID);

                writer.Write(glyph.OutlineZone.X);
                writer.Write(glyph.OutlineZone.Y);
                writer.Write(glyph.OutlineZone.Width);
                writer.Write(glyph.OutlineZone.Height);

                writer.Write(glyph.ShaderTextureID);

                writer.Write(glyph.ShaderZone.X);
                writer.Write(glyph.ShaderZone.Y);
                writer.Write(glyph.ShaderZone.Width);
                writer.Write(glyph.ShaderZone.Height);

                writer.Write(glyph.Zone.X);
                writer.Write(glyph.Zone.Y);
                writer.Write(glyph.Zone.Width);
                writer.Write(glyph.Zone.Height);
            }
            writer.Write(gameFont.SpaceSize);
            writer.Write(gameFont.TextureFilename);
            File.WriteAllBytes(outputFile, CompressZlib(ms.ToArray()));
        }
        private void ReadSettingFile()
        {
            MemoryStream settingFile = new MemoryStream(DecompressZlib(File.ReadAllBytes(inputSettingFile)));
            BinaryReader reader = new BinaryReader(settingFile);
            gameFont.Type = reader.ReadString();
            gameFont.CharacterSpacing = reader.ReadInt32();
            gameFont.FontSize = reader.ReadInt32();
            gameFont.GlyphCount = reader.ReadInt32();
            gameFont.glyphs = new List<GlyphInfo>();
            for (int i = 0; i < gameFont.GlyphCount; i++)
            {
                GlyphInfo glyph = new GlyphInfo();
                glyph.Key = (char)reader.ReadInt32();
                glyph.Kerning = reader.ReadInt32();
                glyph.MainTextureID = reader.ReadInt32();
                glyph.OffsetY = reader.ReadInt32();
                glyph.OutlineTextureID = reader.ReadInt32();
                glyph.OutlineZone = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                glyph.ShaderTextureID = reader.ReadInt32();
                glyph.ShaderZone = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                glyph.Zone = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                gameFont.glyphs.Add(glyph);
            }
            gameFont.SpaceSize = reader.ReadInt32();
            gameFont.TextureFilename = reader.ReadString();
        }
    }
}
