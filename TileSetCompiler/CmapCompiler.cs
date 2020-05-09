using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    class CmapCompiler : BitmapCompiler
    {
        const string _subDirName = "Cmap";
        const string _unknownFileName = "UnknownCmap.png";
        const int _lineLength = 3;

        public CmapCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownFileName, tileNameWriter)
        {

        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Cmap line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var map = splitLine[1];
            var name = splitLine[2];
            var subDir2 = map;

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            FileInfo usedFile = null;
            var fileName = map.ToLower() + "_" + name.Substring(2).ToLower() + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Cmap directory '{0}' not found. Using Unknown Cmap icon.", dirPath);
                usedFile = UnknownFile;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Cmap icon.");
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
                    Console.WriteLine("File '{0}' not found. Using Unknown Cmap icon.", file.FullName);
                    usedFile = UnknownFile;
                    WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Cmap icon.");
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
