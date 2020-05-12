using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler.Creators
{
    class DarknessCreator
    {
        public DirectoryInfo BaseDirectory { get; set; }
        public FileInfo UnknownFile { get; private set; }

        public DarknessCreator(string subDirName, string unknownFileName)
        {
            if (string.IsNullOrEmpty(subDirName))
            {
                BaseDirectory = Program.InputDirectory;
            }
            else
            {
                BaseDirectory = new DirectoryInfo(Path.Combine(Program.InputDirectory.FullName, subDirName));
            }

            UnknownFile = new FileInfo(Path.Combine(BaseDirectory.FullName, unknownFileName));

            if (!UnknownFile.Exists)
            {
                throw new Exception(string.Format("Unknown Statue File '{0}' not found.", UnknownFile.FullName));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceBitmap"></param>
        /// <param name="opaquePercent">Must be between 0 and 1. 0 is completely transparent, 1 is completely opaque.</param>
        /// <returns></returns>
        public Bitmap CreateDarkBitmap(Bitmap sourceBitmap, float opacity)
        {
            if(opacity > 1f || opacity < 0f)
            {
                throw new ArgumentOutOfRangeException("opaquePercent", opacity, "opaquePercent must be between 0 and 1.");
            }

            Bitmap destBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            using (Graphics g = Graphics.FromImage(destBitmap))
            {
                var rect = new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height);
                g.DrawImage(sourceBitmap, rect);

                //Create a solid black, partly transparent brush
                SolidBrush sb = new SolidBrush(Color.FromArgb((int)(255f * opacity), 0, 0, 0));
                g.FillRectangle(sb, rect);
            }

            return destBitmap;
        }

        public Bitmap CreateDarkBitmapFromFile(FileInfo sourceFile, float opacity, out bool isUnknown)
        {
            FileInfo usedSourceFile = null;
            isUnknown = false;
            if (!sourceFile.Exists)
            {
                Console.WriteLine("Source File '{0}' not found. Using Unknown Statue file.", sourceFile.FullName);
                usedSourceFile = UnknownFile;
                isUnknown = true;
            }
            else
            {
                usedSourceFile = sourceFile;
            }
            Bitmap sourceBitmap = (Bitmap)Image.FromFile(usedSourceFile.FullName);
            if (!isUnknown)
            {
                var statueBitmap = CreateDarkBitmap(sourceBitmap, opacity);
                sourceBitmap.Dispose();
                return statueBitmap;
            }
            else
            {
                return sourceBitmap;
            }
        }
    }
}
