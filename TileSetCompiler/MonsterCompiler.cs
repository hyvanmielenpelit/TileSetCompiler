﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Exceptions;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class MonsterCompiler : BitmapCompiler
    {
        const string _subDirName = "Monsters";
        const int _monsterLineLength = 7;
        const string _type_statue = "statue";
        const string _type_corpse = "body";
        const string _typeNormal = "normal";
        const string _missingCorpseTileType = "Corpse";
        const string _missingMonsterTileType = "Monster";

        private Dictionary<string, CategoryData> _genderData = new Dictionary<string, CategoryData>()
        {
            { "male", new CategoryData("_male", "Male") },
            { "female", new CategoryData("_female", "Female") },
            { "base", new CategoryData("", "") }
        };

        private Dictionary<string, CategoryData> _typeData = new Dictionary<string, CategoryData>()
        {
            { "normal", new CategoryData("", "") },
            { "statue", new CategoryData("_statue", "Statue") },
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

        public StatueCreator StatueCreator { get; private set; }
        protected MissingTileCreator MissingMonsterTileCreator { get; private set; }
        protected MissingTileCreator MissingCorpseTileCreator { get; private set; }

        public MonsterCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingMonsterTileCreator = new MissingTileCreator();
            MissingMonsterTileCreator.TextColor = Color.Red;
            MissingMonsterTileCreator.Capitalize = true;

            MissingCorpseTileCreator = new MissingTileCreator();
            MissingCorpseTileCreator.TextColor = Color.DarkBlue;
            MissingCorpseTileCreator.TileSize = MissingTileSize.Item;

            StatueCreator = new StatueCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _monsterLineLength)
            {
                throw new Exception(string.Format("Monster line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var gender = splitLine[1];
            if(!_genderData.ContainsKey(gender))
            {
                throw new Exception(string.Format("Invalid gender '{0}' in monster line '{1}'.", gender, string.Join(',', splitLine)));
            }

            var type = splitLine[2];
            if (!_typeData.ContainsKey(type))
            {
                throw new Exception(string.Format("Invalid type '{0}' in monster line '{1}'.", type, string.Join(',', splitLine)));
            }

            var name = splitLine[3];
            var widthInTiles = int.Parse(splitLine[4]);
            var heightInTiles = int.Parse(splitLine[5]);
            var mainTileAligntmentInt = int.Parse(splitLine[6]);
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAligntmentInt;
            char monsterLetter = splitLine[7][0];
            var letterColorCode = int.Parse(splitLine[8]);
            var letterColor = GetColorFromColorCode(letterColorCode);

            if (type == _type_statue)
            {
                var sourceSubDir2 =name.ToLower().Replace(" ", "_");

                var sourceMonsterDirPath = Path.Combine(BaseDirectory.FullName, sourceSubDir2);
                var sourceFileName = name.ToLower().Replace(" ", "_") + _genderData[gender].Suffix + Program.ImageFileExtension;
                var sourceRelativePath = Path.Combine(_subDirName, sourceSubDir2, sourceFileName);
                FileInfo sourceFile = new FileInfo(Path.Combine(sourceMonsterDirPath, sourceFileName));

                var destSubDirPath = Path.Combine(name.ToLower().Replace(" ", "_"));
                string destFileName = name.ToLower().Replace(" ", "_") + _genderData[gender].Suffix + _typeData[type].Suffix + Program.ImageFileExtension;
                var destFileRelativePath = Path.Combine(_subDirName, destSubDirPath, destFileName);

                bool isUnknown;
                using (var image = StatueCreator.CreateStatueMainTileFromFile(sourceFile,
                    widthInTiles, heightInTiles, mainTileAlignment, name, _genderData[gender].Description, monsterLetter, out isUnknown))
                {
                    if(!isUnknown)
                    {
                        Console.WriteLine("Autogenerated Monster Statue Tile {0} successfully.", destFileRelativePath);
                        WriteTileNameAutogenerationSuccess(sourceRelativePath, destFileRelativePath, type);
                    }
                    else
                    {
                        Console.WriteLine("Monster file '{0}' not found for statue creation. Creating a Missing Monster Tile.", sourceFile.FullName);
                        WriteTileNameAutogenerationError(sourceRelativePath, destFileRelativePath, type);
                    }
                    DrawImageToTileSet(image);
                    StoreTileFile(sourceFile, image.Size, true);
                    IncreaseCurXY();
                }
            }
            else
            {
                var subDir2 = name.ToLower().Replace(" ", "_");
                var monsterDirPath = Path.Combine(BaseDirectory.FullName, subDir2);

                if(!_typeData.ContainsKey(type))
                {
                    throw new Exception(string.Format("Unknown monster type '{0}'.", type));
                }

                var fileName = name.ToLower().Replace(" ", "_") +
                     _genderData[gender].Suffix + 
                    _typeData[type].Suffix +
                    Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(monsterDirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                var fileName2 = name.ToLower().Replace(" ", "_") +
                     _genderData[gender].Suffix +
                    _typeData[_typeNormal].Suffix +
                    Program.ImageFileExtension;

                var relativePath2 = Path.Combine(_subDirName, subDir2, fileName2);
                var filePath2 = Path.Combine(monsterDirPath, fileName2);
                FileInfo file2 = new FileInfo(filePath2);

                if (file.Exists)
                {
                    string typeDesc = _typeData[type].Description;
                    if (string.IsNullOrEmpty(typeDesc))
                    {
                        typeDesc = "Normal";
                    }
                    Console.WriteLine("Created Monster {0} Tile '{1}' successfully.", typeDesc, relativePath);
                    WriteTileNameSuccess(relativePath);

                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        if (image.Width != widthInTiles * Program.MaxTileSize.Width || image.Height != heightInTiles * Program.MaxTileSize.Height)
                        {
                            throw new WrongSizeException(image.Size, new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height),
                                string.Format("Monster Tile '{0}' is wrong size ({1}x{2}). It should be {3}x{4}.", file.FullName,
                                image.Width, image.Height, widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height));
                        }
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                        StoreTileFile(file, image.Size);
                    }
                }
                else if (file2.Exists)
                {
                    string typeDesc = _typeData[_typeNormal].Description;
                    if (string.IsNullOrEmpty(typeDesc))
                    {
                        typeDesc = "Normal";
                    }
                    Console.WriteLine("Replaced Monster {0} Tile {1} with a corresponding normal tile {2}.", typeDesc, relativePath, relativePath2);
                    WriteTileNameSuccess(relativePath2);

                    using (var image = new Bitmap(Image.FromFile(file2.FullName)))
                    {
                        if (image.Width != widthInTiles * Program.MaxTileSize.Width || image.Height != heightInTiles * Program.MaxTileSize.Height)
                        {
                            throw new WrongSizeException(image.Size, new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height),
                                string.Format("Monster Tile '{0}' is wrong size ({1}x{2}). It should be {3}x{4}.", file2.FullName,
                                image.Width, image.Height, widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height));
                        }
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file2);
                        StoreTileFile(file2, image.Size);
                    }
                }
                else
                {
                    if(type == _type_corpse)
                    {
                        Console.WriteLine("Corpse file '{0}' not found. Creating a Missing Corpse Tile.", file.FullName);
                        WriteTileNameErrorFileNotFound(relativePath, "Creating a Missing Corpse Tile.");

                        using (var image = MissingCorpseTileCreator.CreateTile(_missingCorpseTileType, name, _genderData[gender].Description))
                        {
                            DrawImageToTileSet(image);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Monster file '{0}' not found. Creating a Missing Monster Tile.", file.FullName);
                        WriteTileNameErrorFileNotFound(relativePath, "Creating a Missing Monster Tile.");

                        using (var image = MissingMonsterTileCreator.CreateTileWithTextLinesAndBackgroundLetter(monsterLetter, letterColor, _missingMonsterTileType, _typeData[type].Description, name, _genderData[gender].Description))
                        {
                            DrawImageToTileSet(image);
                        }
                    }
                }
                IncreaseCurXY();
            }
        }        
    }
}
