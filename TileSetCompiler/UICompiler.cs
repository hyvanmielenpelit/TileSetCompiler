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
    class UICompiler : BitmapCompiler
    {
        const string _subDirName = "UI";
        const int _minLineLength = 3;
        const string _missingUIType = "UI";
        const string _typeCursor = "cursor";
        const string _typeUITile = "ui-tile";
        const int _uiTileSplitItemsBeforeNames = 6;

        public MissingTileCreator MissingUITileCreator { get; set; }
        public MissingSubTileCreator MissingUISubTileCreator { get; set; }

        public UICompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingUITileCreator = new MissingTileCreator();
            MissingUITileCreator.TextColor = Color.Yellow;
            MissingUITileCreator.BackgroundColor = Color.Gray;

            MissingUISubTileCreator = new MissingSubTileCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _minLineLength)
            {
                throw new Exception(string.Format("User Interface line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];

            if(type == _typeCursor)
            {
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
            else if (type == _typeUITile)
            {
                if(splitLine.Length < 4)
                {
                    throw new Exception(string.Format("UI Tile line has too few elements: {0}", string.Join(',', splitLine)));
                }
                var tileName = splitLine[2];
                int numSubTiles = int.Parse(splitLine[3]);

                if (splitLine.Length < numSubTiles + _uiTileSplitItemsBeforeNames)
                {
                    throw new Exception(string.Format("UI Tile line has too few elements ({0}, {1} required): {2}", splitLine.Length, numSubTiles + _uiTileSplitItemsBeforeNames, string.Join(',', splitLine)));
                }

                int subTileWidth = int.Parse(splitLine[4]);
                int subTileHeight = int.Parse(splitLine[5]);
                Size subTileSize = new Size(subTileWidth, subTileHeight);

                var dirPath = Path.Combine(BaseDirectory.FullName, type.ToLower().Replace(" ", "_"), tileName.ToLower().Replace(" ", "_"));

                using (Bitmap tileBitmap = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height))
                {
                    for (int i = 0; i < numSubTiles; i++)
                    {
                        int i2 = i + _uiTileSplitItemsBeforeNames;
                        var subTileName = splitLine[i2];
                        var fileName = tileName.ToLower().Replace(" ", "_") + "_" + subTileName.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
                        var relativePath = Path.Combine(_subDirName, type.ToLower().Replace(" ", "_"), tileName.ToLower().Replace(" ", "_"), fileName);
                        var filePath = Path.Combine(dirPath, fileName);
                        FileInfo file = new FileInfo(filePath);
                        bool isSubTileMissing = false;

                        if (!Directory.Exists(dirPath))
                        {
                            Console.WriteLine("User Interface directory '{0}' not found. Creating Missing UI Sub-Tile.", dirPath);
                            isSubTileMissing = true;
                            WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing UI Sub-Tile.");
                        }
                        else
                        {
                            if (file.Exists)
                            {
                                Console.WriteLine("Compiled UI Sub-Tile '{0}' successfully.", relativePath);
                                WriteTileNameSuccess(relativePath);
                            }
                            else
                            {
                                Console.WriteLine("File '{0}' not found. Creating Missing UI Sub-Tile.", file.FullName);
                                isSubTileMissing = true;
                                WriteTileNameErrorFileNotFound(relativePath, "Creating Missing UI Tile.");
                            }
                        }

                        if (!isSubTileMissing)
                        {
                            using (var subTileBitmap = new Bitmap(Image.FromFile(file.FullName)))
                            {
                                if(subTileBitmap.Size != subTileSize)
                                {
                                    throw new WrongSizeException(subTileBitmap.Size, subTileSize, string.Format("Image File '{0}' is of wrong size ({1}x{2}, when the right is {3}x{4}).",
                                        file.FullName, subTileBitmap.Width, subTileBitmap.Height, subTileSize.Width, subTileSize.Height));
                                }
                                DrawSubTile(tileBitmap, subTileSize, i, subTileBitmap);
                            }
                        }
                        else
                        {
                            using (var subTileBitmap = MissingUISubTileCreator.CreateSubTile(subTileSize, subTileName))
                            {
                                DrawSubTile(tileBitmap, subTileSize, i, subTileBitmap);
                            }
                        }
                    }

                    DrawImageToTileSet(tileBitmap);
                    IncreaseCurXY();
                }

            }
            else
            {
                throw new Exception(string.Format("Unknown UI type '{0}' in line '{1}'.", type, string.Join(',', splitLine)));
            }
        }

        protected void DrawSubTile(Bitmap tileBitmap, Size subTileSize, int index, Bitmap subTileBitmap)
        {
            int x = (subTileSize.Width * index) % tileBitmap.Width;
            int y = ((subTileSize.Width * index) / tileBitmap.Width) * subTileSize.Height;

            if(y + subTileSize.Height > tileBitmap.Height)
            {
                throw new Exception(string.Format("Error UI Sub-Tile would overflow in height: {0} > {1}.", y + subTileSize.Height, tileBitmap.Height));
            }
            else if(x + subTileBitmap.Width > tileBitmap.Width)
            {
                throw new Exception(string.Format("Error UI Sub-Tile would overflow in width: {0} > {1}.", x + subTileBitmap.Width, tileBitmap.Width));
            }

            using(Graphics gTileBitmap = Graphics.FromImage(tileBitmap))
            {
                gTileBitmap.DrawImage(subTileBitmap, new Point(x, y));
            }
        }
    }
}
