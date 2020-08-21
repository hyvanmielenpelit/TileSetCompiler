using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class BrokenCmapCompiler : DungeonTileCompiler
    {
        const string _subDirName = "Cmap";
        const int _lineLength = 7;
        const string _noDescription = "no description";
        const string _brokenSuffix = "_broken";
        const string _missingBrokenCmapType = "Broken Cmap";

        protected MissingTileCreator MissingBrokenCmapTileCreator { get; set; }

        public BrokenCmapCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingBrokenCmapTileCreator = new MissingTileCreator();
            MissingBrokenCmapTileCreator.BackgroundColor = Color.LightGray;
            MissingBrokenCmapTileCreator.TextColor = Color.DarkRed;
            MissingBrokenCmapTileCreator.Capitalize = false;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Broken Cmap line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var map = splitLine[1];
            var name = splitLine[2];
            var desc = splitLine[3];

            if (desc == _noDescription)
            {
                desc = "";
            }

            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            MainTileAlignment mainTileAlignment = GetMainTileAlignment(splitLine[6]);

            var subDir2 = map.ToFileName();
            var name2 = name.Substring(2) + _brokenSuffix;

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            var fileName = map.ToFileName() + "_" + name2.ToFileName() + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                WriteCmapTileNameSuccess(relativePath, desc);
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
                Console.WriteLine("File '{0}' not found. Creating Missing Broken Cmap tile.", file.FullName);
                WriteCmapTileNameErrorFileNotFound(relativePath, desc, "Creating Missing Broken Cmap tile.");

                using (var image = MissingBrokenCmapTileCreator.CreateTileWithTextLines(_missingBrokenCmapType, name2))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
