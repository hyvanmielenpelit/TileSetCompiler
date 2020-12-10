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
        public Size BitmapSize { get; set; }
        public bool IsStatue { get; set; }
        public bool IsFromTemplate { get; set; }
        public Point? PointInTiles { get; set; }
        public Size? BitmapSizeInTiles { get; set; }
        public bool FlipHorizontal { get; set; }
        public bool FlipVertical { get; set; }
        public TemplateData TemplateData { get; set; }
        public FloorTileData FloorTileData { get; set; }

        public TileData()
        {

        }
    }
}
