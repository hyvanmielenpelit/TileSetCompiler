using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Exceptions;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class AnimationCompiler : ItemCompiler
    {
        const string _subDirName = "Animation";
        const int _lineLength = 7;
        const string _missingAnimationType = "Animation";
        const string _missingFloorTileType = "FloorAnim";

        public MissingTileCreator MissingAnimationCreator { get; set; }
        public MissingTileCreator MissingAnimationFloorCreator { get; set; }


        public AnimationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingAnimationCreator = new MissingTileCreator();
            MissingAnimationCreator.TextColor = Color.Black;

            MissingAnimationFloorCreator = new MissingTileCreator();
            MissingAnimationFloorCreator.TextColor = Color.Black;
            MissingAnimationFloorCreator.BitmapSize = Program.ItemSize;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Animation line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var animation = splitLine[1];
            var frame = splitLine[2];
            var originalTileNumber = int.Parse(splitLine[3]);
            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            int mainTileAlignmentInt = int.Parse(splitLine[6]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

            TileData originalTileData = GetTileFile(originalTileNumber);
            Point? originalFilePointInTiles = originalTileData != null ? originalTileData.PointInTiles : null;
            Size? originalFileBitmapSizeInTiles = originalTileData != null ? originalTileData.BitmapSizeInTiles : null;
            bool flipHorizontal = originalTileData != null ? originalTileData.FlipHorizontal : false;
            bool flipVertical = originalTileData != null ? originalTileData.FlipVertical : false;

            var dirPath = Path.Combine(BaseDirectory.FullName, animation.ToFileName());
            var fileName = animation.ToFileName() + "_" + frame.ToFileName() + Program.ImageFileExtension;
            var fileName2 = frame.ToFileName() + Program.ImageFileExtension;
            var fileNameFloor = animation.ToFileName() + "_" + frame.ToFileName() + _floorSuffix + Program.ImageFileExtension;
            var fileNameFloor2 = frame.ToFileName() + _floorSuffix + Program.ImageFileExtension;

            var relativePath = Path.Combine(_subDirName, animation.ToFileName(), fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            var filePath2 = Path.Combine(dirPath, fileName2);
            FileInfo file2 = new FileInfo(filePath2);
            var filePathFloor = Path.Combine(dirPath, fileNameFloor);
            FileInfo fileFloor = new FileInfo(filePathFloor);
            var filePathFloor2 = Path.Combine(dirPath, fileNameFloor2);
            FileInfo fileFloor2 = new FileInfo(filePathFloor2);

            if (file.Exists || file2.Exists)
            {
                if(!file.Exists && file2.Exists)
                {
                    file = file2;
                    fileFloor = fileFloor2;
                }

                Console.WriteLine("Compiled Animation '{0}' successfully.", relativePath);
                WriteTileNameSuccess(relativePath);

                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if(originalFileBitmapSizeInTiles.HasValue && (originalFileBitmapSizeInTiles.Value.Width > 1 || originalFileBitmapSizeInTiles.Value.Height > 1))
                    {
                        Size rightSize = new Size(originalFileBitmapSizeInTiles.Value.Width * Program.MaxTileSize.Width, originalFileBitmapSizeInTiles.Value.Height * Program.MaxTileSize.Height);
                        if (image.Size != rightSize)
                        {
                            throw new WrongSizeException(image.Size, rightSize, string.Format("Image '{0}' should be {1}x{2} but is in reality {3}x{4}",
                                file.FullName, rightSize.Width, rightSize.Height, image.Width, image.Height));
                        }
                        Point pointInPixels = new Point(originalFilePointInTiles.Value.X * Program.MaxTileSize.Width, originalFilePointInTiles.Value.Y * Program.MaxTileSize.Height);
                        CropAndDrawImageToTileSet(image, pointInPixels, Program.MaxTileSize, file, flipHorizontal, flipVertical);
                        StoreTileFile(file);
                    }
                    else
                    {
                        if (image.Size == Program.ItemSize)
                        {
                            var baseTileData = GetTileFile(originalTileNumber);
                            var floorTileData = baseTileData.FloorTileData;
                            FloorTileData floorTileDataReplacement = floorTileData != null ? new FloorTileData(fileFloor, floorTileData.HasTileFile, floorTileData.SubType, floorTileData.NameOrDesc) : null;

                            using (var floorImage = GetFloorTile(fileFloor, floorTileData, animation, frame))
                            {
                                DrawItemToTileSet(image, false, mainTileAlignment, floorImage);
                                StoreTileFile(file, floorTileDataReplacement);
                            }
                        }
                        else if (image.Size == Program.MaxTileSize)
                        {
                            DrawImageToTileSet(image);
                            StoreTileFile(file);
                        }
                        else
                        {
                            DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                            StoreTileFile(file);
                        }
                    }                    
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

        private Bitmap GetFloorTile(FileInfo fileFloor, FloorTileData floorTileData, string animation, string frame)
        {
            if (floorTileData != null && floorTileData.HasTileFile)
            {
                if (fileFloor.Exists)
                {
                    return new Bitmap(Image.FromFile(fileFloor.FullName));
                }
                else
                {
                    return MissingAnimationFloorCreator.CreateTileWithTextLines(_missingFloorTileType, animation, frame);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
