﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class PlayerCompiler : BitmapCompiler
    {
        const string _subDirName = "Player";
        const int _lineLength = 7;
        const string _missingTileType = "Player";
        const string _typeNormal = "normal";

        private Dictionary<string, CategoryData> _typeData = new Dictionary<string, CategoryData>()
        {
            { "normal", new CategoryData("", "") },
            { "body", new CategoryData("_body", "Body") },
            { "attack", new CategoryData("_attack", "Attack") },
            { "throw", new CategoryData("_throw", "Throw") },
            { "fire", new CategoryData("_fire", "Fire") },
            { "cast-dir", new CategoryData("_cast-dir", "Cast Directional") },
            { "cast-nodir", new CategoryData("_cast-nodir", "Cast Non-Directional") },
            { "special-attack", new CategoryData("_special-attack", "Special Attack") },
            { "item-use", new CategoryData("_item-use", "Item Use") },
            { "door-use", new CategoryData("_door-use", "Door Use") },
            { "kick", new CategoryData("_kick", "Kick") },
            { "death", new CategoryData("_death", "Death") },
            { "passive-defense", new CategoryData("_passive-defense", "Passive Defense") },
            { "special-attack-2", new CategoryData("_special-attack-2", "Special Attack 2") },
            { "special-attack-3", new CategoryData("_special-attack-3", "Special Attack 3") }
        };

        private Dictionary<string, CategoryData> _alignmentData = new Dictionary<string, CategoryData>()
        {
            { "any", new CategoryData("", "") },
            { "lawful", new CategoryData("_lawful", "Lawful") },
            { "neutral", new CategoryData("_neutral", "Neutral") },
            { "chaotic", new CategoryData("_chaotic", "Chaotic") }
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

            if(!_typeData.ContainsKey(type))
            {
                throw new Exception(string.Format("Player Type '{0}' not found in _typeData. Line: {1}", type, string.Join(',', splitLine)));
            }

            var role = splitLine[2];
            var race = splitLine[3];
            var gender = splitLine[4];
            var alignment = splitLine[5];

            if (!_alignmentData.ContainsKey(alignment))
            {
                throw new Exception(string.Format("Player Alignment '{0}' not found in _alignmentData. Line: {1}", alignment, string.Join(',', splitLine)));
            }

            var level = splitLine[6]; //Not used for now

            var subDir2 = Path.Combine(race.ToFileName(), role.ToFileName());

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);

            string fileName = race.ToFileName() + "_" + role.ToFileName() + "_" + gender.ToFileName() + 
                _alignmentData[alignment].Suffix + _typeData[type].Suffix + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            string fileName2 = race.ToFileName() + "_" + role.ToFileName() + "_" + gender.ToFileName() +
                _alignmentData[alignment].Suffix + _typeData[_typeNormal].Suffix + Program.ImageFileExtension;
            var relativePath2 = Path.Combine(_subDirName, subDir2, fileName2);
            var filePath2 = Path.Combine(dirPath, fileName2);
            FileInfo file2 = new FileInfo(filePath2);

            if (file.Exists)
            {
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    CropAndDrawImageToTileSet(image);
                    StoreTileFile(file, image.Size);
                }

                Console.WriteLine("Compiled Player Tile {0} successfully.", relativePath);
                WriteTileNameSuccess(relativePath);
            }
            else if (file2.Exists)
            {
                using (var image = new Bitmap(Image.FromFile(file2.FullName)))
                {
                    CropAndDrawImageToTileSet(image);
                    StoreTileFile(file2, image.Size);
                }

                Console.WriteLine("Replaced Player Tile {0} with a corresponding normal tile {1}.", relativePath, relativePath2);
                WriteTileReplacementSuccess(relativePath, relativePath2);
            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Player Tile.", file.FullName);
                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Player Tile.");

                using (var image = MissingPlayerTileCreator.CreateTileWithTextLines(_missingTileType, 
                    race, role, gender, _alignmentData[alignment].Description, _typeData[type].Description))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
