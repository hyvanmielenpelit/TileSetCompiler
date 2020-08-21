using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ReplacementCompiler : ItemCompiler
    {
        const string _subDirName = "Replacement";
        const int _lineLength = 7;
        const string _missingReplacementType = "Replacement";

        public MissingTileCreator MissingReplacementCreator { get; set; }

        public ReplacementCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingReplacementCreator = new MissingTileCreator();
            MissingReplacementCreator.TextColor = Color.Black;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Replacement line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var replacementName = splitLine[1];
            var tileName = splitLine[2];
            var baseTileNumber = int.Parse(splitLine[3]); //Not used   
            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            int mainTileAlignmentInt = int.Parse(splitLine[6]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

            var dirPath = Path.Combine(BaseDirectory.FullName, replacementName.ToFileName());
            var fileName = replacementName.ToFileName() + "_" + tileName.ToFileName() + Program.ImageFileExtension;
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            var relativePath = Path.GetRelativePath(Program.InputDirectory.FullName, file.FullName);

            if(file.Exists)
            {
                Console.WriteLine("Compiled Replacement '{0}' successfully.", relativePath);
                WriteTileNameSuccess(relativePath);

                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if (image.Size == Program.ItemSize)
                    {
                        DrawItemToTileSet(image, false);
                    }
                    else if (image.Size == Program.MaxTileSize)
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
                Console.WriteLine("File '{0}' not found. Creating Missing Replacement Tile.", file.FullName);
                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Replacement Tile.");

                using (var image = MissingReplacementCreator.CreateTileWithTextLines(_missingReplacementType, replacementName, tileName))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
