using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class ArtifactCompiler : BitmapCompiler
    {
        const string _subDirName = "Artifacts";
        const int _lineLength = 3;
        const string _artifactMissingTileType = "Item";

        public MissingTileCreator MissingArtifactTileCreator { get; set; }

        public ArtifactCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingArtifactTileCreator = new MissingTileCreator();
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
            var fileName = name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Artifact directory '{0}' not found. Creating Missing Artifact icon.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Artifact icon.");
            }
            else
            {
                if (file.Exists)
                {
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Artifact icon.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Artifact icon.");
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
                using (var image = MissingArtifactTileCreator.CreateTile(_artifactMissingTileType, "", name))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
