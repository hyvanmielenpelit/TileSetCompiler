using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace TileSetCompiler.Creators
{
    class StatueCreator
    {
        const string _missingTileType = "Statue";
        const string _missingTileSubType = null;

        public ColorMatrix GrayScaleMatrix { get; set; }
        public MissingTileCreator MissingStatueTileCreator { get; private set; }

        public StatueCreator()
        {
            MissingStatueTileCreator = new MissingTileCreator();
            MissingStatueTileCreator.TextColor = Color.GhostWhite;
            MissingStatueTileCreator.BackgroundColor = Color.Gray;
            MissingStatueTileCreator.Capitalize = true;

            float red = 0.8f;
            float green = 0.8f;
            float blue = 0.8f;

            GrayScaleMatrix = new ColorMatrix(
              new float[][]
              {
                 new float[] {red,   red,   red,   0, 0},
                 new float[] {green, green, green, 0, 0},
                 new float[] {blue,  blue,  blue,  0, 0},
                 new float[] {    0,     0,     0, 1, 0},
                 new float[] {    0,     0,     0, 0, 1}
              });

            //GrayScaleMatrix = new ColorMatrix(
            //  new float[][]
            //  {
            //     new float[] {0.30f, 0.30f, 0.30f, 0, 0},
            //     new float[] {0.59f, 0.59f, 0.59f, 0, 0},
            //     new float[] {0.11f, 0.11f, 0.11f, 0, 0},
            //     new float[] {    0,     0,     0, 1, 0},
            //     new float[] {    0,     0,     0, 0, 1}
            //  });
        }

        public Bitmap CreateStatueBitmap(Bitmap sourceBitmap)
        {
            Bitmap destBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            
            using (Graphics g = Graphics.FromImage(destBitmap))
            {
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorMatrix(GrayScaleMatrix);

                g.DrawImage(sourceBitmap, new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                    0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel, attr);
            }

            return destBitmap;            
        }

        public Bitmap CreateStatueBitmapFromFile(FileInfo sourceFile, string name, out bool isUnknown)
        {
            isUnknown = false;
            if (!sourceFile.Exists)
            {
                Console.WriteLine("Source File '{0}' not found. Creating Missing Statue file.", sourceFile.FullName);
                isUnknown = true;
            }
            if (!isUnknown)
            {
                using (Bitmap sourceBitmap = (Bitmap)Image.FromFile(sourceFile.FullName))
                {
                    return CreateStatueBitmap(sourceBitmap);
                }
            }
            else
            {
                return MissingStatueTileCreator.CreateTile(_missingTileType, _missingTileSubType, name);
            }
            
        }
    }
}
