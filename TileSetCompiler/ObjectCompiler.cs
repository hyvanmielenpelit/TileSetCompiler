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
        const int _lineLength = 5;
        const string _noDescription = "no description";

        public ObjectCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownFileName, tileNameWriter)
        {

        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Object line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var objectType = splitLine[2];
            var name = splitLine[3];
            var desc = splitLine[4];

            var subDir2 = objectType.ToLower().Replace(" ", "_");

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            FileInfo usedFile = null;
            var objectTypeSingular = objectType.ToLower();
            if (objectTypeSingular.EndsWith("s"))
            {
                objectTypeSingular = objectTypeSingular.Substring(0, objectTypeSingular.Length - 1);
            }

            string fileName = null;
            if(string.IsNullOrWhiteSpace(desc) || desc == _noDescription)
            {
                fileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" + name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
            }
            else
            {
                fileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" + desc.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
            }
            
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Object directory '{0}' not found. Using Unknown Object icon.", dirPath);
                usedFile = UnknownFile;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Object icon.");
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
                    Console.WriteLine("File '{0}' not found. Using Unknown Object icon.", file.FullName);
                    usedFile = UnknownFile;
                    WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Object icon.");
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
