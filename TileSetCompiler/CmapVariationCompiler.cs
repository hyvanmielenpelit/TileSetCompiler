using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Creators.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class CmapVariationCompiler : BitmapCompiler
    {
        const string _subDirName = "Cmap Variations";
        const int _lineLength = 6;
        const string _cmapVariationSubType = "Variation";
        const string _missingCmapType = "Cmap";

        protected MissingTileCreator MissingCmapVariationTileCreator { get; set; }

        public CmapVariationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingCmapVariationTileCreator = new MissingTileCreator();
            MissingCmapVariationTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapVariationTileCreator.TextColor = Color.DarkGreen;
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

            var nameWithoutIndex = GetNameWithoutIndex(name);

            int widthInTiles = int.Parse(splitLine[3]);
            int heightInTiles = int.Parse(splitLine[4]);
            MainTileAlignment mainTileAlignment = GetMainTileAlignment(splitLine[5]);

            var subDir = map.ToFileName();

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir);
            var fileName = map.ToFileName() + "_" + name.ToFileName() + Program.ImageFileExtension;
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
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                    }
                    StoreTileFile(file);
                }
            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Cmap Variation tile.", file.FullName);
                WriteCmapTileNameErrorFileNotFound(relativePath, null, "Creating Missing Cmap Variation tile.");
                using (var image = MissingCmapVariationTileCreator.CreateTile(_missingCmapType, _cmapVariationSubType, name))
                {
                    DrawImageToTileSet(image);
                }
            }

            IncreaseCurXY();            
        }
    }
}
