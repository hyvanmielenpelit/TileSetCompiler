﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Exceptions;

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
            var originalTileNumber = int.Parse(splitLine[3]);            
            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            int mainTileAlignmentInt = int.Parse(splitLine[6]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

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
                if (!Program.TileFiles.ContainsKey(originalTileNumber))
                {
                    throw new Exception(string.Format("Original Tile ID '{0}' not found in TileFiles. Original File probably does not exist.", originalTileNumber));
                }
                using (var originalImage = new Bitmap(Image.FromFile(Program.TileFiles[originalTileNumber].FullName)))
                {
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        if(originalImage.Size != image.Size)
                        {
                            throw new WrongSizeException(image.Size, originalImage.Size,
                                string.Format("Animation tile and original tile are of different size: {0}x{1} and {2}x{3}.",
                                image.Width, image.Height, originalImage.Width, originalImage.Height));
                        }
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
                            DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment);
                        }
                        StoreTileFile(file);
                    }
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