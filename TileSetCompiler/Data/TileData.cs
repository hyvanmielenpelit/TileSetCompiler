using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler.Data
{
    class TileData
    {
        public FileInfo File { get; set; }
        public bool IsStatue { get; set; }
        public Point? PointInTiles { get; set; }
        public Size? BitmapSizeInTiles { get; set; }

        public TileData()
        {

        }
    }
}
