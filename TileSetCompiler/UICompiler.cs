﻿using System;
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
        const string _typeSpecialEffect = "special-effect";
        const string _typeSimpleDoodad = "simple-doodad";
        const string _typeRotatableDoodad = "rotatable-doodad";
        const string _typeHitTile = "hit-tile";
        const string _typeGeneralTile = "general-tile";
        const string _typeSpellTile = "spell-tile";
        const string _typeSkillTile = "skill-tile";
        const string _typeCommandTile = "command-tile";
        const string _typeUITile = "ui-tile";
        const string _typeBuff = "buff";
        const int _uiTileSplitItemsBeforeNames = 6;

        public MissingTileCreator MissingUITileCreator { get; set; }
        public MissingSubTileCreator MissingUISubTileCreator { get; set; }
        public GrayScaleCreator GrayScaleCreator { get; set; }

        public UICompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingUITileCreator = new MissingTileCreator();
            MissingUITileCreator.TextColor = Color.Yellow;
            MissingUITileCreator.BackgroundColor = Color.Gray;

            MissingUISubTileCreator = new MissingSubTileCreator();
            MissingUISubTileCreator.TextColor = Color.Yellow;
            MissingUISubTileCreator.BackgroundColor = Color.Gray;

            GrayScaleCreator = new GrayScaleCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _minLineLength)
            {
                throw new Exception(string.Format("User Interface line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];

            if(type == _typeCursor || type == _typeSpecialEffect || type == _typeSimpleDoodad || type == _typeRotatableDoodad || type == _typeHitTile || type == _typeGeneralTile)
            {
                var name = splitLine[2];
                var dirPath = Path.Combine(BaseDirectory.FullName, type.ToFileName());
                var fileName = type.ToFileName() + "_" + name.ToFileName() + Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, type.ToFileName(), fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                var widthInTiles = int.Parse(splitLine[3]);
                var heightInTiles = int.Parse(splitLine[4]);
                var mainTileAligntmentInt = int.Parse(splitLine[5]);
                MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAligntmentInt;

                if (file.Exists)
                {
                    //using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    //{
                    //    DrawImageToTileSet(image);
                    //    StoreTileFile(file, image.Size);
                    //}
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        if (image.Width != widthInTiles * Program.MaxTileSize.Width || image.Height != heightInTiles * Program.MaxTileSize.Height)
                        {
                            throw new WrongSizeException(image.Size, new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height),
                                string.Format("Monster Tile '{0}' is wrong size ({1}x{2}). It should be {3}x{4}.", file.FullName,
                                image.Width, image.Height, widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height));
                        }
                        DrawMainTileToTileSet(image, widthInTiles, heightInTiles, mainTileAlignment, file);
                        StoreTileFile(file, image.Size);
                    }

                    Console.WriteLine("Compiled UI Tile '{0}' successfully.", relativePath);
                    WriteTileNameSuccess(relativePath);
                }
                else
                {
                    using (var image = MissingUITileCreator.CreateTile(_missingUIType, type, name))
                    {
                        DrawImageToTileSet(image);
                    }

                    Console.WriteLine("File '{0}' not found. Creating Missing UI Tile.", file.FullName);
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing UI Tile.");
                }
                
                IncreaseCurXY();
            }
            else if (type == _typeUITile || type == _typeBuff || type == _typeSpellTile || type == _typeSkillTile || type == _typeCommandTile)
            {
                if(splitLine.Length < 4)
                {
                    throw new Exception(string.Format("UI Tile line has too few elements: {0}", string.Join(',', splitLine)));
                }
                
                var tileName = splitLine[2];
                var tileNameSingular = type == _typeSpellTile || type == _typeCommandTile ? tileName : GetSingular(tileName);

                int numSubTiles = int.Parse(splitLine[3]);

                if (splitLine.Length < numSubTiles + _uiTileSplitItemsBeforeNames)
                {
                    throw new Exception(string.Format("UI Tile line has too few elements ({0}, {1} required): {2}", splitLine.Length, numSubTiles + _uiTileSplitItemsBeforeNames, string.Join(',', splitLine)));
                }

                int subTileWidth = int.Parse(splitLine[4]);
                int subTileHeight = int.Parse(splitLine[5]);
                Size subTileSize = new Size(subTileWidth, subTileHeight);

                var dirPath = Path.Combine(BaseDirectory.FullName, type.ToFileName(), tileName.ToFileName());
                var missingDirPath = Path.Combine(BaseDirectory.FullName, type.ToFileName());

                using (Bitmap tileBitmap = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height))
                {
                    for (int i = 0; i < numSubTiles; i++)
                    {
                        int i2 = i + _uiTileSplitItemsBeforeNames;
                        var subTileName = splitLine[i2];
                        var fileName = tileNameSingular.ToFileName() + "_" + subTileName.ToFileName() + Program.ImageFileExtension;
                        var relativePath = Path.Combine(_subDirName, type.ToFileName(), tileName.ToFileName(), fileName);
                        var filePath = Path.Combine(dirPath, fileName);
                        FileInfo file = new FileInfo(filePath);
                        string tileCalled = numSubTiles > 1 ? "Sub-Tile" : "Tile";

                        var missingFileName = "missing-tile".ToFileName() + "_" + subTileName.ToFileName() + Program.ImageFileExtension;
                        var missingRelativePath = Path.Combine(_subDirName, type.ToFileName(), missingFileName);
                        var missingFilePath = Path.Combine(missingDirPath, missingFileName);
                        FileInfo missingFile = new FileInfo(missingFilePath);

                        if (file.Exists || missingFile.Exists)
                        {
                            FileInfo usedFile = file.Exists ? file : missingFile;
                            string usedRelativePath = file.Exists ? relativePath : missingRelativePath;
                            string fullName = usedFile.FullName;
                            using (var subTileBitmap = new Bitmap(Image.FromFile(fullName)))
                            {
                                if (subTileBitmap.Size != subTileSize)
                                {
                                    throw new WrongSizeException(subTileBitmap.Size, subTileSize, string.Format("Image File '{0}' is of wrong size ({1}x{2}, when the right is {3}x{4}).",
                                        fullName, subTileBitmap.Width, subTileBitmap.Height, subTileSize.Width, subTileSize.Height));
                                }

                                DrawSubTile(tileBitmap, subTileSize, i, subTileBitmap);
                                StoreTileFile(i, usedFile, usedRelativePath, subTileBitmap.Size);
                            }

                            Console.WriteLine("Compiled UI {0} '{1}' successfully.", tileCalled, usedRelativePath);
                            WriteSubTileNameSuccess(i, numSubTiles, usedRelativePath);
                        }
                        else
                        {
                            Data.TileData activeTileData = null;
                            if((type == _typeSpellTile || type == _typeSkillTile || type == _typeCommandTile) && i > 0 && subTileName == "inactive" 
                                && (activeTileData = GetTileFile(Program.CurrentCount, 0)) != null)
                            {
                                using (var subTileBitmap = new Bitmap(Image.FromFile(activeTileData.File.FullName)))
                                {
                                    if (subTileBitmap.Size != subTileSize)
                                    {
                                        throw new WrongSizeException(subTileBitmap.Size, subTileSize, string.Format("Image File '{0}' is of wrong size ({1}x{2}, when the right is {3}x{4}).",
                                            activeTileData.File.FullName, subTileBitmap.Width, subTileBitmap.Height, subTileSize.Width, subTileSize.Height));
                                    }
                                    var grayScaleBitmap = GrayScaleCreator.CreateGrayScaleBitmap(subTileBitmap);
                                    DrawSubTile(tileBitmap, subTileSize, i, grayScaleBitmap);
                                }

                                Console.WriteLine("Autogenerated UI {0} '{1}' successfully.", tileCalled, relativePath);
                                WriteSubTileNameAutogenerationSuccess(i, numSubTiles, activeTileData.RelativePath, relativePath, "inactive tile");
                            }
                            else
                            {
                                Console.WriteLine("File '{0}' not found. Creating Missing UI {1}.", file.FullName, tileCalled);
                                WriteSubTileNameErrorFileNotFound(i, numSubTiles, relativePath,
                                    string.Format("Creating Missing UI {0}.", tileCalled));

                                if (subTileSize == Program.MaxTileSize)
                                {
                                    using (var subTileBitmap = MissingUITileCreator.CreateTile(_missingUIType, type.ToProperCase() + Environment.NewLine + tileName.ToProperCase(), subTileName))
                                    {
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
    }
}
