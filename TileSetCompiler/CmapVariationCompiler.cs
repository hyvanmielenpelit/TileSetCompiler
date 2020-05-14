using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class CmapVariationCompiler : BitmapCompiler
    {
        const string _subDirName = "CmapVariation";
        const string _missingCmapType = "CmapVariation";

        public DarknessCreator DarknessCreator { get; private set; }
        protected MissingTileCreator MissingCmapVariationTileCreator { get; set; }

        public CmapVariationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            DarknessCreator = new DarknessCreator(_missingCmapType);
            MissingCmapVariationTileCreator = new MissingTileCreator();
            MissingCmapVariationTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapVariationTileCreator.TextColor = Color.DarkGreen;
        }

        public override void CompileOne(string[] splitLine)
        {
            throw new NotImplementedException();
        }
    }
}
