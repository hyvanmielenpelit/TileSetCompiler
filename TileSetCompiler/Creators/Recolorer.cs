using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Creators
{
    class Recolorer
    {
        public Recolorer()
        {

        }

        public Bitmap RecolorBitmap(Bitmap sourceBitmap, Dictionary<Color, Color> colorMappings)
        {
            return RecolorBitmap(sourceBitmap, colorMappings, null);
        }

        public Bitmap RecolorBitmap(Bitmap sourceBitmap, Dictionary<Color, Color> colorMappings, Bitmap mask)
        {
            if(sourceBitmap == null)
            {
                throw new ArgumentNullException("sourceBitmap");
            }
            if(colorMappings == null)
            {
                throw new ArgumentNullException("colorMappings");
            }

            Bitmap targetBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            for(int x = 0; x < sourceBitmap.Width; x++)
            {
                for(int y = 0; y < sourceBitmap.Height; y++)
                {
                    bool inMask = true;
                    if (mask != null)
                    {
                        var maskPixel = mask.GetPixel(x, y);
                        if(maskPixel == Color.Black)
                        {
                            inMask = false;
                        }
                    }
                    if(inMask)
                    {
                        var pixelColor = sourceBitmap.GetPixel(x, y);
                        if (colorMappings.ContainsKey(pixelColor))
                        {
                            targetBitmap.SetPixel(x, y, colorMappings[pixelColor]);
                        }
                    }
                }
            }
            return targetBitmap;
        }
    }
}
