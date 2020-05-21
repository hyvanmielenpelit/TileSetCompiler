using 
    System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators.Data;
using TileSetCompiler.Exceptions;

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
            MissingMissileTileCreator.TextColor = Color.Brown;
        }

        public Bitmap CreateMissile(Bitmap itemBitmap, MissileDirection direction)
        {
            MissileBitmapTransformation transformation = _transformations[direction];
            Bitmap targetBitmap = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height);
            targetBitmap.SetResolution(itemBitmap.HorizontalResolution, itemBitmap.VerticalResolution);

            if (itemBitmap.Size == Program.ItemSize)
            {
                using (Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
                {
                    if (transformation.Rotation == 0f)
                    {
                        int x = (targetBitmap.Width - itemBitmap.Width) / 2;
                        int y = (targetBitmap.Height - itemBitmap.Height) / 2;
                        gTargetBitmap.DrawImage(itemBitmap, x, y);
                        if (transformation.FlipHorizontally)
                        {
                            targetBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        return targetBitmap;
                    }
                    else
                    {
                        //Create square bitmap
                        int sideLength = Math.Min(Program.MaxTileSize.Width, Program.MaxTileSize.Height);
                        using (Bitmap centerBitmap = new Bitmap(sideLength, sideLength))
                        {
                            centerBitmap.SetResolution(itemBitmap.HorizontalResolution, itemBitmap.VerticalResolution);
                            using (Graphics gCenterBitmap = Graphics.FromImage(centerBitmap))
                            {
                                int x = (centerBitmap.Width - itemBitmap.Width) / 2;
                                int y = (centerBitmap.Height - itemBitmap.Height) / 2;
                                gCenterBitmap.DrawImage(itemBitmap, x, y);
                            }
                            RotateSquareBitmap(targetBitmap, gTargetBitmap, centerBitmap, transformation);
                            return targetBitmap;
                        }
                    }
                }
            }
            else if(itemBitmap.Size == Program.MaxTileSize)
            {
                using (Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
                {
                    if (transformation.Rotation == 0f)
                    {
                        gTargetBitmap.DrawImage(itemBitmap, 0, 0);
                        if (transformation.FlipHorizontally)
                        {
                            targetBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                        return targetBitmap;
                    }
                    else
                    {
                        //Create square bitmap
                        int sideLength = Math.Min(Program.MaxTileSize.Width, Program.MaxTileSize.Height);
                        int x = (itemBitmap.Width- sideLength) / 2;
                        int y = (itemBitmap.Height - sideLength) / 2;
                        using (Bitmap centerBitmap = itemBitmap.Clone(new Rectangle(new Point(x, y), new Size(sideLength, sideLength)), itemBitmap.PixelFormat))
                        {
                            centerBitmap.SetResolution(itemBitmap.HorizontalResolution, itemBitmap.VerticalResolution);
                            using (Graphics gCenterBitmap = Graphics.FromImage(centerBitmap))
                            {
                                gCenterBitmap.DrawImage(itemBitmap, x, y);
                            }
                            RotateSquareBitmap(targetBitmap, gTargetBitmap, centerBitmap, transformation);
                            return targetBitmap;
                        }
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Image for missile creations is of wrong size: {0}x{1}.", itemBitmap.Width, itemBitmap.Height));
            }            
        }

        private void RotateSquareBitmap(Bitmap targetBitmap, Graphics gTargetBitmap, Bitmap centerBitmap, MissileBitmapTransformation transformation)
        {
            if (transformation.FlipHorizontally)
            {
                centerBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            using (Bitmap rotatedBitmap = new Bitmap(centerBitmap.Width, centerBitmap.Height))
            {
                rotatedBitmap.SetResolution(centerBitmap.HorizontalResolution, centerBitmap.VerticalResolution);
                using (Graphics gRotatedBitmap = Graphics.FromImage(rotatedBitmap))
                {
                    gRotatedBitmap.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    float middleX = (float)centerBitmap.Width / 2;
                    float middleY = (float)centerBitmap.Height / 2;
                    gRotatedBitmap.TranslateTransform(middleX, middleY);
                    gRotatedBitmap.RotateTransform(transformation.Rotation);
                    gRotatedBitmap.TranslateTransform(-1 * middleX, -1 * middleY);
                    gRotatedBitmap.DrawImage(centerBitmap, new Point(0, 0));

                    int x = (targetBitmap.Width - rotatedBitmap.Width) / 2;
                    int y = (targetBitmap.Height - rotatedBitmap.Height) / 2;
                    gTargetBitmap.DrawImage(rotatedBitmap, x, y);
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
