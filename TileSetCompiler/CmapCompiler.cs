﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Creators.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class CmapCompiler : DungeonTileCompiler
    {
        const string _subDirName = "Cmap";
        const int _lineLength = 7;
        const string _noDescription = "no description";

        private Dictionary<string, CmapDarknessCreatorData> _autogeneratedDarkTiles = new Dictionary<string, CmapDarknessCreatorData>()
        {
            { "darkroom", new CmapDarknessCreatorData("room", _darknessOpacity) },
            { "corr", new CmapDarknessCreatorData("litcorr", _darknessOpacity) },
            { "darkgrass", new CmapDarknessCreatorData("grass", _darknessOpacity) }
        };

        protected MissingTileCreator MissingCmapTileCreator { get; set; }

        public CmapCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingCmapTileCreator = new MissingTileCreator();
            MissingCmapTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapTileCreator.TextColor = Color.DarkGreen;
            MissingCmapTileCreator.Capitalize = false;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Cmap line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var map = splitLine[1];
            var name = splitLine[2];
            var desc = splitLine[3];

            if (desc == _noDescription)
            {
                desc = "";
            }

            int widthInTiles = int.Parse(splitLine[4]);
            int heightInTiles = int.Parse(splitLine[5]);
            MainTileAlignment mainTileAlignment = GetMainTileAlignment(splitLine[6]);

            var subDir2 = map.ToLower().Replace(" ", "_");
            var name2 = name.Substring(2);

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            var fileName = map.ToFileName() + "_" + name2.ToFileName() + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var cmapName = name.Substring(2).ToFileName();
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (file.Exists)
            {
                WriteCmapTileNameSuccess(relativePath, desc);
                using (var image = new Bitmap(Image.FromFile(file.FullName)))
                {
                    if (image.Size == Program.MaxTileSize)
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
            else if (_autogeneratedDarkTiles.ContainsKey(cmapName))
            {
                var info = _autogeneratedDarkTiles[cmapName];
                var sourceCmapName = info.OriginalCmapName.ToLower().Replace(" ", "_");
                var sourceFileName = map.ToLower().Replace(" ", "_") + "_" + sourceCmapName + Program.ImageFileExtension;
                var sourceFilePath = Path.Combine(dirPath, sourceFileName);
                var sourceFile = new FileInfo(sourceFilePath);

                bool isTileMissing;
                using (var image = DarknessCreator.CreateDarkBitmapFromFile(sourceFile, name, info.Opacity, out isTileMissing))
                {
                    if (!isTileMissing)
                    {
                        WriteCmapTileNameAutogenerationSuccess(sourceFilePath, relativePath, "cmap", desc);
                    }
                    else
                    {
                        Console.WriteLine("Cmap file '{0}' not found for darkness creation. Creating Missing Cmap Tile.", sourceFile.FullName);
                        WriteCmapTileNameAutogenerationError(sourceFilePath, relativePath, "cmap", desc);
                    }

                    DrawImageToTileSet(image);
                    StoreTileFile(sourceFile);
                }
            }
            else
            {
                Console.WriteLine("File '{0}' not found. Creating Missing Cmap tile.", file.FullName);
                WriteCmapTileNameErrorFileNotFound(relativePath, desc, "Creating Missing Cmap tile.");

                using (var image = MissingCmapTileCreator.CreateTile(_missingCmapType, null, name2))
                {
                    DrawImageToTileSet(image);
                }                
            }
            IncreaseCurXY();
        }
    }
}
