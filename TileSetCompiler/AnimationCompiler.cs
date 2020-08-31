using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Exceptions;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class AnimationCompiler : ItemCompiler
    {
        const string _subDirName = "Animation";
        const int _lineLength = 7;
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
            var originalTileNumber = int.Parse(splitLine[3]); //Not used   
            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            int mainTileAlignmentInt = int.Parse(splitLine[6]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

            var dirPath = Path.Combine(BaseDirectory.FullName, animation.ToFileName());
            var fileName = animation.ToFileName() + "_" + frame.ToFileName() + Program.ImageFileExtension;

            var relativePath = Path.Combine(_subDirName, animation.ToFileName(), fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                Console.WriteLine("Compiled Animation '{0}' successfully.", relativePath);
                WriteTileNameSuccess(relativePath);

                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if (image.Size == Program.ItemSize)
                    {
                        DrawItemToTileSet(image, false);
                    }
                    else if (image.Size == Program.MaxTileSize)
                    {
                        DrawImageToTileSet(image);
                    }
                    else
                    {
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                    }
                    StoreTileFile(file);
                }
            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Animation.", file.FullName);
                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Animation.");

                using (var image = MissingAnimationCreator.CreateTile(_missingAnimationType, animation, frame))
                {
                    DrawImageToTileSet(image);
                }
            }

            IncreaseCurXY();
        }
    }
}
