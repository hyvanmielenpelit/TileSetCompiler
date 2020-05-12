using System;
using System.Collections.Generic;
using System.Text;

namespace TileSetCompiler.Creators.Data
{
    public class CmapDarknessCreatorData
    {
        public string OriginalCmapName { get; set; }
        public float Opacity { get; set; }

        public CmapDarknessCreatorData()
        {

        }

        public CmapDarknessCreatorData(string originalCmapName, float opacity)
        {
            OriginalCmapName = originalCmapName;
            Opacity = opacity;
        }
    }
}
