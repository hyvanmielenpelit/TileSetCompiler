using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TileSetCompiler.Data
{
    public class FloorTileData
    {
        public FileInfo FloorFile { get; set; }
        public bool HasTileFile { get; set; }
        public string SubType { get; set; }
        public string NameOrDesc { get; set; }

        public FloorTileData()
        {

        }

        public FloorTileData(FileInfo floorFile, bool hasTileFile, string subType, string nameOrDesc)
        {
            FloorFile = floorFile;
            HasTileFile = hasTileFile;
            SubType = subType;
            NameOrDesc = nameOrDesc;
        }
    }
}
