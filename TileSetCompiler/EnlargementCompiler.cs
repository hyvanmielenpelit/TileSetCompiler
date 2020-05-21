﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Exceptions;

namespace TileSetCompiler
{
    public enum MainTileAlignment
    {
        Left = 0,
        Right = 1
    }

    public enum EnlargementTilePosition
    {
        TopLeft = 0,
        TopCenter = 1,
        TopRight = 2,
        MiddleLeft = 3,
        MiddleRight = 4
    }

    class EnlargementCompiler : BitmapCompiler
    {
        const string _subDirName = "Enlargement";
        const int _lineLength = 9;
        const string _missingAnimationType = "Enlargement";

        public MissingTileCreator MissingEnlargementCreator { get; set; }

        public EnlargementCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingEnlargementCreator = new MissingTileCreator();
            MissingEnlargementCreator.BackgroundColor = Color.LightGray;
            MissingEnlargementCreator.TextColor = Color.Brown;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Animation line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var enlargementName = splitLine[1];
            var tilePositionName = splitLine[2];
            var originalTileIndex = int.Parse(splitLine[3]);

            var enlargementWidthInTiles = int.Parse(splitLine[4]);
            if(enlargementWidthInTiles < 1 || enlargementWidthInTiles > 3)
            {
                throw new Exception(string.Format("enlargementWidthInTiles must be between 1 and 3. Value: {0}", enlargementWidthInTiles));
            }

            var enlargementHeightInTiles = int.Parse(splitLine[5]);
            if (enlargementHeightInTiles < 1 || enlargementHeightInTiles > 2)
            {
                throw new Exception(string.Format("enlargementHeightInTiles must be between 1 and 2. Value: {0}", enlargementHeightInTiles));
            }

            var mainTileAlignmentInt = int.Parse(splitLine[6]);
            if(!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new IndexOutOfRangeException(string.Format("mainTileAlignmentInt '{0}' is out of range.", mainTileAlignmentInt));
            }
            var mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;
            var enlargementTileCount = int.Parse(splitLine[7]);

            if(enlargementTileCount != (enlargementWidthInTiles * enlargementHeightInTiles - 1))
            {
                throw new Exception(string.Format("enlargementTileCount is not equal to enlargementWidthInTiles x enlargementHeightInTiles - 1 = {0}. Value: {1}", 
                    enlargementWidthInTiles * enlargementHeightInTiles - 1, enlargementTileCount));
            }

            var tilePositionIndex = int.Parse(splitLine[8]);
            if (!Enum.IsDefined(typeof(EnlargementTilePosition), tilePositionIndex))
            {
                throw new IndexOutOfRangeException(string.Format("tilePositionIndex '{0}' is out of range.", tilePositionIndex));
            }
            var tilePosition = (EnlargementTilePosition)tilePositionIndex;

            if (Program.TileFiles.ContainsKey(originalTileIndex))
            {
                //The original tile exists
                var originalTileFile = Program.TileFiles[originalTileIndex];
                using (var originalImage = new Bitmap(Image.FromFile(originalTileFile.FullName)))
                {
                    int xTile = 0;
                    int yTile = 0;
                    Size originalImageTileSize = new Size(originalImage.Width / Program.MaxTileSize.Width, originalImage.Height / Program.MaxTileSize.Height);

                    if (originalImageTileSize.Height == 1 && (tilePosition == EnlargementTilePosition.TopLeft || tilePosition == EnlargementTilePosition.TopCenter || tilePosition == EnlargementTilePosition.TopRight))
                    {
                        throw new WrongSizeException(string.Format("Image '{0}' is too small in height for enlargement. Size: {1}x{2}. Minimum Height: {3}.",
                            originalTileFile.FullName, originalImage.Width, originalImage.Height, 2 * Program.MaxTileSize.Height));
                    }
                    if (originalImageTileSize.Width == 1 && (tilePosition == EnlargementTilePosition.TopLeft || tilePosition == EnlargementTilePosition.TopRight || tilePosition == EnlargementTilePosition.MiddleRight))
                    {
                        throw new WrongSizeException(string.Format("Image '{0}' is too small in width for enlargement. Size: {1}x{2}. Minimum Height: {3}.",
                            originalTileFile.FullName, originalImage.Width, originalImage.Height, 2 * Program.MaxTileSize.Height));
                    }
                    if (originalImageTileSize.Width == 2 && mainTileAlignment == MainTileAlignment.Left && (tilePosition == EnlargementTilePosition.TopLeft || tilePosition == EnlargementTilePosition.MiddleLeft))
                    {
                        throw new WrongSizeException(string.Format("Image '{0}' is too small in left width for enlargement. Size: {1}x{2}. Minimum Height: {3}.",
                            originalTileFile.FullName, originalImage.Width, originalImage.Height, 3 * Program.MaxTileSize.Height));
                    }
                    if (originalImageTileSize.Width == 2 && mainTileAlignment == MainTileAlignment.Right && (tilePosition == EnlargementTilePosition.TopRight || tilePosition == EnlargementTilePosition.MiddleRight))
                    {
                        throw new WrongSizeException(string.Format("Image '{0}' is too small in right width for enlargement. Size: {1}x{2}. Minimum Height: {3}.",
                            originalTileFile.FullName, originalImage.Width, originalImage.Height, 3 * Program.MaxTileSize.Height));
                    }

                    if (tilePosition == EnlargementTilePosition.TopLeft)
                    {
                        xTile = 0;
                        yTile = 0;
                    }
                    else if (tilePosition == EnlargementTilePosition.TopCenter)
                    {
                        yTile = 0;
                        if (enlargementWidthInTiles == 1)
                        {
                            xTile = 0;
                        }
                        else if (enlargementWidthInTiles == 2)
                        {
                            if (mainTileAlignment == MainTileAlignment.Left)
                            {
                                xTile = 0;
                            }
                            else
                            {
                                xTile = 1;
                            }
                        }
                        else
                        {
                            xTile = 1;
                        }
                    }
                    else if (tilePosition == EnlargementTilePosition.TopRight)
                    {
                        yTile = 0;
                        if (enlargementWidthInTiles == 2)
                        {
                            xTile = 1;
                        }
                        else if (enlargementWidthInTiles == 3)
                        {
                            xTile = 2;
                        }
                    }
                    else if (tilePosition == EnlargementTilePosition.MiddleLeft)
                    {
                        if(enlargementHeightInTiles == 1)
                        {
                            yTile = 0;
                        }
                        else
                        {
                            yTile = 1;
                        }
                        xTile = 0;
                    }
                    else if (tilePosition == EnlargementTilePosition.MiddleRight)
                    {
                        if (enlargementHeightInTiles == 1)
                        {
                            yTile = 0;
                        }
                        else
                        {
                            yTile = 1;
                        }
                        if (enlargementWidthInTiles == 2)
                        {
                            xTile = 1;
                        }
                        else if (enlargementWidthInTiles == 3)
                        {
                            xTile = 2;
                        }
                    }

                    int x = xTile * Program.MaxTileSize.Width;
                    int y = yTile * Program.MaxTileSize.Height;

                    CropAndDrawImageToTileSet(originalImage, new Point(x, y), Program.MaxTileSize);
                    IncreaseCurXY();
                }
            }
            else
            {
                //The original tile is missing
                using (var image = MissingEnlargementCreator.CreateTileWithTextLines(_missingAnimationType, enlargementName, tilePositionName))
                {
                    DrawImageToTileSet(image);
                }
                IncreaseCurXY();
            }
        }
    }
}
