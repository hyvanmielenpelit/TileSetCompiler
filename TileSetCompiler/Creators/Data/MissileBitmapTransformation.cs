using System;
using System.Collections.Generic;
using System.Text;

namespace TileSetCompiler.Creators.Data
{
    public class MissileBitmapTransformation
    {
        public bool FlipHorizontally { get; set; }
        public float Rotation { get; set; }

        public MissileBitmapTransformation()
        {

        }

        public MissileBitmapTransformation(bool flipHorizontally, float rotation)
        {
            FlipHorizontally = flipHorizontally;
            Rotation = rotation;
        }
    }
}
