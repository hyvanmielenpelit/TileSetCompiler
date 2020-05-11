using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    class PlayerCompiler : BitmapCompiler
    {
        const string _subDirName = "Player";
        const string _unknownFileName = "UnknownPlayer.png";
        const int _lineLength = 6;
        const string _alignmantAny = "any";

        public PlayerCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownFileName, tileNameWriter)
        {

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

            FileInfo usedFile = null;
            string alignmentSuffix = alignment == _alignmantAny ? "" : "_" + alignment.ToLower().Replace(" ", "_");
            string fileName = race.ToLower().Replace(" ", "_") + "_" + role.ToLower().Replace(" ", "_") + "_" + gender.ToLower().Replace(" ", "_") + alignmentSuffix + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Player directory '{0}' not found. Using Unknown Player icon.", dirPath);
                usedFile = UnknownFile;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Player icon.");
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
                    Console.WriteLine("File '{0}' not found. Using Unknown Player icon.", file.FullName);
                    usedFile = UnknownFile;
                    WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Player icon.");
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
