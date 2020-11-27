﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ObjectCompiler : ItemCompiler
    {
        const string _subDirName = "Objects";
        const int _lineLength = 12;
        const string _noDescription = "no description";
        const string _missingTileType = "Object";
        const string _missingMissileType = "Missile";

        public MissingTileCreator MissingObjectTileCreator { get; set; }

        public ObjectCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingObjectTileCreator = new MissingTileCreator();
            MissingObjectTileCreator.TextColor = Color.DarkBlue;
            MissingObjectTileCreator.TileSize = MissingTileSize.Item;
        }

        public override void CompileOne(string[] splitLine)
        {
            if (splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Object line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var objectType = splitLine[2];
            var name = splitLine[3];
            var desc = splitLine[4];
            string nameOrDesc = null;
            if (string.IsNullOrWhiteSpace(desc) || desc == _noDescription)
            {
                nameOrDesc = name;
            }
            else
            {
                nameOrDesc = desc;
            }

            if (!_typeSuffix.ContainsKey(type))
            {
                throw new Exception(string.Format("Object Type '{0}' unknown. Line: '{1}'.", type, string.Join(',', splitLine)));
            }

            var objectTypeSingular = GetSingular(objectType);
            string direction = null;
            bool isFullSizeBitmap = true;

            if (type == _typeMissile)
            {
                direction = splitLine[5];
                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("Invalid direction '{0}'. Line: '{1}'.", direction, string.Join(',', splitLine)));
                }
            }
            else
            {
                isFullSizeBitmap = int.Parse(splitLine[5]) > 0;
            }

            int widthInTiles = int.Parse(splitLine[6]);
            int heightInTiles = int.Parse(splitLine[7]);
            int mainTileAlignmentInt = int.Parse(splitLine[8]);
            if(!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

            int colorCode = int.Parse(splitLine[9]);
            Color templateColor = GetColorFromColorCode(colorCode);

            int subTypeCode = int.Parse(splitLine[10]);
            string subTypeName = splitLine[11];

            if (type == _typeMissile)
            {
                //Autogenerate missile icon
                string subDir2 = Path.Combine(objectType.ToFileName(), nameOrDesc.ToFileName());
                    
                string fileNameBase = objectTypeSingular.ToFileName() + "_" +
                    nameOrDesc.ToFileName() + Program.ImageFileExtension;

                string fileNameMissile = objectTypeSingular.ToFileName() + "_" +
                    nameOrDesc.ToFileName() + _missileSuffix + Program.ImageFileExtension;

                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("_missileData does not contain direction '{0}'.", direction));
                }

                MissileDirection missileDirection = _missileData[direction].Direction;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePathBase = Path.Combine(_subDirName, subDir2, fileNameBase);
                var filePathBase = Path.Combine(dirPath, fileNameBase);
                FileInfo fileBase = new FileInfo(filePathBase);

                var relativePathMissile = Path.Combine(_subDirName, subDir2, fileNameMissile);
                var filePathMissile = Path.Combine(dirPath, fileNameMissile);
                FileInfo fileMissile = new FileInfo(filePathMissile);

                var targetSubDir2 = Path.Combine(objectType.ToFileName(), nameOrDesc.ToFileName());
                var targetFileName = objectTypeSingular.ToFileName() + "_" +
                    nameOrDesc.ToFileName() +
                    _typeSuffix[type] +
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;
                var targetRelativePath = Path.Combine(_subDirName, targetSubDir2, targetFileName);

                bool isTileMissing = false;
                FileInfo file = fileMissile.Exists ? fileMissile : fileBase;
                string relativePath = fileMissile.Exists ? relativePathMissile : relativePathBase;

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, nameOrDesc.ToProperCaseFirst(), missileDirection, out isTileMissing))
                {
                    if(!isTileMissing)
                    {
                        Console.WriteLine("Autogenerated Object Missile Tile {0} Successfully.", targetRelativePath);
                        WriteTileNameAutogenerationSuccess(relativePath, targetRelativePath, _missileAutogenerateType);
                        StoreTileFile(file);
                    }
                    else
                    {
                        Console.WriteLine("Autogenerated Missing Object Missile Tile {0}.", targetRelativePath);
                        WriteTileNameAutogenerationError(relativePath, targetRelativePath, _missileAutogenerateType);
                    }
                    DrawImageToTileSet(missileBitmap);
                    IncreaseCurXY();
                }               
            }
            else
            {
                string subDir2 = Path.Combine(objectType.ToFileName(), nameOrDesc.ToFileName());
                string fileName = objectTypeSingular.ToFileName() + "_" +
                    nameOrDesc.ToFileName() +
                    _typeSuffix[type] + Program.ImageFileExtension;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                string templateSubDir = objectType.ToFileName();

                string templateFileName = null;
                if(string.IsNullOrEmpty(subTypeName))
                {
                    templateFileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + Program.ImageFileExtension;
                }
                else
                {
                    templateFileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + "_" + subTypeName.ToDashed() + Program.ImageFileExtension;
                }

                string templateDirPath = Path.Combine(BaseDirectory.FullName, templateSubDir);
                string templateRelativePath = Path.Combine(_subDirName, templateSubDir, templateFileName);
                string templateFilePath = Path.Combine(templateDirPath, templateFileName);
                FileInfo templateFile = new FileInfo(templateFilePath);

                if (file.Exists)
                {
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment);
                        StoreTileFile(file);
                    }

                    Console.WriteLine("Compiled Object {0} successfully.", relativePath);
                    WriteTileNameSuccess(relativePath);
                }
                else if(templateFile.Exists)
                {
                    using (var image = CreateItemFromTemplate(templateFile, templateColor, subTypeCode, subTypeName))
                    {
                        DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment);
                        StoreTileFile(templateFile, false, true, new TemplateData(templateColor, subTypeCode, subTypeName));
                    }

                    Console.WriteLine("Created Object {0} from Template {1} successfully.", relativePath, templateRelativePath);
                    WriteTileNameTemplateGenerationSuccess(relativePath, templateRelativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Object Tile.", file.FullName);
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Object Tile.");

                    var subType = objectTypeSingular;
                    if (type != _typeNormal)
                    {
                        subType += " " + type;
                    }

                    using (var image = MissingObjectTileCreator.CreateTile(_missingTileType, subType, nameOrDesc.ToProperCaseFirst()))
                    {
                        DrawImageToTileSet(image);
                    }
                }

                IncreaseCurXY();
            }           
        }
    }
}
