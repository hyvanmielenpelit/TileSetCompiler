﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ObjectCompiler : ItemCompiler
    {
        const string _subDirName = "Objects";
        const int _lineLength = 5;
        const string _noDescription = "no description";
        const string _missingTileType = "Object";
        const string _missingMissileType = "Missile";

        public MissingTileCreator MissingObjectTileCreator { get; set; }

        public ObjectCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingObjectTileCreator = new MissingTileCreator();
            MissingObjectTileCreator.TextColor = Color.DarkBlue;
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

            var objectTypeSingular = objectType.ToLower();
            if (objectTypeSingular.EndsWith("s"))
            {
                objectTypeSingular = objectTypeSingular.Substring(0, objectTypeSingular.Length - 1);
            }

            string direction = null;
            string subDir2 = null;
            string fileName = null;

            if (type == _objectTypeMissile)
            {
                if (splitLine.Length < 6)
                {
                    throw new Exception(string.Format("Too few elements in a missile line: '{0}'.", string.Join(',', splitLine)));
                }
                direction = splitLine[5];
                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("Invalid direction '{0}'. Line: '{1}'.", direction, string.Join(',', splitLine)));
                }
            }

            if (type == _objectTypeMissile && _missileData[direction].Direction.HasValue && direction != _baseMissileDirection)
            {
                //Autogenerate missile icon
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"),
                    objectType.ToLower().Replace(" ", "_"),
                    nameOrDesc.ToLower().Replace(" ", "_"));
                    
                fileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" +
                    nameOrDesc.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] +
                    _missileData[_baseMissileDirection].FileSuffix + Program.ImageFileExtension;
                MissileDirection? missileDirection = _missileData[direction].Direction;

                var targetFileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" +
                    nameOrDesc.ToLower().Replace(" ", "_") + 
                    _typeSuffix[type] + 
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                var targetFilePath = Path.Combine(dirPath, targetFileName);
                var targetRelativePath = Path.Combine(_subDirName, subDir2, targetFileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, nameOrDesc.ToProperCaseFirst(), missileDirection.Value, out isTileMissing))
                {
                    if(!isTileMissing)
                    {
                        WriteTileNameAutogenerationSuccess(relativePath, targetRelativePath, _missileAutogenerateType);
                    }
                    else
                    {
                        WriteTileNameAutogenerationError(relativePath, targetRelativePath, _missileAutogenerateType);
                    }
                    DrawImageToTileSet(missileBitmap);
                    IncreaseCurXY();
                }               
            }
            else if (type == _objectTypeMissile)
            {
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"),
                        objectType.ToLower().Replace(" ", "_"),
                        nameOrDesc.ToLower().Replace(" ", "_"));

                fileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" +
                    nameOrDesc.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] +
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, nameOrDesc.ToProperCaseFirst(), MissileDirection.MiddleLeft, out isTileMissing))
                {
                    if (!isTileMissing)
                    {
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Object Missile Tile.");
                    }
                    DrawImageToTileSet(missileBitmap);
                    IncreaseCurXY();
                }
            }
            else
            {
                subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), objectType.ToLower().Replace(" ", "_"));
                fileName = objectTypeSingular.ToLower().Replace(" ", "_") + "_" +
                    nameOrDesc.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] + Program.ImageFileExtension;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                if (!Directory.Exists(dirPath))
                {
                    Console.WriteLine("Object directory '{0}' not found. Creating Missing Object Tile.", dirPath);
                    isTileMissing = true;
                    WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Object Tile.");
                }
                else
                {
                    if (file.Exists)
                    {
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        Console.WriteLine("File '{0}' not found. Creating Missing Object Tile.", file.FullName);
                        isTileMissing = true;
                        WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Object Tile.");
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
                    using (var image = MissingObjectTileCreator.CreateTile(_missingTileType, objectTypeSingular, name))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }

            
        }
    }
}
