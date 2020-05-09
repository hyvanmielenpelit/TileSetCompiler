using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    class ArtifactCompiler : BitmapCompiler
    {
        const string _subDirName = "Artifacts";
        const string _unknownFileName = "UnknownArtifact.png";
        const int _lineLength = 3;

        public ArtifactCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownFileName, tileNameWriter)
        {

        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Artifact line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var name = splitLine[2];

            var dirPath = BaseDirectory.FullName;
            FileInfo usedFile = null;
            var fileName = name + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, fileName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Artifact directory '{0}' not found. Using Unknown Artifact icon.", dirPath);
                usedFile = UnknownFile;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Artifact icon.");
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
                    Console.WriteLine("File '{0}' not found. Using Unknown Artifact icon.", file.FullName);
                    usedFile = UnknownFile;
                    WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Artifact icon.");
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
