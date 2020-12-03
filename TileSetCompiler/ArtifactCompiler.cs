using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    class ArtifactCompiler : ItemCompiler
    {
        const string _subDirName = "Artifacts";
        const string _objectSubDirName = "Objects";
        const int _lineLength = 15;
        const string _artifactMissingTileType = "Artifact";
        const string _artifactNoDescription = "no base item description";
        const string _missingFloorTileType = "FloorArti";


        protected DirectoryInfo ObjectBaseDirectory { get; set; }
        public MissingTileCreator MissingArtifactTileCreator { get; set; }
        public MissingTileCreator MissingArtifactFloorTileCreator { get; set; }


        public ArtifactCompiler(StreamWriter tileNameWriter) : base(_subDirName, tileNameWriter)
        {
            ObjectBaseDirectory = new DirectoryInfo(Path.Combine(Program.InputDirectory.FullName, _objectSubDirName));

            MissingArtifactTileCreator = new MissingTileCreator();
            MissingArtifactTileCreator.TextColor = Color.Purple;
            MissingArtifactTileCreator.TileSize = MissingTileSize.Item;

            MissingArtifactFloorTileCreator = new MissingTileCreator();
            MissingArtifactFloorTileCreator.TextColor = Color.Purple;
            MissingArtifactFloorTileCreator.BitmapSize = Program.ItemSize;
        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _lineLength)
            {
                throw new Exception(string.Format("Artifact line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var type = splitLine[1];
            var name = splitLine[2];
            var desc = splitLine[3];
            var desc2 = splitLine[5];
            var nameOrDesc = name;
            if(!string.IsNullOrWhiteSpace(desc))
            {
                nameOrDesc = desc;
            }
            else if (!string.IsNullOrWhiteSpace(desc2))
            {
                nameOrDesc = desc2;
            }

            if(!_typeSuffix.ContainsKey(type))
            {
                throw new Exception(string.Format("Artifact Type '{0}' unknown. Line: '{1}'.", type, string.Join(',', splitLine)));
            }

            string direction = null;
            bool isFullSizeBitmap = true;

            if (type == _typeMissile)
            {
                direction = splitLine[6];
                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("Invalid direction '{0}'. Line: '{1}'.", direction, string.Join(',', splitLine)));
                }
            }
            else
            {
                isFullSizeBitmap = int.Parse(splitLine[6]) > 0;
            }

            int widthInTiles = int.Parse(splitLine[7]);
            int heightInTiles = int.Parse(splitLine[8]);
            int mainTileAlignmentInt = int.Parse(splitLine[9]);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            MainTileAlignment mainTileAlignment = (MainTileAlignment)mainTileAlignmentInt;

            int colorCode = int.Parse(splitLine[10]);
            Color templateColor = GetColorFromColorCode(colorCode);

            int subTypeCode = int.Parse(splitLine[11]);
            string subTypeName = splitLine[12];
            string objectType = splitLine[13];
            var objectTypeSingular = GetSingular(objectType);

            int hasFloorTileInt = int.Parse(splitLine[14]);
            bool hasFloorTile = hasFloorTileInt > 0;

            if (type == _typeMissile)
            {
                //Autogenerate missile icon
                var subDir2 = name.ToFileName();
                var fileNameBase = name.ToFileName() + Program.ImageFileExtension;
                var fileNameMissile = name.ToFileName() + _missileSuffix + Program.ImageFileExtension;
                var fileNameFloor = name.ToFileName() + _floorSuffix + Program.ImageFileExtension;

                if (!_missileData.ContainsKey(direction))
                {
                    throw new Exception(string.Format("_missileData does not contain direction '{0}'.", direction));
                }

                MissileDirection missileDirection = _missileData[direction].Direction;

                string dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var relativePathBase = Path.Combine(_subDirName, subDir2, fileNameBase);
                var filePathBase = Path.Combine(dirPath, fileNameBase);
                FileInfo fileBase = new FileInfo(filePathBase);

                var relativePathFloor = Path.Combine(_subDirName, subDir2, fileNameFloor);
                var filePathFloor = Path.Combine(dirPath, fileNameFloor);
                FileInfo fileFloor = new FileInfo(filePathFloor);

                var relativePathMissile = Path.Combine(_subDirName, subDir2, fileNameMissile);
                var filePathMissile = Path.Combine(dirPath, fileNameMissile);
                FileInfo fileMissile = new FileInfo(filePathMissile);

                var targetSubDir2 = name.ToFileName();
                var targetFileName = name.ToFileName() +
                    _typeSuffix[type] +
                    _missileData[direction].FileSuffix + Program.ImageFileExtension;
                var targetRelativePath = Path.Combine(_subDirName, targetSubDir2, targetFileName);

                bool isTileMissing = false;

                FileInfo file = null;
                string relativePath = null;
                if (fileMissile.Exists)
                {
                    file = fileMissile;
                    relativePath = relativePathMissile;
                }
                else if (fileFloor.Exists)
                {
                    file = fileFloor;
                    relativePath = relativePathFloor;
                }
                else
                {
                    file = fileBase;
                    relativePath = relativePathBase;
                }

                using (var missileBitmap = ItemMissileCreator.CreateMissileFromFile(file, nameOrDesc.ToProperCaseFirst(), missileDirection, out isTileMissing))
                {
                    if (!isTileMissing)
                    {
                        Console.WriteLine("Autogenerated Artifact Missile Tile {0} successfully.", relativePath);
                        WriteTileNameAutogenerationSuccess(relativePath, targetRelativePath, _missileAutogenerateType);
                    }
                    else
                    {
                        Console.WriteLine("Autogenerated Missing Artifact Missile Tile {0}.", relativePath);
                        WriteTileNameAutogenerationError(relativePath, targetRelativePath, _missileAutogenerateType);
                    }
                    DrawImageToTileSet(missileBitmap);
                    StoreTileFile(file);
                    IncreaseCurXY();
                }
            }
            else
            {
                var subDir2 = name.ToFileName();
                var dirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                var fileName = name.ToFileName() + _typeSuffix[type] + Program.ImageFileExtension;

                var relativePath = Path.Combine(_subDirName, subDir2, fileName);
                var filePath = Path.Combine(dirPath, fileName);
                FileInfo file = new FileInfo(filePath);

                string fileNameFloor = name.ToFileName() + _typeSuffix[type] + _floorSuffix + Program.ImageFileExtension;
                string relativePathFloor = Path.Combine(_subDirName, subDir2, fileNameFloor);
                string filePathFloor = Path.Combine(dirPath, fileNameFloor);
                FileInfo fileFloor = hasFloorTile ? new FileInfo(filePathFloor) : null;

                //-----------------------------------------------
                // Template 1 is found under Artifacts directory
                //-----------------------------------------------
                string templateFileName = null;
                if (string.IsNullOrEmpty(subTypeName))
                {
                    templateFileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + Program.ImageFileExtension;
                }
                else
                {
                    templateFileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + "_" + subTypeName.ToDashed() + Program.ImageFileExtension;
                }

                string templateDirPath = BaseDirectory.FullName;
                string templateRelativePath = Path.Combine(_subDirName, templateFileName);
                string templateFilePath = Path.Combine(templateDirPath, templateFileName);
                FileInfo templateFile = new FileInfo(templateFilePath);

                //---------------------------------------------
                // Template 2 is found under Objects directory
                //---------------------------------------------
                string template2SubDir = objectType.ToFileName();

                string template2FileName = null;
                if (string.IsNullOrEmpty(subTypeName))
                {
                    template2FileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + Program.ImageFileExtension;
                }
                else
                {
                    template2FileName = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateSuffix + "_" + subTypeName.ToDashed() + Program.ImageFileExtension;
                }

                string template2DirPath = Path.Combine(ObjectBaseDirectory.FullName, template2SubDir);
                string template2RelativePath = Path.Combine(_objectSubDirName, template2SubDir, template2FileName);
                string template2FilePath = Path.Combine(template2DirPath, template2FileName);
                FileInfo template2File = new FileInfo(template2FilePath);

                var subType = "";
                if (type != _typeNormal)
                {
                    subType = type;
                }

                if (file.Exists)
                {
                    using (var image = new Bitmap(Image.FromFile(file.FullName)))
                    {
                        using(var floorImage = GetFloorTile(fileFloor, hasFloorTile, subType, nameOrDesc))
                        {
                            DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment, floorImage);
                            StoreTileFile(file);
                        }
                    }

                    Console.WriteLine("Compiled Artifact '{0}' successfully.", relativePath);
                    WriteTileNameSuccess(relativePath);
                }
                else if (templateFile.Exists)
                {
                    string templateFileNameFloor = null;
                    if (string.IsNullOrEmpty(subTypeName))
                    {
                        templateFileNameFloor = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateFloorSuffix + Program.ImageFileExtension;
                    }
                    else
                    {
                        templateFileNameFloor = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateFloorSuffix + "_" + subTypeName.ToDashed() + Program.ImageFileExtension;
                    }
                    string templateFilePathFloor = Path.Combine(templateDirPath, templateFileNameFloor);
                    FileInfo templateFileFloor = new FileInfo(templateFilePathFloor);

                    using (var image = CreateItemFromTemplate(templateFile, templateColor, subTypeCode, subTypeName))
                    {
                        using (var floorTemplateImage = GetFloorTileFromTemplate(templateFileFloor, templateColor, subTypeCode, subTypeName, hasFloorTile, subType, nameOrDesc))
                        {
                            DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment, floorTemplateImage);
                            StoreTileFile(templateFile, false, true, new TemplateData(templateColor, subTypeCode, subTypeName));
                        }
                    }

                    Console.WriteLine("Created Object {0} from Template {1} successfully.", relativePath, templateRelativePath);
                    WriteTileNameTemplateGenerationSuccess(relativePath, templateRelativePath);
                }
                else if (template2File.Exists)
                {
                    string template2FileNameFloor = null;
                    if (string.IsNullOrEmpty(subTypeName))
                    {
                        template2FileNameFloor = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateFloorSuffix + Program.ImageFileExtension;
                    }
                    else
                    {
                        template2FileNameFloor = objectTypeSingular.ToFileName() + _typeSuffix[type] + _templateFloorSuffix + "_" + subTypeName.ToDashed() + Program.ImageFileExtension;
                    }
                    string template2FilePathFloor = Path.Combine(template2DirPath, template2FileNameFloor);
                    FileInfo template2FileFloor = new FileInfo(template2FilePathFloor);

                    using (var image = CreateItemFromTemplate(template2File, templateColor, subTypeCode, subTypeName))
                    {
                        using (var floorTemplateImage = GetFloorTileFromTemplate(template2FileFloor, templateColor, subTypeCode, subTypeName, hasFloorTile, subType, nameOrDesc))
                        {
                            DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment, floorTemplateImage);
                            StoreTileFile(template2File, false, true, new TemplateData(templateColor, subTypeCode, subTypeName));
                        }
                    }

                    Console.WriteLine("Created Object {0} from Template {1} successfully.", relativePath, template2RelativePath);
                    WriteTileNameTemplateGenerationSuccess(relativePath, template2RelativePath);
                }
                else
                {
                    Console.WriteLine("File '{0}' not found. Creating Missing Artifact icon.", file.FullName);
                    WriteTileNameErrorFileNotFound(relativePath, "Creating Missing Artifact icon.");

                    var missingTileCreator = isFullSizeBitmap ? MissingArtifactTileCreator : MissingArtifactFloorTileCreator;
                    using (var image = missingTileCreator.CreateTile(_artifactMissingTileType, subType, nameOrDesc))
                    {
                        using (var floorImage = GetFloorTile(fileFloor, hasFloorTile, subType, nameOrDesc))
                        {
                            DrawItemToTileSet(image, isFullSizeBitmap, mainTileAlignment, floorImage);
                        }
                    }
                }
                
                IncreaseCurXY();
            }            
        }

        private Bitmap GetFloorTile(FileInfo fileFloor, bool hasFloorTile, string subType, string nameOrDesc)
        {
            if (fileFloor != null && fileFloor.Exists)
            {
                return new Bitmap(Image.FromFile(fileFloor.FullName));
            }
            else if (hasFloorTile)
            {
                return MissingArtifactFloorTileCreator.CreateTileWithTextLines(_missingFloorTileType, subType, nameOrDesc.ToProperCaseFirst());
            }
            else
            {
                return null;
            }
        }

        private Bitmap GetFloorTileFromTemplate(FileInfo templateFileFloor, Color templateColor, int subTypeCode, string subTypeName, bool hasFloorTile, string subType, string nameOrDesc)
        {
            if (templateFileFloor != null && templateFileFloor.Exists)
            {
                return CreateItemFromTemplate(templateFileFloor, templateColor, subTypeCode, subTypeName);
            }
            else if (hasFloorTile)
            {
                return MissingArtifactFloorTileCreator.CreateTileWithTextLines(_missingFloorTileType, subType, nameOrDesc.ToProperCaseFirst());
            }
            else
            {
                return null;
            }
        }
    }
}
