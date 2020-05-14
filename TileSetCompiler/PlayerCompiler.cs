using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class PlayerCompiler : BitmapCompiler
    {
        const string _subDirName = "Player";
        const int _lineLength = 6;
        const string _alignmantAny = "any";
        const string _missingTileType = "Player";

        public MissingTileCreator MissingPlayerTileCreator { get; set; }

        public PlayerCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingPlayerTileCreator = new MissingTileCreator();
            MissingPlayerTileCreator.BackgroundColor = Color.White;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Player line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var role = splitLine[1];
            var race = splitLine[2];
            var gender = splitLine[3];
            var alignment = splitLine[4];
            var level = splitLine[5]; //Not used for now

            var subDir2 = Path.Combine(race.ToLower().Replace(" ", "_"), role.ToLower().Replace(" ", "_"));

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);

            string alignmentSuffix = alignment == _alignmantAny ? "" : "_" + alignment.ToLower().Replace(" ", "_");
            string fileName = race.ToLower().Replace(" ", "_") + "_" + role.ToLower().Replace(" ", "_") + "_" + gender.ToLower().Replace(" ", "_") + alignmentSuffix + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Player directory '{0}' not found. Creating Missing Player Tile.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Player Tile.");
            }
            else
            {
                if (file.Exists)
                {
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Player Tile.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Player Tile.");
                }
            }

            if(!isTileMissing)
            {
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    DrawImageToTileSet(image);
                }
            }
            else
            {
                using (var image = MissingPlayerTileCreator.CreateTile(_missingTileType, race, role))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
