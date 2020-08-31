using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using TileSetCompiler.Creators;
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
        const string _missingTileType = "Misc";

        private Dictionary<string, int> _lineLengths = new Dictionary<string, int>()
        {
            { _miscInvisible, 3 },
            { _miscExplode, 4 },
            { _miscZap, 4 },
            { _miscSwallow, 4 },
            { _miscWarning, 3 }
        };

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
            if(!_lineLengths.ContainsKey(type))
            {
                throw new Exception(string.Format("Misc type '{0}' not valid.", type));
            }
            int lineLength = _lineLengths[type];
            if (splitLine.Length < lineLength)
            {
                throw new Exception(string.Format("Misc line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            string subDir2 = null;
            string fileName = null;
            string name = "";
            Point? pointInTiles = null;
            Size? bitmapSizeInTiles = null;

            if (type == _miscInvisible)
            {
                var type2 = splitLine[2];
                subDir2 = type.ToFileName();
                fileName = type.ToFileName() + "_" + type2.ToFileName() + Program.ImageFileExtension;
                name = type2;
            }
            else if (type == _miscExplode)
            {
                if (splitLine.Length < 8)
                {
                    throw new Exception(string.Format("Misc Explode line '{0}' has less than 8 elements.", string.Join(',', splitLine)));
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
                if (splitLine.Length < 8)
                {
                    throw new Exception(string.Format("Misc Zap line '{0}' has less than 8 elements.", string.Join(',', splitLine)));
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
            else if (type == _miscSwallow)
            {
                if (splitLine.Length < 8)
                {
                    throw new Exception(string.Format("Misc Swallow line '{0}' has less than 8 elements.", string.Join(',', splitLine)));
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
                var level = splitLine[2];
                subDir2 = type.ToFileName();
                fileName = level.ToFileName() + Program.ImageFileExtension;
                name = level;
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

            if (file.Exists)
            {
                Console.WriteLine("Compiled Misc Tile {0} successfully.", relativePath);
                WriteTileNameSuccess(relativePath);

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
                        image.Tag = file.FullName;
                        CropAndDrawImageToTileSet(image, pointInPixels, Program.MaxTileSize);
                    }
                    else
                    {
                        DrawImageToTileSet(image);
                    }
                    StoreTileFile(file, pointInTiles, bitmapSizeInTiles);
                }

            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Misc tile.", file.FullName);
                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Misc tile.");
                using (var image = MissingMiscTileCreator.CreateTile(_missingTileType, type, name))
                {
                    DrawImageToTileSet(image);
                }
            }

            IncreaseCurXY();
        }
    }
}
