using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Creators.Data
{
    public class RecolorData
    {
        public string BaseFileName { get; set; }
        public string MaskFileName { get; set; }
        public Dictionary<Color, Color> ColorMappings { get; set; }

        public RecolorData(string baseFileName, string maskFileName, Dictionary<Color, Color> colorMappings)
        {
            BaseFileName = baseFileName;
            MaskFileName = maskFileName;
            ColorMappings = colorMappings;
        }
    }
}
