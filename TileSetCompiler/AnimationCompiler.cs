using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;

namespace TileSetCompiler
{
    class AnimationCompiler : ItemCompiler
    {
        const string _subDirName = "Animation";
        const int _lineLength = 3;
        const string _missingAnimationType = "Animation";

        public MissingTileCreator MissingAnimationCreator { get; set; }

        public AnimationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingAnimationCreator = new MissingTileCreator();
            MissingAnimationCreator.TextColor = Color.Black;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Animation line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var animation = splitLine[1];
            var frame = splitLine[2];

            var dirPath = Path.Combine(BaseDirectory.FullName, animation.ToLower().Replace(" ", "_"));
            var fileName = animation.ToLower().Replace(" ", "_") + "_" + frame.ToLower().Replace(" ", "_") + Program.ImageFileExtension;

            var relativePath = Path.Combine(_subDirName, animation.ToLower().Replace(" ", "_"), fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            bool isTileMissing = false;

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Animation directory '{0}' not found. Creating Missing Animation.", dirPath);
                isTileMissing = true;
                WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Animation.");
            }
            else
            {
                if (file.Exists)
                {
                    Console.WriteLine("Compiled Animation '{0}' successfully.", relativePath);
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Animation.", file.FullName);
                    isTileMissing = true;
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Animation.");
                }
            }

            if (!isTileMissing)
            {
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if(image.Size == Program.ItemSize)
                    {
                        DrawItemToTileSet(image);
                    }
                    else
                    {
                        CropAndDrawImageToTileSet(image);
                    }
                    StoreTileFile(file);
                }
            }
            else
            {
                using (var image = MissingAnimationCreator.CreateTile(_missingAnimationType, animation, frame))
                {
                    DrawImageToTileSet(image);
                }
            }
            IncreaseCurXY();

        }
    }
}
