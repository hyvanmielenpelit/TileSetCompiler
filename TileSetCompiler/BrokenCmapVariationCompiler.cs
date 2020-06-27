using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class BrokenCmapVariationCompiler : DungeonTileCompiler
    {
        const string _subDirName = "Cmap Variations";
        const int _lineLength = 6;
        const string _cmapVariationSubType = "Variation";
        const string _brokenSuffix = "_broken";
        const string _missingBrokenCmapType = "Broken Cmap";

        protected MissingTileCreator MissingCmapVariationTileCreator { get; set; }

        public BrokenCmapVariationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingCmapVariationTileCreator = new MissingTileCreator();
            MissingCmapVariationTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapVariationTileCreator.TextColor = Color.DarkRed;
            MissingCmapVariationTileCreator.Capitalize = false;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Cmap Variation line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var map = splitLine[1];
            var name = splitLine[2];

            int widthInTiles = int.Parse(splitLine[3]);
            int heightInTiles = int.Parse(splitLine[4]);
            MainTileAlignment mainTileAlignment = GetMainTileAlignment(splitLine[5]);

            var subDir = map.ToFileName();
            var dirPath = Path.Combine(BaseDirectory.FullName, subDir);
            var fileName = map.ToFileName() + "_" + name.ToFileName() + _brokenSuffix + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                WriteCmapTileNameSuccess(relativePath, null);
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if (image.Size == Program.MaxTileSize)
                    {
                        DrawImageToTileSet(image);
                    }
                    else
                    {
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment);
                    }
                    StoreTileFile(file);
                }
            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Broken Cmap Variation tile.", file.FullName);
                WriteCmapTileNameErrorFileNotFound(relativePath, null, "Creating Missing Broken Cmap Variation tile.");
                using (var image = MissingCmapVariationTileCreator.CreateTile(_missingBrokenCmapType, _cmapVariationSubType, name))
                {
                    DrawImageToTileSet(image);
                }
            }

            IncreaseCurXY();
        }
    }
}
