﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Creators.Data;

namespace TileSetCompiler
{
    class CmapVariationCompiler : DungeonTileCompiler
    {
        const string _subDirName = "Cmap Variations";
        const int _lineLength = 6;
        const string _cmapVariationSubType = "Variation";
        const string _cmapVariationAutogenerationType = "cmap-variation";

        private Dictionary<string, CmapDarknessCreatorData> _autogeneratedDarkTiles = new Dictionary<string, CmapDarknessCreatorData>()
        {
            { "dark-room-floor-variation", new CmapDarknessCreatorData("room-floor-variation", _darknessOpacity) }
        };

        protected MissingTileCreator MissingCmapVariationTileCreator { get; set; }

        public CmapVariationCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingCmapVariationTileCreator = new MissingTileCreator();
            MissingCmapVariationTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapVariationTileCreator.TextColor = Color.DarkGreen;
            MissingCmapVariationTileCreator.Capitalize = false;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Cmap Variation line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var map = splitLine[1];
            var name = splitLine[2];

            var nameWithoutIndex = name.ToLower().Replace(" ", "_");
            int lastDash = name.LastIndexOf('-');
            int lastPartNumber = -1;
            if (lastDash > 0 && lastDash < name.Length - 1)
            {
                string lastPart = name.Substring(lastDash + 1);
                bool isNumeral = int.TryParse(lastPart, out lastPartNumber);
                if(isNumeral)
                {
                    nameWithoutIndex = name.Substring(0, lastDash).ToLower().Replace(" ", "_");
                }
            }

            int widthInTiles = int.Parse(splitLine[3]);
            int heightInTiles = int.Parse(splitLine[4]);
            int mainTileAlignmentInt = int.Parse(splitLine[5]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;


            var subDir = map.ToLower().Replace(" ", "_");

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir);
            var fileName = map.ToLower().Replace(" ", "_") + "_" + name.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir, fileName);
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Cmap Variation directory '{0}' not found. Creating Missing Cmap Variation icon.", dirPath);
                WriteCmapTileNameErrorDirectoryNotFound(relativePath, null, "Creating Missing  Cmap Variation icon.");

                using (var image = MissingCmapVariationTileCreator.CreateTile(_missingCmapType, _cmapVariationSubType, name))
                {
                    DrawImageToTileSet(image);
                    IncreaseCurXY();
                }
            }            
            else if (_autogeneratedDarkTiles.ContainsKey(nameWithoutIndex))
            {
                //Autogenerate tile
                var info = _autogeneratedDarkTiles[nameWithoutIndex];
                var sourceCmapVariationName = info.OriginalCmapName.ToLower().Replace(" ", "_");

                StringBuilder sbSourceFileName = new StringBuilder();
                sbSourceFileName.Append(map.ToLower().Replace(" ", "_"));
                sbSourceFileName.Append("_");
                sbSourceFileName.Append(sourceCmapVariationName);
                if(lastPartNumber >= 0)
                {
                    sbSourceFileName.Append("-").Append(lastPartNumber);
                }
                sbSourceFileName.Append(Program.ImageFileExtension);

                var sourceRelativePath = Path.Combine(_subDirName, subDir, sbSourceFileName.ToString());
                var sourceFilePath = Path.Combine(dirPath, sbSourceFileName.ToString());
                var sourceFile = new FileInfo(sourceFilePath);

                Bitmap sourceBitmap = null;
                try
                {
                    if (!sourceFile.Exists)
                    {
                        Console.WriteLine("Cmap Variation file '{0}' not found for darkness creation. Creating Missing Cmap Variation Tile.", sourceFile.FullName);
                        WriteCmapTileNameAutogenerationError(sourceRelativePath, relativePath, _cmapVariationAutogenerationType, null);
                        sourceBitmap = MissingCmapVariationTileCreator.CreateTile(_missingCmapType, _cmapVariationSubType, name);
                    }
                    else
                    {
                        Console.WriteLine("Autogenerated Cmap Variation Tile '{0}'.", relativePath);
                        WriteCmapTileNameAutogenerationSuccess(sourceRelativePath, relativePath, _cmapVariationAutogenerationType, null);
                        sourceBitmap = new Bitmap(sourceFile.FullName);
                        StoreTileFile(sourceFile);
                    }
                    using (var image = DarknessCreator.CreateDarkBitmap(sourceBitmap, info.Opacity))
                    {
                        DrawImageToTileSet(image);
                        IncreaseCurXY();
                    }
                }
                finally
                {
                    if(sourceBitmap != null)
                    {
                        sourceBitmap.Dispose();
                    }
                }
            }
            else
            {
                //Read from file
                bool isTileMissing = false;

                if (file.Exists)
                {
                    WriteCmapTileNameSuccess(relativePath, null);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Cmap Variation tile.", file.FullName);
                    isTileMissing = true;
                    WriteCmapTileNameErrorFileNotFound(relativePath, null, "Creating Missing Cmap Variation tile.");
                }

                if (!isTileMissing)
                {
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
                else
                {
                    using (var image = MissingCmapVariationTileCreator.CreateTile(_missingCmapType, _cmapVariationSubType, name))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }
            
        }
    }
}
