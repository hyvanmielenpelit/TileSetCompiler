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
            var relativePath2 = Path.Combine(_subDirName, animation.ToFileName(), fileName2);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);
            var filePath2 = Path.Combine(dirPath, fileName2);
            FileInfo file2 = new FileInfo(filePath2);
            var filePathFloor = Path.Combine(dirPath, fileNameFloor);
            FileInfo fileFloor = new FileInfo(filePathFloor);
            var filePathFloor2 = Path.Combine(dirPath, fileNameFloor2);
            FileInfo fileFloor2 = new FileInfo(filePathFloor2);

            var originalFileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalTileData.File.Name).ToFileName();
            var templateDir = Path.Combine(BaseDirectory.FullName, originalFileNameWithoutExtension);
            var templateName = originalFileNameWithoutExtension + "_" + frame.ToFileName() + Program.ImageFileExtension;
            var templateName2 = frame.ToFileName() + Program.ImageFileExtension;

            var templateRelativePath = Path.Combine(_subDirName, originalFileNameWithoutExtension, templateName);
            var templateRelativePath2 = Path.Combine(_subDirName, originalFileNameWithoutExtension, templateName2);
            var templatePath = Path.Combine(templateDir, templateName);
            FileInfo template = new FileInfo(templatePath);
            var templatePath2 = Path.Combine(templateDir, templateName2);
            FileInfo template2 = new FileInfo(templatePath2);

            if (file.Exists || file2.Exists)
            {
                if(!file.Exists && file2.Exists)
                {
                    file = file2;
                    fileFloor = fileFloor2;
                    relativePath = relativePath2;
                }

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
                        StoreTileFile(file, image.Size);
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
                                StoreTileFile(file, image.Size, floorTileDataReplacement);
                            }
                        }
                        else if (image.Size == Program.MaxTileSize)
                        {
                            DrawImageToTileSet(image);
                            StoreTileFile(file, image.Size);
                        }
                        else
                        {
                            DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                            StoreTileFile(file, image.Size);
                        }
                    }                    
                }

                Console.WriteLine("Compiled Animation '{0}' successfully.", relativePath);
                WriteTileNameSuccess(relativePath);
            }
            else if(template.Exists || template2.Exists)
            {
                if (!template.Exists && template2.Exists)
                {
                    template = template2;
                    templateRelativePath = templateRelativePath2;
                }

                var templateData = originalTileData.TemplateData;

                if(templateData == null)
                {
                    throw new Exception(string.Format("TemplateData for Tile {0} is null.", originalTileNumber));
                }

                using (var image = CreateBitmapFromTemplate(template, templateData.TemplateColor, originalTileData.BitmapSize))
                {
                    if (originalFileBitmapSizeInTiles.HasValue && (originalFileBitmapSizeInTiles.Value.Width > 1 || originalFileBitmapSizeInTiles.Value.Height > 1))
                    {
                        Size rightSize = new Size(originalFileBitmapSizeInTiles.Value.Width * Program.MaxTileSize.Width, originalFileBitmapSizeInTiles.Value.Height * Program.MaxTileSize.Height);
                        if (image.Size != rightSize)
                        {
                            throw new WrongSizeException(image.Size, rightSize, string.Format("Image '{0}' should be {1}x{2} but is in reality {3}x{4}",
                                template.FullName, rightSize.Width, rightSize.Height, image.Width, image.Height));
                        }
                        Point pointInPixels = new Point(originalFilePointInTiles.Value.X * Program.MaxTileSize.Width, originalFilePointInTiles.Value.Y * Program.MaxTileSize.Height);
                        CropAndDrawImageToTileSet(image, pointInPixels, Program.MaxTileSize, file, flipHorizontal, flipVertical);
                        StoreTileFile(template, image.Size);
                    }
                    else
                    {
                        if (image.Size == Program.ItemSize)
                        {
                            var floorTileData = originalTileData.FloorTileData;

                            FileInfo templateFloor = null;
                            FileInfo templateFloor2 = null;
                            if (floorTileData != null && floorTileData.FloorFile != null)
                            {
                                var floorFileWithoutExtension = Path.GetFileNameWithoutExtension(floorTileData.FloorFile.Name).ToFileName();
                                var templateDirFloor = Path.Combine(BaseDirectory.FullName, floorFileWithoutExtension);
                                var templateNameFloor = floorFileWithoutExtension + "_" + frame.ToFileName() + Program.ImageFileExtension;
                                var templateNameFloor2 = frame.ToFileName() + Program.ImageFileExtension;
                                var templatePathFloor = Path.Combine(templateDir, templateNameFloor);
                                templateFloor = new FileInfo(templatePathFloor);
                                var templatePathFloor2 = Path.Combine(templateDir, templateNameFloor2);
                                templateFloor2 = new FileInfo(templatePathFloor2);

                                if (!templateFloor.Exists && templateFloor2.Exists)
                                {
                                    templateFloor = templateFloor2;
                                    templateRelativePath = templateRelativePath2;
                                }
                            }

                            FloorTileData floorTileDataReplacement = floorTileData != null ? new FloorTileData(fileFloor, floorTileData.HasTileFile, floorTileData.SubType, floorTileData.NameOrDesc) : null;

                            using (var floorImage = GetFloorTileFromTemplate(templateFloor, templateData, floorTileData))
                            {
                                DrawItemToTileSet(image, false, mainTileAlignment, floorImage);
                                StoreTileFile(file, image.Size, floorTileDataReplacement);
                            }
                        }
                        else if (image.Size == Program.MaxTileSize)
                        {
                            DrawImageToTileSet(image);
                            StoreTileFile(file, image.Size);
                        }
                        else
                        {
                            DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                            StoreTileFile(file, image.Size);
                        }
                    }
                }

                Console.WriteLine("Created Animation {0} from Template {1} successfully.", relativePath, templateRelativePath);
                WriteTileNameTemplateGenerationSuccess(relativePath, templateRelativePath);
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

        private Bitmap GetFloorTileFromTemplate(FileInfo templateFileFloor, TemplateData templateData, FloorTileData floorTileData)
        {
            if (templateFileFloor != null && templateFileFloor.Exists)
            {
                return CreateItemFromTemplate(templateFileFloor, templateData.TemplateColor, templateData.SubTypeCode, templateData.SubTypeName);
            }
            else if (floorTileData.HasTileFile)
            {
                return MissingAnimationFloorCreator.CreateTileWithTextLines(_missingFloorTileType, floorTileData.SubType, floorTileData.NameOrDesc.ToProperCaseFirst());
            }
            else
            {
                return null;
            }
        }

    }
}
