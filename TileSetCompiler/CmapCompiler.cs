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

        public CmapCompiler() : base(_subDirName, _unknownFileName)
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

            var dirPath = Path.Combine(BaseDirectory.FullName, map);
            FileInfo usedFile = null;
            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Cmap directory '{0}' not found. Using Unknown Cmap icon.", dirPath);
                usedFile = UnknownFile;
            }
            else
            {
                var fileName = map.ToLower() + "_" + name.ToLower() + Program.ImageFileExtension;
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                if (file.Exists)
                {
                    usedFile = file;
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Using Unknown Cmap icon.", file.FullName);
                    usedFile = UnknownFile;
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
