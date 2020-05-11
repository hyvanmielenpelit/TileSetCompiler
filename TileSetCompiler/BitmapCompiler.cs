using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace TileSetCompiler
{
    abstract class BitmapCompiler
    {
        protected DirectoryInfo BaseDirectory { get; set; }
        protected FileInfo UnknownFile { get; set; }
        protected StreamWriter TileNameWriter { get; private set; }
        protected Color TransparencyColor { get; private set; }

        protected BitmapCompiler(string subDirectoryName, string unknownFileName, StreamWriter tileNameWriter)
        {
            BaseDirectory = new DirectoryInfo(Path.Combine(Program.InputDirectory.FullName, subDirectoryName));
            UnknownFile = new FileInfo(Path.Combine(BaseDirectory.FullName, unknownFileName));

            if (!BaseDirectory.Exists)
            {
                throw new Exception(string.Format("Base directory '{0}' not found.", BaseDirectory.FullName));
            }
                
            if (!UnknownFile.Exists)
            {
                throw new Exception(string.Format("Unknown file '{0}' not found in directory '{1}'.", UnknownFile, BaseDirectory.FullName));
            }

            TileNameWriter = tileNameWriter;

            TransparencyColor = Color.FromArgb(71, 108, 108);
        }

        public abstract void CompileOne(string[] splitLine);

        protected void DrawImageToTileSet(Bitmap image)
        {
            foreach(var kvp in Program.TileSets)
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

        private void DrawImage(Bitmap image, Size tileSize, Dictionary<TransparencyMode, Bitmap> tileSetDic)
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
                        if (kvp.Key == TransparencyMode.Color && c.A == 0)
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
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

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
            TileNameWriter.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}", Program.CurrentCount + 1,
                (relativePath ?? "").Replace("\t","_"),
                (successText ?? "").Replace("\t", "_"),
                (infoText ?? "").Replace("\t", "_")));
        }

        protected void WriteTileNameSuccess(string relativePath)
        {
            WriteTileNameLine(relativePath, "OK", "OK");
            Program.FoundTileNumber++;
        }

        protected void WriteTileNameStatueSuccess(string relativePathSource, string relativePathDest)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATED", "Source: " + relativePathSource);
            Program.AutoGeneratedTileNumber++;
        }

        protected void WriteTileNameStatueError(string relativePathSource, string relativePathDest)
        {
            WriteTileNameLine(relativePathDest, "AUTOGENERATED, Source file not found", "Source: " + relativePathSource);
            Program.AutoGeneratedUnknownTileNumber++;
        }

        protected void WriteTileNameErrorFileNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "File not found", infoText);
            Program.UnknownTileNumber++;
        }

        protected void WriteTileNameErrorDirectoryNotFound(string relativePath, string infoText)
        {
            WriteTileNameLine(relativePath, "Directory not found", infoText);
            Program.UnknownTileNumber++;
        }
    }
}
