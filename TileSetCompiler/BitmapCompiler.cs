using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;

namespace TileSetCompiler
{
    abstract class BitmapCompiler
    {
        private List<string> _sEndingSingularWords = new List<string>()
        {
            "status"
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
            using (var croppedBitmap = image.Clone(new Rectangle(point, size), image.PixelFormat))
            {
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

        protected void DrawMainTileToTileSet(Bitmap image, int widthInTiles, int heightInTiles, MainTileAlignment mainTileAlignment)
        {            
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

        private void DrawImage(Bitmap image, Size tileSize, Dictionary<OutputFileFormatData, Bitmap> tileSetDic)
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
                        var tileSet = kvp.Value;
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
            if (Program.CurX > Program.MaxX)
            {
                Program.CurX = 0;
                Program.CurY++;
            }
            if (Program.CurY > Program.MaxY)
            {
                Console.WriteLine("Program.CurY '{0}' is greater than Program.MaxY '{1}'.", Program.CurY, Program.MaxY);
                throw new Exception("Aborting.");
            }
            Program.CurrentCount++;
        }

        protected void StoreTileFile(FileInfo file, bool isStatue = false)
        {
            if(file == null)
            {
                throw new ArgumentNullException("file");
            }
            var tileFileData = new TileData();
            tileFileData.File = file;
            tileFileData.IsStatue = isStatue;
            Program.TileFileData.Add(Program.CurrentCount, tileFileData);
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
            Program.FoundTileNumber++;
        }

        protected void WriteSubTileNameSuccess(int subTileIndex, int numSubTiles, string relativePath)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "OK", "OK");
            Program.FoundTileNumber++;
        }

        protected void WriteTileNameAutogenerationSuccess(string relativePathSource, string relativePathDest, string type)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE SUCCESS: " + type, "Source: " + relativePathSource);
            Program.AutoGeneratedTileNumber++;
        }

        protected void WriteTileNameAutogenerationError(string relativePathSource, string relativePathDest, string type)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATE ERROR: " + type, "Source file not found: " + relativePathSource);
            Program.AutoGeneratedMissingTileNumber++;
        }

        protected void WriteTileNameErrorFileNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "File not found", infoText);
            Program.MissingTileNumber++;
        }

        protected void WriteSubTileNameErrorFileNotFound(int subTileIndex, int numSubTiles, string relativePath, string infoText)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "File not found", infoText);
            Program.MissingTileNumber++;
        }

        protected void WriteTileNameErrorDirectoryNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "Directory not found", infoText);
            Program.MissingTileNumber++;
        }

        protected void WriteSubTileNameErrorDirectoryNotFound(int subTileIndex, int numSubTiles, string relativePath, string infoText)
        {
            WriteSubTileNameLine(subTileIndex, numSubTiles, relativePath, "Directory not found", infoText);
            Program.MissingTileNumber++;
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

            if(wordLower.EndsWith("ses"))
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
    }
}