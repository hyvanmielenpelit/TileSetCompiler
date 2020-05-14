using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace TileSetCompiler.Creators
{
    class DarknessCreator
    {
        public string MissingTileTypeName { get; private set; }
        public MissingTileCreator MissingDarknessTileCreator { get; set; }

        public DarknessCreator(string missingTileTypeName)
        {
            MissingTileTypeName = missingTileTypeName;
            MissingDarknessTileCreator = new MissingTileCreator();
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

        public Bitmap CreateDarkBitmapFromFile(FileInfo sourceFile, string name, float opacity, out bool isUnknown)
        {
            isUnknown = false;
            if (!sourceFile.Exists)
            {
                Console.WriteLine("Source File '{0}' not found. Creating Missing Darkened Tile.", sourceFile.FullName);
                isUnknown = true;
            }
            
            if (!isUnknown)
            {
                using(Bitmap sourceBitmap = (Bitmap)Image.FromFile(sourceFile.FullName))
                {
                    return CreateDarkBitmap(sourceBitmap, opacity);
                }
            }
            else
            {
                using (var missingBitmap = MissingDarknessTileCreator.CreateTile(MissingTileTypeName, "Darkened", name))
                {
                    return CreateDarkBitmap(missingBitmap, opacity);
                }
            }
        }
    }
}
