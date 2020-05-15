using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class MonsterCompiler : BitmapCompiler
    {
        const string _subDirName = "Monsters";
        const int _monsterLineLength = 4;
        const string _type_normal = "normal";
        const string _type_statue = "statue";
        const string _statueDirName = "statues";
        const string _normalDirName = "normal";
        const string _missingMonsterTileType = "Monster";

        private Dictionary<string, string> _genderSuffix = new Dictionary<string, string>()
        {
            { "male", "_male" },
            { "female", "_female" },
            { "base", "" }
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
            string genderSuffix = _genderSuffix[gender];

            var type = splitLine[2];
            var name = splitLine[3];
            if (type == _type_normal)
            {
                var subDir2 = Path.Combine(_normalDirName, name.ToLower().Replace(" ", "_"));

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
                        DrawImageToTileSet(image);
                    }
                }
                else
                {
                    using (var image = MissingMonsterTileCreator.CreateTile(_missingMonsterTileType, "", name))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }
            else if (type == _type_statue)
            {
                var sourceSubDir2 = Path.Combine(_normalDirName, name.ToLower().Replace(" ", "_"));

                var sourceMonsterDirPath = Path.Combine(BaseDirectory.FullName, sourceSubDir2);
                var sourceFileName = name.ToLower().Replace(" ", "_") + genderSuffix + Program.ImageFileExtension;
                var sourceRelativePath = Path.Combine(_subDirName, sourceSubDir2, sourceFileName);
                FileInfo sourceFile = new FileInfo(Path.Combine(sourceMonsterDirPath, sourceFileName));

                var destSubDirPath = Path.Combine(_statueDirName, name.ToLower().Replace(" ", "_"));
                string destFileName = _type_statue + "_" + name.ToLower().Replace(" ", "_") + genderSuffix + Program.ImageFileExtension;
                var destFileRelativePath = Path.Combine(_subDirName, destSubDirPath, destFileName);

                bool isUnknown;
                using (var image = StatueCreator.CreateStatueBitmapFromFile(sourceFile, name, out isUnknown))
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
                var subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), name.ToLower().Replace(" ", "_"));
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
                        DrawImageToTileSet(image);
                    }
                }
                else
                {
                    using (var image = MissingMonsterTileCreator.CreateTile(_missingMonsterTileType, type, name))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }
        }        
    }
}
