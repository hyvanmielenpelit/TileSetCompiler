using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Creators.Data
{
    public class MissileBitmaps : Dictionary<MissileDirection, Bitmap>, IDisposable
    {
        public MissileBitmaps() : base()
        {

        }

        public void Dispose()
        {
            foreach(var kvp in this)
            {
                kvp.Value.Dispose();
            }
        }
    }
}
