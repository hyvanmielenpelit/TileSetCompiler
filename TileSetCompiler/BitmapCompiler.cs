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
        protected FileInfo Manifest { get; set; }
        protected FileInfo UnknownFile { get; set; }

        protected BitmapCompiler(string subDirectoryName, string manifestFileName, string unknownFileName)
        {
            BaseDirectory = new DirectoryInfo(Path.Combine(Program.WorkingDirectory.FullName, subDirectoryName));
            Manifest = new FileInfo(Path.Combine(BaseDirectory.FullName, manifestFileName));
            UnknownFile = new FileInfo(Path.Combine(BaseDirectory.FullName, unknownFileName));

            if (!BaseDirectory.Exists)
            {
                throw new Exception(string.Format("Monsters directory '{0}' not found.", Program.WorkingDirectory.FullName + "\\" + "Monsters"));
            }
                
            if (!Manifest.Exists)
            {
                throw new Exception(string.Format("Manifest file '{0}' not found in directory '{1}'.", Manifest, BaseDirectory.FullName));
            }

            if (!UnknownFile.Exists)
            {
                throw new Exception(string.Format("Unknown monster file '{0}' not found in directory '{1}'.", UnknownFile, BaseDirectory.FullName));
            }
        }

        public abstract void Compile();

        public abstract int GetTileNumber();

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

        private void DrawImage(Bitmap image, Size tileSize, Bitmap tileSet)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    int tileSetX = Program.CurX * tileSize.Width + x;
                    int tileSetY = Program.CurY * tileSize.Height + y;
                    Color c = image.GetPixel(x, y);
                    tileSet.SetPixel(tileSetX, tileSetY, c);
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
    }
}
