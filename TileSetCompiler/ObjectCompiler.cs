using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class ObjectCompiler : BitmapCompiler
    {
        const string _subDirName = "Objects";
        const int _lineLength = 5;
        const string _noDescription = "no description";
        const string _missingTileType = "Item";

        public MissingTileCreator MissingObjectTileCreator { get; set; }

        public ObjectCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingObjectTileCreator = new MissingTileCreator();
            MissingObjectTileCreator.TextColor = Color.DarkBlue;
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
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Object directory '{0}' not found. Creating Missing Object Tile.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Object Tile.");
            }
            else
            {
                if (file.Exists)
                {
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Object Tile.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Object Tile.");
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
                using (var image = MissingObjectTileCreator.CreateTile(_missingTileType, objectTypeSingular, name))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
