using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class UICompiler : BitmapCompiler
    {
        const string _subDirName = "UI";
        const int _lineLength = 3;
        const string _missingUIType = "UI";

        public MissingTileCreator MissingUITileCreator { get; set; }

        public UICompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingUITileCreator = new MissingTileCreator();
            MissingUITileCreator.TextColor = Color.Yellow;
            MissingUITileCreator.BackgroundColor = Color.Gray;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("User Interface line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var name = splitLine[2];

            var dirPath = Path.Combine(BaseDirectory.FullName, type.ToLower().Replace(" ", "_"));
            var fileName = type.ToLower().Replace(" ", "_") + "_" + name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;

            var relativePath = Path.Combine(_subDirName, type.ToLower().Replace(" ", "_"), fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("User Interface directory '{0}' not found. Creating Missing UI icon.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing UI icon.");
            }
            else
            {
                if (file.Exists)
                {
                    Console.WriteLine("Compiled UI Tile '{0}' successfully.", relativePath);
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing UI Tile.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing UI Tile.");
                }
            }

            if (!isTileMissing)
            {
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    DrawImageToTileSet(image);
                }
            }
            else
            {
                using (var image = MissingUITileCreator.CreateTile(_missingUIType, type, name))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();
        }
    }
}
