using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    class MiscCompiler : BitmapCompiler
    {
        const string _subDirName = "Misc";
        const string _unknownFileName = "UnknownMisc.png";
        const string _miscInvisible = "invisible";
        const string _miscExplode = "explode";
        const string _miscZap = "zap";
        const string _miscSwallow = "swallow";
        const string _miscWarning = "warning";

        private Dictionary<string, int> _lineLengths = new Dictionary<string, int>()
        {
            { _miscInvisible, 3 },
            { _miscExplode, 4 },
            { _miscZap, 4 },
            { _miscSwallow, 4 },
            { _miscWarning, 3 }
        };

        public MiscCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownFileName, tileNameWriter)
        {

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
            if (type == _miscInvisible)
            {
                var type2 = splitLine[2];
                subDir2 = type;
                fileName = type2 + Program.ImageFileExtension;
            }
            else if (type == _miscExplode)
            {
                var type2 = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type, type2);
                fileName = direction + Program.ImageFileExtension;
            }
            else if (type == _miscZap)
            {
                var type2 = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type, type2);
                fileName = type2 + "_" + direction + Program.ImageFileExtension;
            }
            else if (type == _miscSwallow)
            {
                var monster = splitLine[2];
                var direction = splitLine[3];
                subDir2 = Path.Combine(type, monster);
                fileName = monster.ToLower() + "_" + direction + Program.ImageFileExtension;
            }
            else if (type == _miscWarning)
            {
                var level = splitLine[2];
                subDir2 = type;
                fileName = level + Program.ImageFileExtension;
            }

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            FileInfo usedFile = null;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Misc directory '{0}' not found. Using Unknown Misc icon.", dirPath);
                usedFile = UnknownFile;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Misc icon.");
            }
            else
            {
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                if (file.Exists)
                {
                    usedFile = file;
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Using Unknown Misc icon.", file.FullName);
                    usedFile = UnknownFile;
                    WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Misc icon.");
                }
            }

            using (var image = new Bitmap(Image.FromFile(usedFile.FullName)))
            {
                DrawImageToTileSet(image);
                IncreaseCurXY();
            }
        }
    }
}
