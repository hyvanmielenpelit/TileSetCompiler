﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Exceptions;
using TileSetCompiler.Extensions;

namespace TileSetCompiler
{
    abstract class BitmapCompiler
    {
        protected const string _templateSuffix = "_template";
        protected const string _templateFloorSuffix = "_template-floor";

        private List<string> _sEndingSingularWords = new List<string>()
        {
            "status",
        };

        private Dictionary<int, Color> _colorCodeMapping = new Dictionary<int, Color>()
        {
            { 0, Color.Black },
            { 1, Color.Red },
            { 2, Color.Green },
            { 3, Color.Brown },
            { 4, Color.Blue },
            { 5, Color.Magenta },
            { 6, Color.Cyan },
            { 7, Color.Gray },
            { 8, Color.Empty },
            { 9, Color.Orange },
            { 10, Color.LightGreen }, /* Bright Green*/
            { 11, Color.Yellow },
            { 12, Color.LightBlue }, /* Bright Blue*/
            { 13, Color.FromArgb(255, 128, 255) }, /* Bright Magenta -> Light Magenta */
            { 14, Color.LightCyan }, /* Bright Cyan */
            { 15, Color.White }
        };


        protected DirectoryInfo BaseDirectory { get; set; }
        protected StreamWriter TileNameWriter { get; private set; }
        protected Color TransparencyColor { get; private set; }

        protected BitmapCompiler(string subDirectoryName, StreamWriter tileNameWriter)
        {
            BaseDirectory = new DirectoryInfo(Path.Combine(Program.InputDirectory.FullName, subDirectoryName));                
            TileNameWriter = tileNameWriter;
            TransparencyColor = Color.FromArgb(71, 108, 108);
        }

        public abstract void CompileOne(string[] splitLine);

        protected void CropAndDrawImageToTileSet(Bitmap image, Point point, Size size)
        {
            CropAndDrawImageToTileSet(image, point, size, image.Tag as FileInfo);
        }

        protected void CropAndDrawImageToTileSet(Bitmap image, Point point, Size size, FileInfo file, bool flipHorizontal = false, bool flipVertical = false)
        {
            if (point.X + size.Width > image.Width || point.Y + size.Height > image.Height)
            {
                throw new Exception(string.Format("Image '{0}' is not large enough for cloning.", file != null ? file.FullName : ""));
            }

            using (var croppedBitmap = image.Clone(new Rectangle(point, size), image.PixelFormat))
            {
                if(flipHorizontal)
                {
                    croppedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                if(flipVertical)
                {
                    croppedBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
                }
                DrawImageToTileSet(croppedBitmap);
            }
        }

        protected void CropAndDrawImageToTileSet(Bitmap image, ContentAlignment alignment = ContentAlignment.BottomCenter)
        {
            Bitmap rightSizeImage = null;
            bool rightSizeImageNeeded = false;

            try
            {
                if (image.Width > Program.MaxTileSize.Width || image.Height > Program.MaxTileSize.Height)
                {
                    int x = 0; 
                    if(alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.BottomCenter)
                    {
                        x = (image.Width - Program.MaxTileSize.Width) / 2; //Align Horizontally: Center
                    }
                    else if (alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.BottomLeft)
                    {
                        x = 0; //Align Horizontally: Left
                    }
                    else if (alignment == ContentAlignment.TopRight || alignment == ContentAlignment.MiddleRight || alignment == ContentAlignment.BottomRight)
                    {
                        x = image.Width - Program.MaxTileSize.Width; //Align Horizontally: Right
                    }
                    else
                    {
                        throw new NotImplementedException(string.Format("Horizontal Alignment in '{0}' not implemented.", alignment.ToString()));
                    }

                    int y = 0;
                    if (alignment == ContentAlignment.BottomCenter || alignment == ContentAlignment.BottomLeft || alignment == ContentAlignment.BottomRight)
                    {
                        y = image.Height - Program.MaxTileSize.Height; //Align Vertically: Bottom
                    }
                    else if (alignment == ContentAlignment.TopCenter || alignment == ContentAlignment.TopLeft || alignment == ContentAlignment.TopRight)
                    {
                        y = 0; //Align Vertically: Top
                    }
                    else if (alignment == ContentAlignment.MiddleCenter || alignment == ContentAlignment.MiddleLeft || alignment == ContentAlignment.MiddleRight)
                    {
                        y = (image.Height - Program.MaxTileSize.Height) / 2; //Align Vertically: Middle
                    }
                    else
                    {
                        throw new NotImplementedException(string.Format("Vertical Alignment in '{0}' not implemented.", alignment.ToString()));
                    }
                    
                    Rectangle rec = new Rectangle(new Point(x, y), Program.MaxTileSize);
                    rightSizeImage = image.Clone(rec, image.PixelFormat);
                    rightSizeImageNeeded = true;
                }
                else
                {
                    rightSizeImage = image;
                }

                DrawImageToTileSet(rightSizeImage);
            }
            finally
            {
                if (rightSizeImageNeeded)
                {
                    rightSizeImage.Dispose();
                }
            }
        }

        protected void DrawMainTileToTileSet(Bitmap image, int widthInTiles, int heightInTiles, MainTileAlignment mainTileAlignment, FileInfo file)
        {
            Size rightSize = new Size(widthInTiles * Program.MaxTileSize.Width, heightInTiles * Program.MaxTileSize.Height);
            if (image.Size != rightSize)
            {
                throw new WrongSizeException(image.Size, rightSize, string.Format("Image '{0}' should be size {1}x{2} but is actually {3}x{4}.", file.FullName, 
                    rightSize.Width, rightSize.Height, image.Width, image.Height));
            }

            bool isOneTile;
            var point = Program.GetMainTileLocationInPixels(widthInTiles, heightInTiles, mainTileAlignment, out isOneTile);

            if(isOneTile)
            {
                DrawImageToTileSet(image);
                return;
            }

            using (var croppedBitmap = image.Clone(new Rectangle(point, Program.MaxTileSize), image.PixelFormat))
            {
                DrawImageToTileSet(croppedBitmap);
            }
        }


        protected void DrawImageToTileSet(Bitmap image)
        {
            foreach (var kvp in Program.TileSets)
            {
                var size = Program.TileSizes[kvp.Key];

                if (kvp.Key < Program.MaxTileSize.Height)
                {
                    using (var resizedImage = ResizeImage(image, size.Width, size.Height))
                    {
                        DrawImage(resizedImage, size, kvp.Value);
                    }
                }
                else
                {
                    DrawImage(image, size, kvp.Value);
                }
            }
        }        

        private void DrawImage(Bitmap image, Size tileSize, Dictionary<OutputFileFormatData, Dictionary<int, Bitmap>> tileSetDic)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    int tileSetX = Program.CurX * tileSize.Width + x;
                    int tileSetY = Program.CurY * tileSize.Height + y;
                    Color c = image.GetPixel(x, y);
                    foreach(var kvp in tileSetDic)
                    {
                        var tpMode = kvp.Key;
                        var dicSheets = kvp.Value;
                        var tileSet = dicSheets[Program.CurrentTileSet];
                        if (kvp.Key.TransparencyMode == TransparencyMode.Color && c.A == 0)
                        {
                            tileSet.SetPixel(tileSetX, tileSetY, TransparencyColor);
                        }
                        else
                        {
                            tileSet.SetPixel(tileSetX, tileSetY, c);
                        }
                    }                    
                }
            }
        }

        protected void IncreaseCurXY()
        {
            Program.CurX++;
            var currentMaxX = Program.TileSetSizes[Program.CurrentTileSet].Width - 1;
            if (Program.CurX > currentMaxX)
            {
                Program.CurX = 0;
                Program.CurY++;
            }
            Program.CurrentCount++;

            //Check next sheet
            int tileSetIndex = Program.CurrentCount / Program.MaxTilesPerTileSet;
            if(tileSetIndex > Program.CurrentTileSet && tileSetIndex < Program.TileSetCount)
            {
                Program.CurrentTileSet = tileSetIndex;
                Program.CurX = 0;
                Program.CurY = 0;
            }

            currentMaxX = Program.TileSetSizes[Program.CurrentTileSet].Width - 1;
            var currentMaxY = Program.TileSetSizes[Program.CurrentTileSet].Height - 1;

            if (Program.CurY > currentMaxY && Program.CurrentCount != Program.TileSetTileCount[Program.TileSetCount - 1] + ((Program.TileSetCount - 1) * Program.MaxTilesPerTileSet))
            {
                Console.WriteLine("Program.CurY '{0}' is greater than Sheet Max Height '{1}'.", Program.CurY, currentMaxY);
                throw new Exception("Aborting.");
            }
        }

        protected void StoreTileFile(FileInfo file, Size bitmapSize, bool isStatue = false, bool isFromTemplate = false, TemplateData templateData = null, FloorTileData floorTileData = null)
        {
            StoreTileFile(file, bitmapSize, null, null, isStatue, isFromTemplate, templateData);
        }

        protected void StoreTileFile(int subIndex, FileInfo file, string relativePath, Size bitmapSize, bool isStatue = false, bool isFromTemplate = false, TemplateData templateData = null, FloorTileData floorTileData = null)
        {
            StoreTileFile(subIndex, file, relativePath, bitmapSize, null, null, isStatue, isFromTemplate, templateData);
        }

        protected void StoreTileFile(FileInfo file, Size bitmapSize, FloorTileData floorTileData)
        {
            StoreTileFile(file, bitmapSize, null, null, false, false, null, floorTileData);
        }

        protected void StoreTileFile(FileInfo file, Size bitmapSize, Point? pointInTiles, Size? bitmapSizeInTiles, bool flipHorizontal, bool flipVertical)
        {
            StoreTileFile(file, bitmapSize, pointInTiles, bitmapSizeInTiles, false, false, null, null, flipHorizontal, flipVertical);
        }

        protected void StoreTileFile(FileInfo file, Size bitmapSize, Point? pointInTiles, Size? bitmapSizeInTiles, bool isStatue = false, bool isFromTemplate = false, TemplateData templateData = null, FloorTileData floorTileData = null, bool flipHorizontal = false, bool flipVertical = false)
        {
            StoreTileFile(0, file, null, bitmapSize, pointInTiles, bitmapSizeInTiles, isStatue, isFromTemplate, templateData, floorTileData, flipHorizontal, flipVertical);
        }

        protected void StoreTileFile(int subIndex, FileInfo file, string relativePath, Size bitmapSize, Point? pointInTiles, Size? bitmapSizeInTiles, bool isStatue = false, bool isFromTemplate = false, TemplateData templateData = null, FloorTileData floorTileData = null, bool flipHorizontal = false, bool flipVertical = false)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            var tileFileData = new TileData();
            tileFileData.File = file;
            tileFileData.RelativePath = relativePath;
            tileFileData.PointInTiles = pointInTiles;
            tileFileData.BitmapSizeInTiles = bitmapSizeInTiles;
            tileFileData.IsStatue = isStatue;
            tileFileData.IsFromTemplate = isFromTemplate;
            tileFileData.TemplateData = templateData;
            tileFileData.FloorTileData = floorTileData;
            tileFileData.FlipHorizontal = flipHorizontal;
            tileFileData.FlipVertical = flipVertical;

            Program.TileFileData.Add(new Point(Program.CurrentCount, subIndex), tileFileData);
        }

        protected TileData GetTileFile(int tileNumber, int subIndex = 0)
        {
            var point = new Point(tileNumber, subIndex);
            if (!Program.TileFileData.ContainsKey(point))
            {
                return null;
                //throw new Exception(string.Format("Tile {0} has not been stored.", tileNumber));
            }
            return Program.TileFileData[point];
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        protected Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.PixelOffsetMode = PixelOffsetMode.None;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        protected void WriteTileNameLine(string relativePath, string successText, string infoText)
        {
            TileNameWriter.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", Program.CurrentCount,
                (relativePath ?? "").Replace("\t","_"),
                (successText ?? "").Replace("\t", "_"),
                (infoText ?? "").Replace("\t", "_")));
        }

        protected void WriteSubTileNameLine(int subTileIndex, int numSubTiles, string relativePath, string successText, string infoText)
        {
            if(numSubTiles > 1)
            {
                TileNameWriter.WriteLine(string.Format("{0}/{1}\t{2}\t{3}\t{4}", Program.CurrentCount, subTileIndex,
                    (relativePath ?? "").Replace("\t", "_"),
                    (successText ?? "").Replace("\t", "_"),
                    (infoText ?? "").Replace("\t", "_")));
            }
            else
            {
                WriteTileNameLine(relativePath, successText, infoText);
            }
        }

        protected void WriteTileNameSuccess(string relativePath)
        {
            WriteTileNameLine(relativePath, "OK", "OK");
            Program.FoundTileCount++;
        }

        protected void WriteSubTileNameSuccess(int subTileIndex, int numSubTiles, string relativePath)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "OK", "OK");
            Program.FoundTileCount++;
        }

        protected void WriteSubTileNameAutogenerationSuccess(int subTileIndex, int numSubTiles, string relativePathSource, string relativePathDest, string type)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePathDest, "AUTOGENERATE SUCCESS: " + type, "Source: " + relativePathSource);
            Program.AutoGeneratedTileCount++;
        }

        protected void WriteTileNameAutogenerationSuccess(string relativePathSource, string relativePathDest, string type)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE SUCCESS: " + type, "Source: " + relativePathSource);
            Program.AutoGeneratedTileCount++;
        }

        protected void WriteTileNameAutogenerationError(string relativePathSource, string relativePathDest, string type)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE ERROR: " + type, "Source file not found: " + relativePathSource);
            Program.AutoGeneratedMissingTileCount++;
        }

        protected void WriteTileNameErrorFileNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "File not found", infoText);
            Program.MissingTileCount++;
        }

        protected void WriteSubTileNameErrorFileNotFound(int subTileIndex, int numSubTiles, string relativePath, string infoText)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "File not found", infoText);
            Program.MissingTileCount++;
        }

        protected void WriteTileNameErrorDirectoryNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "Directory not found", infoText);
            Program.MissingTileCount++;
        }

        protected void WriteSubTileNameErrorDirectoryNotFound(int subTileIndex, int numSubTiles, string relativePath, string infoText)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "Directory not found", infoText);
            Program.MissingTileCount++;
        }

        protected void WriteTileNameTemplateGenerationSuccess(string relativePath, string templateRelativePath)
        {
            WriteTileNameLine(relativePath, "GENERATED FROM TEMPLATE SUCCESSFULLY", "Template Path: " + templateRelativePath);
            Program.TileNumberFromTemplate++;
        }

        protected void WriteTileReplacementSuccess(string relativePathTileWhichWasReplaced, string relativePathTileReplacedWith)
        {
            WriteTileNameLine(relativePathTileWhichWasReplaced, "REPLACED WITH ANOTHER TILE", "Another Tile Path: " + relativePathTileReplacedWith);
            Program.TilesReplacedWithAnother++;
        }

        protected void DrawSubTile(Bitmap tileBitmap, Size subTileSize, int index, Bitmap subTileBitmap)
        {
            int x = (subTileSize.Width * index) % tileBitmap.Width;
            int y = ((subTileSize.Width * index) / tileBitmap.Width) * subTileSize.Height;

            if (y + subTileSize.Height > tileBitmap.Height)
            {
                throw new Exception(string.Format("Error UI Sub-Tile would overflow in height: {0} > {1}.", y + subTileSize.Height, tileBitmap.Height));
            }
            else if (x + subTileBitmap.Width > tileBitmap.Width)
            {
                throw new Exception(string.Format("Error UI Sub-Tile would overflow in width: {0} > {1}.", x + subTileBitmap.Width, tileBitmap.Width));
            }

            using (Graphics gTileBitmap = Graphics.FromImage(tileBitmap))
            {
                gTileBitmap.DrawImage(subTileBitmap, new Point(x, y));
            }
        }

        protected string GetSingular(string word)
        {
            if(string.IsNullOrEmpty(word))
            {
                return word;
            }

            string wordLower = word.ToLower();
            if (_sEndingSingularWords.Contains(wordLower))
            {
                //Is singular
                return word;
            }

            if (wordLower.EndsWith("ness"))
            {
                //Is singular
                return word;
            }

            if (wordLower.EndsWith("ses"))
            {
                return word.Substring(0, word.Length - 2);
            }

            if (wordLower.EndsWith("s"))
            {
                return word.Substring(0, word.Length - 1);
            }

            //Already singular
            return word;
        }

        protected MainTileAlignment GetMainTileAlignment(string s)
        {
            int mainTileAlignmentInt = int.Parse(s);
            if (!Enum.IsDefined(typeof(MainTileAlignment), mainTileAlignmentInt))
            {
                throw new Exception(string.Format("MainTileAlignment '{0}' is invalid. Should be 0 or 1.", mainTileAlignmentInt));
            }
            return (MainTileAlignment)mainTileAlignmentInt;
        }

        protected Color GetColorFromColorCode(int colorCode)
        {
            if(_colorCodeMapping.ContainsKey(colorCode))
            {
                return _colorCodeMapping[colorCode];
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Invalid Color Code. _colorCodeMapping does not contain value {0}.", colorCode));
            }
        }

        protected Bitmap CreateBitmapFromTemplate(FileInfo templateFile, Color templateColor, Size bitmapSize, int subTypeCode = 0, string subTypeName = null)
        {
            using (var templateImage = new Bitmap(Image.FromFile(templateFile.FullName)))
            {
                if (templateImage.Size != bitmapSize)
                {
                    throw new WrongSizeException(templateImage.Size, bitmapSize,
                        string.Format("File '{0}' is of wrong size. It should be {0}x{1} but it is {2}x{3}.",
                        templateFile.FullName, bitmapSize.Width, bitmapSize.Height, templateImage.Width, templateImage.Height));
                }

                Bitmap bmp = new Bitmap(bitmapSize.Width, bitmapSize.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (Brush b = new SolidBrush(templateColor))
                    {
                        g.FillRectangle(b, 0, 0, bmp.Width, bmp.Height);
                    }

                    g.DrawImage(templateImage, new Point(0, 0));
                }

                //Remove Transparency color
                for(int x = 0; x < bmp.Width; x++)
                {
                    for(int y = 0; y < bmp.Height; y++)
                    {
                        if(bmp.GetPixel(x, y) == TransparencyColor)
                        {
                            bmp.SetPixel(x, y, Color.Transparent);
                        }
                    }
                }

                return bmp;
            }
        }

        //-------------------------------
        // From Old DungeonTile Compiler
        //_______________________________
        protected void WriteCmapTileNameSuccess(string relativePath, string description)
        {
            WriteTileNameLine(relativePath, "OK", GetDescAndInfo(description, null, null));
            Program.FoundTileCount++;
        }

        protected void WriteCmapTileNameErrorFileNotFound(string relativePath, string description, string infoText)
        {
            WriteTileNameLine(relativePath, "File not found", GetDescAndInfo(description, "Info:", infoText));
            Program.MissingTileCount++;
        }

        protected void WriteCmapTileNameErrorDirectoryNotFound(string relativePath, string description, string infoText)
        {
            WriteTileNameLine(relativePath, "Directory not found", GetDescAndInfo(description, "Info:", infoText));
            Program.MissingTileCount++;
        }

        protected void WriteCmapTileNameAutogenerationSuccess(string relativePathSource, string relativePathDest, string type, string description)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE SUCCESS: " + type, GetDescAndInfo(description, "Source:", relativePathSource));
            Program.AutoGeneratedTileCount++;
        }

        protected void WriteCmapTileNameAutogenerationError(string relativePathSource, string relativePathDest, string type, string description)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE ERROR: " + type, GetDescAndInfo(description, "Source file not found:", relativePathSource));
            Program.AutoGeneratedMissingTileCount++;
        }

        private string GetDescAndInfo(string description, string infoHeader, string infoText)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(description))
            {
                sb.Append("Description: ").Append(description);
            }
            if (!string.IsNullOrWhiteSpace(infoText))
            {
                if (sb.Length > 0)
                {
                    sb.Append(", ");
                }
                if (!string.IsNullOrEmpty(infoHeader))
                {
                    sb.Append(infoHeader).Append(" ");
                }
                sb.Append(infoText);
            }
            return sb.ToString();
        }

        protected string GetNameWithoutIndex(string name)
        {
            var nameWithoutIndex = name.ToFileName();
            int lastDash = name.LastIndexOf('-');
            int lastPartNumber = -1;
            if (lastDash > 0 && lastDash < name.Length - 1)
            {
                string lastPart = name.Substring(lastDash + 1);
                bool isNumeral = int.TryParse(lastPart, out lastPartNumber);
                if (isNumeral)
                {
                    nameWithoutIndex = name.Substring(0, lastDash).ToFileName();
                }
            }
            return nameWithoutIndex;
        }
    }
}