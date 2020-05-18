using System;
using System.Collections.Generic;
using System.Text;

namespace TileSetCompiler.Creators.Data
{
    public class MissileData
    {
        public MissileData()
        {

        }

        public MissileData(string fileSuffix, MissileDirection direction)
        {
            FileSuffix = fileSuffix;
            Direction = direction;
        }

        public string FileSuffix { get; set; }
        public MissileDirection Direction { get; set; }

    }
}
