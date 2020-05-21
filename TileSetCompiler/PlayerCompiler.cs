using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class PlayerCompiler : BitmapCompiler
    {
        const string _subDirName = "Player";
        const int _lineLength = 7;
        const string _alignmantAny = "any";
        const string _missingTileType = "Player";
        const string _typeNormal = "normal";

        private Dictionary<string, string> _typeSuffix = new Dictionary<string, string>()
        {
            { "normal", "" },
            { "body", "_body" },
            { "attack", "_attack" }
        };

        public MissingTileCreator MissingPlayerTileCreator { get; set; }

        public PlayerCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingPlayerTileCreator = new MissingTileCreator();
            MissingPlayerTileCreator.BackgroundColor = Color.White;
            MissingPlayerTileCreator.SetTextFont(FontFamily.GenericSansSerif, 10.0f);
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Player line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];

            if(!_typeSuffix.ContainsKey(type))
            {
                throw new Exception(string.Format("Player Type '{0}' not found in _typeSuffix. Line: {1}", type, string.Join(',', splitLine)));
            }

            var typeSuffix = _typeSuffix[type];

            var role = splitLine[2];
            var race = splitLine[3];
            var gender = splitLine[4];
            var alignment = splitLine[5];
            var level = splitLine[6]; //Not used for now

            var subDir2 = Path.Combine(race.ToLower().Replace(" ", "_"), role.ToLower().Replace(" ", "_"));

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);

            string alignmentSuffix = alignment == _alignmantAny ? "" : "_" + alignment.ToLower().Replace(" ", "_");
            string fileName = race.ToLower().Replace(" ", "_") + "_" + role.ToLower().Replace(" ", "_") + "_" + gender.ToLower().Replace(" ", "_") + 
                alignmentSuffix + typeSuffix + Program.ImageFileExtension;
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
                    Console.WriteLine("Compiled Player Tile {0} successfully.", relativePath);
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
                    CropAndDrawImageToTileSet(image);
                    StoreTileFile(file);
                }
            }
            else
            {
                using (var image = MissingPlayerTileCreator.CreateTileWithTextLines(_missingTileType, 
                    race.ToProperCase(), role.ToProperCase(), gender.ToProperCase(),
                    (alignment != _alignmantAny ? alignment.ToProperCase() : ""),
                    (type != _typeNormal ? type.ToProperCase() : "")))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
