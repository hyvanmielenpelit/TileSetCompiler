using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ReplacementCompiler : ItemCompiler
    {
        const string _subDirName = "Replacement";
        const int _lineLength = 7;
        const string _missingReplacementType = "Replacement";
        const string _missingFloorTileType = "FloorRepl";

        public MissingTileCreator MissingReplacementCreator { get; set; }
        public MissingTileCreator MissingReplacementFloorCreator { get; set; }

        public ReplacementCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingReplacementCreator = new MissingTileCreator();
            MissingReplacementCreator.TextColor = Color.Black;

            MissingReplacementFloorCreator = new MissingTileCreator();
            MissingReplacementFloorCreator.TextColor = Color.Black;
            MissingReplacementFloorCreator.BitmapSize = Program.ItemSize;

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

            if (file.Exists)
            {
                Console.WriteLine("Compiled Replacement '{0}' successfully.", relativePath);
                WriteTileNameSuccess(relativePath);

                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if (image.Size == Program.ItemSize)
                    {
                        var fileNameFloor = replacementName.ToFileName() + "_" + tileName.ToFileName() + _floorSuffix + Program.ImageFileExtension;
                        var filePathFloor = Path.Combine(dirPath, fileNameFloor);
                        FileInfo fileFloor = new FileInfo(filePathFloor);

                        var baseTileData = GetTileFile(baseTileNumber);
                        var floorTileData = baseTileData.FloorTileData;

                        FloorTileData floorTileDataReplacement = floorTileData != null ? new FloorTileData(fileFloor, floorTileData.HasTileFile, floorTileData.SubType, floorTileData.NameOrDesc) : null;

                        using (var floorImage = GetFloorTile(fileFloor, floorTileData, replacementName, tileName))
                        {
                            DrawItemToTileSet(image, false, mainTileAlignment, floorImage);
                            StoreTileFile(file, image.Size, floorTileDataReplacement);
                        }
                    }
                    else if (image.Size == Program.MaxTileSize)
                    {
                        DrawImageToTileSet(image);
                        StoreTileFile(file, image.Size);
                    }
                    else
                    {
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                        StoreTileFile(file, image.Size);
                    }
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

        private Bitmap GetFloorTile(FileInfo fileFloor, FloorTileData floorTileData, string replacementName, string tileName)
        {
            if (floorTileData != null && floorTileData.HasTileFile)
            {
                if (fileFloor.Exists)
                {
                    return new Bitmap(Image.FromFile(fileFloor.FullName));
                }
                else
                {
                    return MissingReplacementFloorCreator.CreateTileWithTextLines(_missingFloorTileType, replacementName, tileName);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
