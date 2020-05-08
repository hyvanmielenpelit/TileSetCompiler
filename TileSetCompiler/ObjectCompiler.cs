using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    class ObjectCompiler : BitmapCompiler
    {
        const string _subDirName = "Objects";
        const string _unknownFileName = "UnknownObject.png";
        const int _lineLength = 3;

        public ObjectCompiler() : base(_subDirName, _unknownFileName)
        {

        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Object line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var name = splitLine[2];

            var dirPath = Path.Combine(BaseDirectory.FullName);
            FileInfo usedFile = null;
            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Object directory '{0}' not found. Using Unknown Object icon.", dirPath);
                usedFile = UnknownFile;
            }
            else
            {
                var fileName = name.ToLower() + Program.ImageFileExtension;
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                if (file.Exists)
                {
                    usedFile = file;
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Using Unknown Object icon.", file.FullName);
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
