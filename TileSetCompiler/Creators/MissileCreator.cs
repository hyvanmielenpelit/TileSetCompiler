using 
    System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators.Data;

namespace TileSetCompiler.Creators
{
    public enum MissileDirection
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    class MissileCreator
    {
        private Dictionary<MissileDirection, MissileBitmapTransformation> _transformations = new Dictionary<MissileDirection, MissileBitmapTransformation>()
        {
            { MissileDirection.TopLeft, new MissileBitmapTransformation(false, 45f) },
            { MissileDirection.TopCenter, new MissileBitmapTransformation(false, 90f) },
            { MissileDirection.TopRight, new MissileBitmapTransformation(true, 315f) },
            { MissileDirection.MiddleLeft, new MissileBitmapTransformation(false, 0f) },
            { MissileDirection.MiddleRight, new MissileBitmapTransformation(true, 0f) },
            { MissileDirection.BottomLeft, new MissileBitmapTransformation(false, 315f) },
            { MissileDirection.BottomCenter, new MissileBitmapTransformation(true, 90f) },
            { MissileDirection.BottomRight, new MissileBitmapTransformation(true, 45f) }
        };

        const string _missileMissingType = "Missile";

        public MissingTileCreator MissingMissileTileCreator { get; set; }

        public MissileCreator()
        {
            MissingMissileTileCreator = new MissingTileCreator();
        }

        public Bitmap CreateMissile(Bitmap middleLeftBitmap, MissileDirection direction)
        {
            MissileBitmapTransformation transformation = _transformations[direction];

            if(transformation.Rotation == 0f)
            {
                Bitmap targetBitmap = new Bitmap(middleLeftBitmap);
                if(transformation.FlipHorizontally)
                {
                    targetBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                }
                return targetBitmap;
            }
            else
            {
                //Create square bitmap
                int sideLength = middleLeftBitmap.Width < middleLeftBitmap.Height ? middleLeftBitmap.Width : middleLeftBitmap.Height;
                int x = (middleLeftBitmap.Width - sideLength) / 2;
                int y = (middleLeftBitmap.Height - sideLength) / 2;
                int width = sideLength;
                int height = sideLength;
                using(Bitmap centerBitmap = middleLeftBitmap.Clone(new Rectangle(x, y, width, height), middleLeftBitmap.PixelFormat))
                {
                    centerBitmap.SetResolution(middleLeftBitmap.HorizontalResolution, middleLeftBitmap.VerticalResolution);
                    if(transformation.FlipHorizontally)
                    {
                        centerBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    }
                    using(Bitmap rotatedBitmap = new Bitmap(centerBitmap.Width, centerBitmap.Height))
                    {
                        rotatedBitmap.SetResolution(middleLeftBitmap.HorizontalResolution, middleLeftBitmap.VerticalResolution);
                        using (Graphics gRotatedBitmap = Graphics.FromImage(rotatedBitmap))
                        {
                            float middleX = (float)centerBitmap.Width / 2;
                            float middleY = (float)centerBitmap.Height / 2;
                            gRotatedBitmap.TranslateTransform(middleX, middleY);
                            gRotatedBitmap.RotateTransform(transformation.Rotation);
                            gRotatedBitmap.TranslateTransform(-1 * middleX, -1 * middleY);
                            gRotatedBitmap.DrawImage(centerBitmap, new Point(0, 0));

                            Bitmap targetBitmap = new Bitmap(middleLeftBitmap.Width, middleLeftBitmap.Height);
                            targetBitmap.SetResolution(middleLeftBitmap.HorizontalResolution, middleLeftBitmap.VerticalResolution);
                            using (Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
                            {
                                gTargetBitmap.DrawImage(rotatedBitmap, x, y);
                                return targetBitmap;
                            }
                        }
                   }
                                      
                }
            }
        }

        public Bitmap CreateMissileFromFile(FileInfo file, string name, MissileDirection direction, out bool isMissing)
        {
            isMissing = false;
            var nameReplaced = name.ToLower().Replace(" ", "_");
            if (!file.Exists)
            {
                isMissing = true;
                var bitmap = MissingMissileTileCreator.CreateTile(_missileMissingType, direction.ToString(), nameReplaced);
                return bitmap;
            }
            else
            {
                using (var bitmap = (Bitmap)Bitmap.FromFile(file.FullName))
                {
                    return CreateMissile(bitmap, direction);
                }
            }
        }
    }
}
