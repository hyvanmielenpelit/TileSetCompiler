﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ArtifactCompiler : ItemCompiler
    {
        const string _subDirName = "Artifacts";
        const int _lineLength = 3;
        const string _artifactMissingTileType = "Artifact";
        const string _artifactNoDescription = "no base item description";

        public MissingTileCreator MissingArtifactTileCreator { get; set; }

        public ArtifactCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            MissingArtifactTileCreator = new MissingTileCreator();
        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Artifact line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var name = splitLine[2];

            if(!_typeSuffix.ContainsKey(type))
            {
                throw new Exception(string.Format("Artifact Type '{0}' unknown. Line: '{1}'.", type, string.Join(',', splitLine)));
            }

            string direction = null;

            if (type == _objectTypeMissile)
            {
                if (splitLine.Length < 7)
                {
                    throw new Exception(string.Format("Too few elements in a missile line: '{0}'.", string.Join(',', splitLine)));
                }
                direction = splitLine[6];
                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("Invalid direction '{0}'. Line: '{1}'.", direction, string.Join(',', splitLine)));
                }
            }

            if (type == _objectTypeMissile && _missileData[direction].Direction.HasValue && direction != _baseMissileDirection)
            {
                //Autogenerate missile icon
                var subDir2 = Path.Combine(type.ToLower().Replace(" ", "_"), name.ToLower().Replace(" ", "_"));

                var fileName = name.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] +
                    _missileData[_baseMissileDirection].FileSuffix + Program.ImageFileExtension;
                MissileDirection? missileDirection = _missileData[direction].Direction;

                var targetFileName = name.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] +
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                var targetFilePath = Path.Combine(dirPath, targetFileName);
                var targetRelativePath = Path.Combine(_subDirName, subDir2, targetFileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, name.ToProperCaseFirst(), missileDirection.Value, out isTileMissing))
                {
                    if (!isTileMissing)
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
                var dirPath = Path.Combine(type.ToLower().Replace(" ", "_"), name.ToLower().Replace(" ", "_"));
                var fileName = name.ToLower().Replace(" ", "_") +
                    _typeSuffix[type] +
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, dirPath, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, name.ToProperCaseFirst(), MissileDirection.MiddleLeft, out isTileMissing))
                {
                    if (!isTileMissing)
                    {
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Artifact Missile Tile.");
                    }
                    DrawImageToTileSet(missileBitmap);
                    IncreaseCurXY();
                }
            }
            else
            {
                var dirPath = Path.Combine(BaseDirectory.FullName, type.ToLower().Replace(" ", "_"));
                var fileName = name.ToLower().Replace(" ", "_") + _typeSuffix[type] + Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                bool isTileMissing = false;

                if (!Directory.Exists(dirPath))
                {
                    Console.WriteLine("Artifact directory '{0}' not found. Creating Missing Artifact icon.", dirPath);
                    isTileMissing = true;
                    WriteTileNameErrorDirectoryNotFound(relativePath, "Creating Missing Artifact icon.");
                }
                else
                {
                    if (file.Exists)
                    {
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        Console.WriteLine("File '{0}' not found. Creating Missing Artifact icon.", file.FullName);
                        isTileMissing = true;
                        WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Artifact icon.");
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
                    using (var image = MissingArtifactTileCreator.CreateTile(_artifactMissingTileType, "", name))
                    {
                        DrawImageToTileSet(image);
                    }
                }
                IncreaseCurXY();
            }

            
        }
    }
}
