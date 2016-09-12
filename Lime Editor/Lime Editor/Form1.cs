﻿using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;

namespace Lime_Editor
{
    public partial class Main : Form
    {

        string DefTilesDocument = File.ReadAllText("D:/REPOS/LimeEditor/Tiles.yml");
        string Deftilesheet = @"D:\REPOS\LimeEditor\tilesheet.png";
        int tileSize = 18;
        List<Tile> tiles = new List<Tile>();

        public Main()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            debug.Text = "";
            Init_Icons();
            Load_Tiles(DefTilesDocument, Deftilesheet);
        }

        private void Log(string x)
        {
            debug.Text += x+"\n";
        }

        private void Init_Icons()
        {
            Icons.Columns.Add("Icons", 100);
        }

        private void Load_Images(string tilesheet)
        {
            ImageList imgs = new ImageList();
            int iterator = 0;
            foreach (Tile tile in tiles)
            {
                Icon ic = getIcon(tile.pos, tilesheet);
                imgs.Images.Add(ic);
                Icons.Items[iterator].ImageIndex = iterator;
                iterator++;
            }

            Icons.LargeImageList = imgs;
        }

        public Icon getIcon(Vector2 pos, string tilesheet)
        {
            Bitmap bmp = new Bitmap(Image.FromFile(tilesheet, false));
            bmp = CropImage(bmp, new Rectangle(pos.x*tileSize, pos.y*tileSize, tileSize, tileSize));
            IntPtr hbmp = bmp.GetHicon();
            Icon ic = Icon.FromHandle(hbmp);
            return ic;
        }

        private void Load_Tiles(string TilesDocument, string tileSheet)
        {
            var r = new StringReader(TilesDocument);
            var deserializer = new Deserializer();
            Tiles tiletypes = deserializer.Deserialize<Tiles>(r);

            //Create Tiles from Tiletype definitions
            foreach (TileType tile in tiletypes.tiles)
            {

                string type = tile.name;
                uint iterator = 1;

                foreach (Position pos in tile.positions)
                {
                    iPosition ipos = pos.GetProper();
                    Vector2 start = ipos.start;
                    Vector2 end = ipos.end;
                    int ix = start.x;
                    int iy = start.y;
                    while (true)
                    {
                        Tile lt = new Tile();
                        lt.type = type;
                        lt.name = type + "." + iterator.ToString();
                        lt.pos = new Vector2(ix, iy);
                        tiles.Add(lt);

                        iterator++;

                        if (!(ix != end.x || iy != end.y))
                        { break; }

                        if (ix < end.x) { ix++; }
                        if (ix > end.x) { ix--; }
                        if (iy < end.y) { iy++; }
                        if (iy > end.y) { iy--; }
                    }
                }
            }

            //Add Tiles to list
            foreach (Tile tile in tiles)
            {
                Icons.Items.Add(new ListViewItem(tile.name));
            }

            //Load Images, and associate with ListView
            Load_Images(tileSheet);

        }   

        public class Tiles
        {
            public TileType[] tiles { get; set; }
        }

        public class Tile
        {
            public string type;
            public string name;
            public Vector2 pos;
        }

        public class TileType
        {
            public string name { get; set; }
            public Position[] positions { get; set; }
        }

        public class Position
        {
            public string start { get; set; }
            public string end { get; set; }
            public iPosition GetProper()
            {
                Vector2 istart = new Vector2(0,0);
                Vector2 iend = new Vector2(0,0);
                Match m;
                Regex vectorReg = new Regex(@"([0-9]+),\s*([0-9]+)");
                
                m = vectorReg.Match(this.start);
                istart.x = Int32.Parse(m.Groups[1].ToString());
                istart.y = Int32.Parse(m.Groups[2].ToString());

                m = vectorReg.Match(this.end);
                iend.x = Int32.Parse(m.Groups[1].ToString());
                iend.y = Int32.Parse(m.Groups[2].ToString());

                var ret = new iPosition();
                ret.start = istart;
                ret.end = iend;
                return ret;
            }
        }

        public class iPosition 
        {
            public Vector2 start { get; set; }
            public Vector2 end { get; set; }
        }

        public class Vector2
        {
            public Vector2 (int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x { get; set; }
            public int y { get; set; }
            public override string ToString()
            {
                return this.x.ToString() + ", " + this.y.ToString();
            }
        }

        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);

            return bmp;
        }
    }
}
