using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class MiscCompiler : BitmapCompiler
    {
        const string _subDirName = "Misc";
        const string _miscInvisible = "invisible";
        const string _miscExplode = "explode";
        const string _miscZap = "zap";
        const string _miscSwallow = "swallow";
        const string _miscWarning = "warning";
        const string _missingTileType = "Misc";

        private Dictionary<string, int> _lineLengths = new Dictionary<string, int>()
        {
            { _miscInvisible, 3 },
            { _miscExplode, 4 },
            { _miscZap, 4 },
            { _miscSwallow, 4 },
            { _miscWarning, 3 }
        };

        public MissingTileCreator MissingMiscTileCreator { get; set; }

        public MiscCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingMiscTileCreator = new MissingTileCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < 2)
            {
                throw new Exception(string.Format("Misc line '{0}' has less than 2 elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            if(!_lineLengths.ContainsKey(type))
            {
                throw new Exception(string.Format("Misc type '{0}' not valid.", type));
            }
            int lineLength = _lineLengths[type];
            if (splitLine.Length < lineLength)
            {
                throw new Exception(string.Format("Misc line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            string subDir2 = null;
            string fileName = null;
            string name = "";
            if (type == _miscInvisible)
            {
                var type2 = splitLine[2];
                subDir2 = type.ToLower().Replace(" ", "_");
                fileName = type2.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                name = type2;
            }
            else if (type == _miscExplode)
            {
                var type2 = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), type2.ToLower().Replace(" ", "_"));
                fileName = direction.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                name = direction;
            }
            else if (type == _miscZap)
            {
                var type2 = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), type2.ToLower().Replace(" ", "_"));
                fileName = type2.ToLower().Replace(" ", "_") + "_" + direction.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                name = type2;
            }
            else if (type == _miscSwallow)
            {
                var monster = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), monster.ToLower().Replace(" ", "_"));
                fileName = monster.ToLower().Replace(" ", "_") + "_" + direction.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                name = direction;
            }
            else if (type == _miscWarning)
            {
                var level = splitLine[2];
                subDir2 = type.ToLower().Replace(" ", "_");
                fileName = level.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                name = level;
            }
            else
            {
                //Other type
                if(splitLine.Length == 3)
                {
                    name = splitLine[2];
                    subDir2 = type.ToLower().Replace(" ", "_");
                    fileName = name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                }
                else if (splitLine.Length >= 4)
                {
                    var category = splitLine[2];
                    name = splitLine[3];
                    subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), category.ToLower().Replace(" ", "_"));
                    fileName = category.ToLower().Replace(" ", "_") + "_" + name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                }
                else
                {
                    throw new Exception(string.Format("Misc line too short: {0} elements. Line is: '{1}'", splitLine.Length, string.Join(',', splitLine)));
                }
            }

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            FileInfo usedFile = null;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Misc directory '{0}' not found. Creating Missing Misc tile.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Misc tile.");
            }
            else
            {
                if (file.Exists)
                {
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Misc tile.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Misc tile.");
                }
            }

            if(!isTileMissing)
            {
                using (var image = new Bitmap(Image.FromFile(usedFile.FullName)))
                {
                    DrawImageToTileSet(image);
                }
            }
            else
            {
                using (var image = MissingMiscTileCreator.CreateTile(_missingTileType, type, name))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
