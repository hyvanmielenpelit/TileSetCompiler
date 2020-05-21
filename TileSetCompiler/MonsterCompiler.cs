using System;
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
        const string _type_normal = "normal";
        const string _type_statue = "statue";
        const string _statueDirName = "statues";
        const string _normalDirName = "normal";
        const string _missingMonsterTileType = "Monster";

        private Dictionary<string, CategoryInfo> _genderSuffix = new Dictionary<string, CategoryInfo>()
        {
            { "male", new CategoryInfo("_male", "Male") },
            { "female", new CategoryInfo("_female", "Female") },
            { "base", new CategoryInfo("", "") }
        };

        private Dictionary<string, CategoryInfo> _typeSuffix = new Dictionary<string, CategoryInfo>()
        {
            { "normal", new CategoryInfo("", "") },
            { "statue", new CategoryInfo("_statue", "Statue") },
            { "body", new CategoryInfo("_body", "Body") },
            { "attack", new CategoryInfo("_attack", "Attack") }
        };

        public static string MonsterDirectoryName { get { return _subDirName; } }
        public static string StatueDirectoryName { get { return _statueDirName; } }

        public StatueCreator StatueCreator { get; private set; }
        protected MissingTileCreator MissingMonsterTileCreator { get; private set; }

        public MonsterCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingMonsterTileCreator = new MissingTileCreator();
            MissingMonsterTileCreator.TextColor = Color.Red;
            MissingMonsterTileCreator.Capitalize = true;

            StatueCreator = new StatueCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _monsterLineLength)
            {
                throw new Exception(string.Format("Monster line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var gender = splitLine[1];
            if(!_genderSuffix.ContainsKey(gender))
            {
                throw new Exception(string.Format("Invalid gender '{0}' in monster line '{1}'.", gender, string.Join(',', splitLine)));
            }
            string genderSuffix = _genderSuffix[gender].Suffix;

            var type = splitLine[2];
            var name = splitLine[3];
            var widthInTiles = int.Parse(splitLine[4]);
            var heightInTiles = int.Parse(splitLine[5]);
            var mainTileAligntmentInt = int.Parse(splitLine[6]);
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAligntmentInt;

            if (type == _type_normal)
            {
                var subDir2 = name.ToLower().Replace(" ", "_");

                var monsterDirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var fileName = name.ToLower().Replace(" ", "_") + genderSuffix + Program.ImageFileExtension;
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(monsterDirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                if (!Directory.Exists(monsterDirPath))
                {
                    Console.WriteLine("Monster directory '{0}' not found. Creating a Missing Monster Tile.", monsterDirPath);
                    isTileMissing = true;
                    WriteTileNameErrorDirectoryNotFound(relativePath, "Creating a Missing Monster Tile.");
                }
                else
                {

                    if (file.Exists)
                    {
                        Console.WriteLine("Compiled Monster Tile {0} successfully.", relativePath);
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        Console.WriteLine("Monster file '{0}' not found. Creating a Missing Monster Tile.", file.FullName);
                        isTileMissing = true;
                        WriteTileNameErrorFileNotFound(relativePath, "Creating a Missing Monster Tile.");
                    }
                }

                if(!isTileMissing)
                {
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        if(image.Width != widthInTiles * Program.MaxTileSize.Width || image.Height != heightInTiles * Program.MaxTileSize.Height)
                        {
                            throw new WrongSizeException(image.Size, new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height),
                                string.Format("Monster Tile '{0}' is wrong size ({1}x{2}). It should be {3}x{4}.", file.FullName,
                                image.Width, image.Height, widthInTiles * Program.MaxTileSize.Width, image.Height != heightInTiles * Program.MaxTileSize.Height));
                        }
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment);
                        StoreTileFile(file);
                    }
                }
                else
                {
                    using (var image = MissingMonsterTileCreator.CreateTileWithTextLines(_missingMonsterTileType, _typeSuffix[type].Description, name, _genderSuffix[gender].Description))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }
            else if (type == _type_statue)
            {
                var sourceSubDir2 =name.ToLower().Replace(" ", "_");

                var sourceMonsterDirPath = Path.Combine(BaseDirectory.FullName, sourceSubDir2);
                var sourceFileName = name.ToLower().Replace(" ", "_") + genderSuffix + Program.ImageFileExtension;
                var sourceRelativePath = Path.Combine(_subDirName, sourceSubDir2, sourceFileName);
                FileInfo sourceFile = new FileInfo(Path.Combine(sourceMonsterDirPath, sourceFileName));

                var destSubDirPath = Path.Combine(name.ToLower().Replace(" ", "_"));
                string destFileName = name.ToLower().Replace(" ", "_") + genderSuffix + _typeSuffix[type].Suffix + Program.ImageFileExtension;
                var destFileRelativePath = Path.Combine(_subDirName, destSubDirPath, destFileName);

                bool isUnknown;
                using (var image = StatueCreator.CreateStatueBitmapFromFile(sourceFile, name, _genderSuffix[gender].Description, out isUnknown))
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
                    IncreaseCurXY();
                }
            }
            else
            {
                //Other type
                var subDir2 = name.ToLower().Replace(" ", "_");
                var monsterDirPath = Path.Combine(BaseDirectory.FullName, subDir2);

                if(!_typeSuffix.ContainsKey(type))
                {
                    throw new Exception(string.Format("Unknown monster type '{0}'.", type));
                }

                var fileName = name.ToLower().Replace(" ", "_") + 
                    genderSuffix + 
                    _typeSuffix[type].Suffix +
                    Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(monsterDirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                if (!Directory.Exists(monsterDirPath))
                {
                    Console.WriteLine("Monster directory '{0}' not found. Creating a Missing Monster Tile.", monsterDirPath);
                    isTileMissing = true;
                    WriteTileNameErrorDirectoryNotFound(relativePath, "Creating a Missing Monster Tile.");
                }
                else
                {

                    if (file.Exists)
                    {
                        Console.WriteLine("Created Monster {0} Tile {1} successfully.", type.ToProperCase(), relativePath);
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        Console.WriteLine("Monster file '{0}' not found. Creating a Missing Monster Tile.", file.FullName);
                        isTileMissing = true;
                        WriteTileNameErrorFileNotFound(relativePath, "Creating a Missing Monster Tile.");
                    }
                }

                if (!isTileMissing)
                {
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        if (image.Width != widthInTiles * Program.MaxTileSize.Width || image.Height != heightInTiles * Program.MaxTileSize.Height)
                        {
                            throw new WrongSizeException(image.Size, new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height),
                                string.Format("Monster Tile '{0}' is wrong size ({1}x{2}). It should be {3}x{4}.", file.FullName,
                                image.Width, image.Height, widthInTiles * Program.MaxTileSize.Width, image.Height != heightInTiles * Program.MaxTileSize.Height));
                        }
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment);
                        StoreTileFile(file);
                    }
                }
                else
                {
                    using (var image = MissingMonsterTileCreator.CreateTileWithTextLines(_missingMonsterTileType, _typeSuffix[type].Description, name, _genderSuffix[gender].Description))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }
        }        
    }
}
