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
    class CmapCompiler : BitmapCompiler
    {
        const string _subDirName = "Cmap";
        const int _lineLength = 3;
        const float _darknessOpacity = 0.60f;
        const string _missingCmapType = "Cmap";
        const string _noDescription = "no description";

        private Dictionary<string, CmapDarknessCreatorData> _autogeneratedDarkTiles = new Dictionary<string, CmapDarknessCreatorData>()
        {
            { "darkroom", new CmapDarknessCreatorData("room", _darknessOpacity) }
        };

        public DarknessCreator DarknessCreator { get; private set; }
        protected MissingTileCreator MissingCmapTileCreator { get; set; }

        public CmapCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            DarknessCreator = new DarknessCreator(_missingCmapType);
            MissingCmapTileCreator = new MissingTileCreator();
            MissingCmapTileCreator.BackgroundColor = Color.LightGray;
            MissingCmapTileCreator.TextColor = Color.DarkGreen;
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

            if(desc == _noDescription)
            {
                desc = "";
            }

            var subDir2 = map.ToLower().Replace(" ", "_");
            var name2 = name.Substring(2);

            var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
            var fileName = map.ToLower().Replace(" ", "_") + "_" + name2.ToLower().Replace(" ", "_") + Program.ImageFileExtension;
            var relativePath = Path.Combine(_subDirName, subDir2, fileName);
            var cmapName = name.Substring(2).ToLower().Replace(" ", "_");
            var filePath = Path.Combine(dirPath, fileName);
            FileInfo file = new FileInfo(filePath);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine("Cmap directory '{0}' not found. Creating Missing Cmap icon.", dirPath);
                WriteCmapTileNameErrorDirectoryNotFound(relativePath, desc, "Creating Missing  Cmap icon.");

                using (var image = MissingCmapTileCreator.CreateTile(_missingCmapType, null, name))
                {
                    DrawImageToTileSet(image);
                    IncreaseCurXY();
                }
            }
            else
            {
                bool isTileMissing = false;
                if (_autogeneratedDarkTiles.ContainsKey(cmapName))
                {
                    var info = _autogeneratedDarkTiles[cmapName];
                    var sourceCmapName = info.OriginalCmapName.ToLower().Replace(" ", "_");
                    var sourceFileName = map.ToLower().Replace(" ", "_") + "_" + sourceCmapName + Program.ImageFileExtension;
                    var sourceFilePath = Path.Combine(dirPath, sourceFileName);
                    var sourceFile = new FileInfo(sourceFilePath);

                    bool isUnknown;
                    using (var image = DarknessCreator.CreateDarkBitmapFromFile(sourceFile, name, info.Opacity, out isUnknown))
                    {
                        if (!isUnknown)
                        {
                            WriteCmapTileNameAutogenerationSuccess(sourceFilePath, relativePath, "cmap", desc);
                        }
                        else
                        {
                            Console.WriteLine("Cmap file '{0}' not found for darkness creation. Creating Missing Cmap Tile.", sourceFile.FullName);
                            WriteCmapTileNameAutogenerationError(sourceFilePath, relativePath, "cmap", desc);
                        }

                        DrawImageToTileSet(image);
                        IncreaseCurXY();
                    }
                }
                else
                {
                    if (file.Exists)
                    {
                        WriteCmapTileNameSuccess(relativePath, desc);
                    }
                    else
                    {
                        Console.WriteLine("File '{0}' not found. Creating Missing Cmap tile.", file.FullName);
                        isTileMissing = true;
                        WriteCmapTileNameErrorFileNotFound(relativePath, "Creating Missing Cmap tile.", desc);
                    }                    

                    if(!isTileMissing)
                    {
                        using (var image = new Bitmap(Image.FromFile(file.FullName)))
                        {
                            DrawImageToTileSet(image);
                        }
                    }
                    else
                    {
                        using (var image = MissingCmapTileCreator.CreateTile(_missingCmapType, null, name2))
                        {
                            DrawImageToTileSet(image);
                        }
                    }
                    IncreaseCurXY();
                }
            }
            
        }

        private void WriteCmapTileNameSuccess(string relativePath, string description)
        {
            WriteTileNameLine(relativePath, "OK", description.ToProperCaseFirst());
            Program.FoundTileNumber++;
        }

        private void WriteCmapTileNameErrorFileNotFound(string relativePath, string description, string infoText)
        {
            WriteTileNameLine(relativePath, "File not found", GetDescAndInfo(description, "Info:", infoText));
            Program.MissingTileNumber++;
        }


        private void WriteCmapTileNameErrorDirectoryNotFound(string relativePath, string description, string infoText)
        {
            WriteTileNameLine(relativePath, "Directory not found", GetDescAndInfo(description, "Info:", infoText));
            Program.MissingTileNumber++;
        }

        protected void WriteCmapTileNameAutogenerationSuccess(string relativePathSource, string relativePathDest, string type, string description)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE SUCCESS: " + type, GetDescAndInfo(description, "Source:", relativePathSource));
            Program.AutoGeneratedTileNumber++;
        }

        protected void WriteCmapTileNameAutogenerationError(string relativePathSource, string relativePathDest, string type, string description)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE ERROR: " + type, GetDescAndInfo(description, "Source file not found:", relativePathSource));
            Program.AutoGeneratedMissingTileNumber++;
        }

        private string GetDescAndInfo(string description, string infoHeader, string infoText)
        {
            StringBuilder sb = new StringBuilder();
            if(!string.IsNullOrWhiteSpace(description))
            {
                sb.Append("Description: ").Append(description);
            }
            if(!string.IsNullOrWhiteSpace(infoText))
            {
                if(sb.Length > 0)
                {
                    sb.Append(", ");
                }
                sb.Append(infoHeader).Append(" ").Append(infoText);
            }
            return sb.ToString();
        }

    }
}
