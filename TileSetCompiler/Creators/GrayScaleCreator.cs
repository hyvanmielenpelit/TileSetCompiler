using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using TileSetCompiler.Exceptions;

namespace TileSetCompiler.Creators
{
    class GrayScaleCreator
    {
        public ColorMatrix GrayScaleMatrix { get; set; }

        public GrayScaleCreator()
        {
            float darken = 0.8f;
            float red = 0.3f * darken;
            float green = 0.59f * darken;
            float blue = 0.11f * darken;

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

        public Bitmap CreateGrayScaleBitmap(Bitmap sourceBitmap)
        {
            Bitmap destBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

            using (Graphics g = Graphics.FromImage(destBitmap))
            {
                using (ImageAttributes attr = new ImageAttributes())
                {
                    attr.SetColorMatrix(GrayScaleMatrix);

                    g.DrawImage(sourceBitmap, new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                        0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel, attr);
                }
            }

            return destBitmap;
        }

        public Bitmap CreateCroppedGrayScaleBitmap(Bitmap image, Point point, Size size)
        {
            using (var croppedBitmap = image.Clone(new Rectangle(point, size), image.PixelFormat))
            {
                return CreateGrayScaleBitmap(croppedBitmap);
            }
        }
    }
}
