using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Exceptions;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class MiscCompiler : BitmapCompiler
    {
        const string _subDirName = "Misc";
        const string _miscInvisible = "invisible";
        const string _miscExplode = "explode";
        const string _miscZap = "zap";
        const string _miscSwallow = "swallow";
        const string _miscWarning = "warning";
        const string _miscWorm = "worm";
        const string _missingTileType = "Misc";

        const int _zapLength = 14;
        const int _wormLength = 8;
        const int _explodeLength = 8;
        const int _invisibleLength = 3;
        const int _swallowLength = 8;
        const int _warningLength = 3;

        const string _breathSuffix = "-breath";

        public MissingTileCreator MissingMiscTileCreator { get; set; }

        public MiscCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingMiscTileCreator = new MissingTileCreator();
            MissingMiscTileCreator.TextColor = Color.Black;
            MissingMiscTileCreator.BackgroundColor = Color.Azure;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < 2)
            {
                throw new Exception(string.Format("Misc line '{0}' has less than 2 elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];

            string subDir2 = null;
            string fileName = null;
            string name = "";
            Point? pointInTiles = null;
            Size? bitmapSizeInTiles = null;

            string templateSubDir = null;
            string templateFileName = null;
            Color templatecolor = Color.Empty;
            Size templateSize = Size.Empty;

            bool flipHorizontal = false;
            bool flipVertical = false;

            if (type == _miscInvisible)
            {
                if (splitLine.Length < _invisibleLength)
                {
                    throw new Exception(string.Format("Misc Invisible line '{0}' has less than {1} elements.", string.Join(',', splitLine), _invisibleLength));
                }

                var type2 = splitLine[2];
                subDir2 = type.ToFileName();
                fileName = type.ToFileName() + "_" + type2.ToFileName() + Program.ImageFileExtension;
                name = type2;
            }
            else if (type == _miscExplode)
            {
                if (splitLine.Length < _explodeLength)
                {
                    throw new Exception(string.Format("Misc Explode line '{0}' has less than {1} elements.", string.Join(',', splitLine), _explodeLength));
                }

                var type2 = splitLine[2];
                var direction = splitLine[3];
                int xInTiles = int.Parse(splitLine[4]);
                int yInTiles = int.Parse(splitLine[5]);
                pointInTiles = new Point(xInTiles, yInTiles);
                int widthInTiles = int.Parse(splitLine[6]);
                int heightInTiles = int.Parse(splitLine[7]);
                bitmapSizeInTiles = new Size(widthInTiles, heightInTiles);

                if (widthInTiles > 1 || heightInTiles > 1)
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }
                else
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + "_" + direction.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }
            }
            else if (type == _miscZap)
            {
                if (splitLine.Length < _zapLength)
                {
                    throw new Exception(string.Format("Misc Zap line '{0}' has less than {1} elements.", string.Join(',', splitLine), _zapLength));
                }

                var type2 = splitLine[2];
                var direction = splitLine[3];
                int xInTiles = int.Parse(splitLine[4]);
                int yInTiles = int.Parse(splitLine[5]);
                pointInTiles = new Point(xInTiles, yInTiles);
                int widthInTiles = int.Parse(splitLine[6]);
                int heightInTiles = int.Parse(splitLine[7]);
                bitmapSizeInTiles = new Size(widthInTiles, heightInTiles);
                int targetBitmapWidthInTiles = int.Parse(splitLine[8]); //Not used
                int targetBitmapHeightInTiles = int.Parse(splitLine[9]); //Not used
                MainTileAlignment targetBitmapMainTileAlignment = GetMainTileAlignment(splitLine[10]); //Not used
                int flipHorizontalInt = int.Parse(splitLine[11]);
                flipHorizontal = flipHorizontalInt > 0;
                int flipVerticalInt = int.Parse(splitLine[12]);
                flipVertical = flipVerticalInt > 0;
                int colorCode = int.Parse(splitLine[13]);
                templatecolor = GetColorFromColorCode(colorCode);

                if (widthInTiles > 1 || heightInTiles > 1)
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }
                else
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + "_" + direction.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }

                templateSubDir = type.ToFileName();
                if(type2.EndsWith(_breathSuffix))
                {
                    templateFileName = type.ToFileName() + _templateSuffix + _breathSuffix + Program.ImageFileExtension;
                }
                else
                {
                    templateFileName = type.ToFileName() + _templateSuffix + Program.ImageFileExtension;
                }

                templateSize = new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height);
            }
            else if (type == _miscSwallow)
            {
                if (splitLine.Length < _swallowLength)
                {
                    throw new Exception(string.Format("Misc Swallow line '{0}' has less than {1} elements.", string.Join(',', splitLine), _swallowLength));
                }

                var monster = splitLine[2];
                var direction = splitLine[3];
                int xInTiles = int.Parse(splitLine[4]);
                int yInTiles = int.Parse(splitLine[5]);
                pointInTiles = new Point(xInTiles, yInTiles);
                int widthInTiles = int.Parse(splitLine[6]);
                int heightInTiles = int.Parse(splitLine[7]);
                bitmapSizeInTiles = new Size(widthInTiles, heightInTiles);

                if (widthInTiles > 1 || heightInTiles > 1)
                {
                    subDir2 = Path.Combine(type.ToFileName(), monster.ToFileName());
                    fileName = monster.ToFileName() + "_" + type.ToFileName() + Program.ImageFileExtension;
                    name = monster + " " + direction;
                }
                else
                {
                    subDir2 = Path.Combine(type.ToFileName(), monster.ToFileName());
                    fileName = monster.ToFileName() + "_" + type.ToFileName() + "_" + direction.ToFileName() + Program.ImageFileExtension;
                    name = monster + " " + direction;
                }
            }
            else if (type == _miscWarning)
            {
                if (splitLine.Length < _warningLength)
                {
                    throw new Exception(string.Format("Misc Warning line '{0}' has less than {1} elements.", string.Join(',', splitLine), _warningLength));
                }

                var level = splitLine[2];
                subDir2 = type.ToFileName();
                fileName = level.ToFileName() + Program.ImageFileExtension;
                name = level;
            }
            else if (type == _miscWorm)
            {
                if (splitLine.Length < _wormLength)
                {
                    throw new Exception(string.Format("Misc Worm line '{0}' has less than {1} elements.", string.Join(',', splitLine), _wormLength));
                }

                var type2 = splitLine[2];
                var direction = splitLine[3];
                int xInTiles = int.Parse(splitLine[4]);
                int yInTiles = int.Parse(splitLine[5]);
                pointInTiles = new Point(xInTiles, yInTiles);
                int widthInTiles = int.Parse(splitLine[6]);
                int heightInTiles = int.Parse(splitLine[7]);
                bitmapSizeInTiles = new Size(widthInTiles, heightInTiles);

                if (widthInTiles > 1 || heightInTiles > 1)
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }
                else
                {
                    subDir2 = Path.Combine(type.ToFileName(), type2.ToFileName());
                    fileName = type.ToFileName() + "_" + type2.ToFileName() + "_" + direction.ToFileName() + Program.ImageFileExtension;
                    name = type2 + " " + direction;
                }
            }
            else
            {
                //Other type
                if(splitLine.Length == 3)
                {
                    name = splitLine[2];
                    subDir2 = type.ToFileName();
                    fileName = name.ToFileName() + Program.ImageFileExtension;
                }
                else if (splitLine.Length >= 4)
                {
                    var category = splitLine[2];
                    var name2 = splitLine[3];
                    subDir2 = Path.Combine(type.ToFileName(), category.ToFileName());
                    fileName = category.ToFileName() + "_" + name2.ToFileName() + Program.ImageFileExtension;
                    name = category + " " + name2;
                }
                else
                {
                    throw new Exception(string.Format("Misc line too short: {0} elements. Line is: '{1}'", splitLine.Length, string.Join(',', splitLine)));
                }
            }

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            string templateFilePath = null;
            string templateRelativePath = null;
            FileInfo templateFile = null;
            if (templateSubDir != null && templateFileName != null)
            {
                templateFilePath = Path.Combine(BaseDirectory.FullName, templateSubDir, templateFileName);
                templateRelativePath = Path.Combine(_subDirName, templateSubDir, templateFileName);
                templateFile = new FileInfo(templateFilePath);
            }

            if (file.Exists)
            {
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if(bitmapSizeInTiles.HasValue && (bitmapSizeInTiles.Value.Width > 1 || bitmapSizeInTiles.Value.Height > 1))
                    {
                        Size rightSize = new Size(bitmapSizeInTiles.Value.Width * Program.MaxTileSize.Width, bitmapSizeInTiles.Value.Height * Program.MaxTileSize.Height);
                        if (image.Size != rightSize)
                        {
                            throw new WrongSizeException(image.Size, rightSize, string.Format("Image '{0}' should be {1}x{2} but is in reality {3}x{4}",
                                file.FullName, rightSize.Width, rightSize.Height, image.Width, image.Height));
                        }
                        Point pointInPixels = new Point(pointInTiles.Value.X * Program.MaxTileSize.Width, pointInTiles.Value.Y * Program.MaxTileSize.Height);
                        CropAndDrawImageToTileSet(image, pointInPixels, Program.MaxTileSize, file, flipHorizontal, flipVertical);
                    }
                    else
                    {
                        DrawImageToTileSet(image);
                    }
                    StoreTileFile(file, image.Size, pointInTiles, bitmapSizeInTiles, flipHorizontal, flipVertical);
                }

                Console.WriteLine("Compiled Misc Tile {0} successfully.", relativePath);
                WriteTileNameSuccess(relativePath);
            }
            else if (templateFile != null && templateFile.Exists)
            {
                using (var image = CreateBitmapFromTemplate(templateFile, templatecolor, templateSize, 0, null))
                {
                    if (bitmapSizeInTiles.HasValue && (bitmapSizeInTiles.Value.Width > 1 || bitmapSizeInTiles.Value.Height > 1))
                    {
                        Size rightSize = new Size(bitmapSizeInTiles.Value.Width * Program.MaxTileSize.Width, bitmapSizeInTiles.Value.Height * Program.MaxTileSize.Height);
                        if (image.Size != rightSize)
                        {
                            throw new WrongSizeException(image.Size, rightSize, string.Format("Image '{0}' should be {1}x{2} but is in reality {3}x{4}",
                                file.FullName, rightSize.Width, rightSize.Height, image.Width, image.Height));
                        }
                        Point pointInPixels = new Point(pointInTiles.Value.X * Program.MaxTileSize.Width, pointInTiles.Value.Y * Program.MaxTileSize.Height);
                        CropAndDrawImageToTileSet(image, pointInPixels, Program.MaxTileSize, file, flipHorizontal, flipVertical);
                    }
                    else
                    {
                        DrawImageToTileSet(image);
                    }
                    StoreTileFile(file, image.Size, pointInTiles, bitmapSizeInTiles, false, true, new TemplateData(templatecolor, 0, null), null, flipHorizontal, flipVertical);
                }

                Console.WriteLine("Created Misc Tile {0} from Template {1} successfully.", relativePath, templateRelativePath);
                WriteTileNameTemplateGenerationSuccess(relativePath, templateRelativePath);
            }
            else
            {
                using (var image = MissingMiscTileCreator.CreateTile(_missingTileType, type, name))
                {
                    DrawImageToTileSet(image);
                }

                Console.WriteLine("File '{0}' not found. Creating Missing Misc tile.", file.FullName);
                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Misc tile.");
            }

            IncreaseCurXY();
        }
    }
}
