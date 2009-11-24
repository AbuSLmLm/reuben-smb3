﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Daiz.Library;

namespace Daiz.NES.Reuben.ProjectManagement
{
    public class SpecialManager
    {
        public PaletteInfo SpecialPalette { get; private set; }
        public PatternTable SpecialTable { get; private set; }
        private List<GraphicsBank> SpecialBanks;
        private Dictionary<int, SpecialDefinition> SpecialDefinitions;
        private Dictionary<int, Dictionary<int, BlockProperty>> BlockProperties;

        public SpecialManager()
        {
            SpecialDefinitions = new Dictionary<int, SpecialDefinition>();
            SpecialPalette = new PaletteInfo();
            SpecialBanks = new List<GraphicsBank>();
            BlockProperties = new Dictionary<int, Dictionary<int, BlockProperty>>();
            for (int i = 0; i < 15; i++)
            {
                BlockProperties.Add(i, new Dictionary<int, BlockProperty>());
            }
            LoadBlockProperties();
        }

        private void LoadBlockProperties()
        {
            XDocument xDoc = XDocument.Parse(Resource.properties);
            foreach (var x in xDoc.Element("properties").Elements("set"))
            {
                int set = x.Attribute("leveltype").Value.ToIntFromHex();

                foreach (var e in x.Elements("block"))
                {
                    int block = e.Attribute("value").Value.ToIntFromHex();
                    BlockProperties[set][block] = (BlockProperty)(Enum.Parse(typeof(BlockProperty), e.Attribute("property").Value, true));
                }
            }
        }

        public BlockProperty GetProperty(int defIndex, int block)
        {
            return BlockProperties[defIndex][block];
        }

        public bool LoadSpecialGraphics(string fileName)
        {
            if (!File.Exists(fileName)) return false;
            int dataPointer = 0;
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, (int) fs.Length);
            fs.Close();

            SpecialBanks.Clear();
            for (int i = 0; i < 4; i++)
            {
                GraphicsBank nextBank = new GraphicsBank();
                for (int j = 0; j < 64; j++)
                {
                    byte[] nextTileChunk = new byte[16];
                    for (int k = 0; k < 16; k++) nextTileChunk[k] = data[dataPointer++];
                    nextBank[j] = new Tile(nextTileChunk);
                }
                SpecialBanks.Add(nextBank);
            }

            SpecialTable = new PatternTable();
            for (int j = 0; j < 4; j++)
            {
                SpecialTable.SetGraphicsbank(j, SpecialBanks[j]);
            }
            return true;
        }

        public void LoadDefaultSpecialGraphics()
        {
            int dataPointer = 0;
            byte[] data = Resource.special_graphics;

            SpecialBanks.Clear();
            for (int i = 0; i < 4; i++)
            {
                GraphicsBank nextBank = new GraphicsBank();
                for (int j = 0; j < 64; j++)
                {
                    byte[] nextTileChunk = new byte[16];
                    for (int k = 0; k < 16; k++) nextTileChunk[k] = data[dataPointer++];
                    nextBank[j] = new Tile(nextTileChunk);
                }
                SpecialBanks.Add(nextBank);
            }

            SpecialTable = new PatternTable();
            for (int j = 0; j < 4; j++)
            {
                SpecialTable.SetGraphicsbank(j, SpecialBanks[j]);
            }
        }

        public bool SaveGraphics(string filename)
        {
            byte[] Data = new byte[SpecialBanks.Count * 0x400];

            int dataPointer = 0;
            foreach (var b in SpecialBanks)
            {
                byte[] bankData = b.GetInterpolatedData();
                for (int i = 0; i < 1024; i++)
                {
                    Data[dataPointer++] = bankData[i];
                }
            }
            FileStream fs = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write);
            fs.Write(Data, 0, Data.Length);
            fs.Close();
            return true;
        }


        public bool LoadSpecialDefinitions(string filename)
        {
            if (!File.Exists(filename)) return false;
            SpecialDefinitions.Clear();
            for (int j = 0; j < 15; j++)
            {
                SpecialDefinitions.Add(j, new SpecialDefinition());
            }
            XDocument xDoc = XDocument.Load(filename);
            XElement e = xDoc.Element("specials");
            foreach (var s in e.Elements("specialblocks"))
            {
                int lvlType = s.Attribute("leveltype").Value.ToInt();
                SpecialDefinitions[lvlType].LoadFromElement(s);
            }

            SpecialPalette.LoadFromElement(e.Element("palette"));
            SpecialPalette.IsSpecial = true;
            return true;
        }

        public void LoadDefaultSpecials()
        {
            SpecialDefinitions.Clear();
            for (int j = 1; j < 15; j++)
            {
                SpecialDefinitions.Add(j, new SpecialDefinition());
            }
            XDocument xDoc = XDocument.Parse(Resource.special_definitions);
            XElement root = xDoc.Element("specials");
            foreach (var s in root.Elements("specialblocks"))
            {
                int lvlType = s.Attribute("leveltype").Value.ToInt();
                SpecialDefinitions[lvlType].LoadFromElement(s);
            }
            SpecialPalette = new PaletteInfo();
            SpecialPalette.LoadFromElement(root.Element("palette"));
            SpecialPalette.IsSpecial = true;
        }

        public SpecialDefinition GetSpecialDefinition(int leveltype)
        {
            if(SpecialDefinitions.ContainsKey(leveltype))
            {
                return SpecialDefinitions[leveltype];
            }

            return null;
        }

        public void SaveSepcials(string filename1)
        {
            XDocument xDoc = new XDocument();
            XElement root = new XElement("specials");

            foreach (var s in SpecialDefinitions.Values)
            {
                root.Add(s.CreateElement());
            }

            root.Add(SpecialPalette.CreateElement());
            xDoc.Add(root);
            xDoc.Save(filename1);
        }

        public void Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                LoadSpecialDefinitions(fileName);
            }
            else
            {
                LoadDefaultSpecials();
            }

            if (File.Exists(fileName))
            {
                LoadSpecialGraphics(fileName);
            }
            else
            {
                LoadDefaultSpecialGraphics();
            }
        }
    }

    public enum BlockProperty
    {
        Background,
        Solid,
        TopSolid,
        Water,
        WaterFall,
        Slope,
        SlopeFiller,
        SlopeFillerSolidTop,
        SlopeFillerSolidBottom,
        Harmful,
        ConveyorLeft,
        ConveyorRight,
        Ice,
    }
}
