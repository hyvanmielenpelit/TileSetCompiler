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
        const string _missileMissingType = "Missile";

        public MissingTileCreator MissingMissileTileCreator { get; set; }

        public MissileCreator()
        {
            MissingMissileTileCreator = new MissingTileCreator();
        }

        public MissileBitmaps CreateMissiles(Bitmap middleLeftBitmap)
        {
            var bitmaps = new MissileBitmaps();

            bitmaps.Add(MissileDirection.MiddleLeft, new Bitmap(middleLeftBitmap));

            var middleRightBitmap = new Bitmap(middleLeftBitmap);
            middleRightBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
            bitmaps.Add(MissileDirection.MiddleRight, middleRightBitmap);

            var topCenterBitmap = new Bitmap(middleLeftBitmap);
            topCenterBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bitmaps.Add(MissileDirection.TopCenter, topCenterBitmap);

            var bottomCenterBitmap = new Bitmap(middleRightBitmap);
            bottomCenterBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bitmaps.Add(MissileDirection.TopCenter, bottomCenterBitmap);

            var topLeftBitmap = new Bitmap(middleLeftBitmap);
            using(Graphics g = Graphics.FromImage(middleLeftBitmap))
            {
                g.RotateTransform(45f);
            }
            bitmaps.Add(MissileDirection.TopLeft, topLeftBitmap);

            var topRightBitmap = new Bitmap(middleRightBitmap);
            using (Graphics g = Graphics.FromImage(topRightBitmap))
            {
                g.RotateTransform(-45f);
            }
            bitmaps.Add(MissileDirection.TopRight, topRightBitmap);

            var bottomLeftBitmap = new Bitmap(middleLeftBitmap);
            using (Graphics g = Graphics.FromImage(bottomLeftBitmap))
            {
                g.RotateTransform(-45f);
            }
            bitmaps.Add(MissileDirection.BottomLeft, bottomLeftBitmap);

            var bottomRightBitmap = new Bitmap(middleRightBitmap);
            using (Graphics g = Graphics.FromImage(bottomRightBitmap))
            {
                g.RotateTransform(45f);
            }
            bitmaps.Add(MissileDirection.BottomRight, bottomRightBitmap);

            return bitmaps;
        }

        public MissileBitmaps CreateMissilesFromFile(FileInfo file, string name)
        {
            var nameReplaced = name.ToLower().Replace(" ", "_");
            if(!file.Exists)
            {
                MissileBitmaps bitmaps = new MissileBitmaps();
                foreach(MissileDirection direction in Enum.GetValues(typeof(MissileDirection)))
                {
                    var bitmap = MissingMissileTileCreator.CreateTile(_missileMissingType, direction.ToString(), nameReplaced);
                    bitmaps.Add(direction, bitmap);
                }
                return bitmaps;
            }
            else
            {
                using(var bitmap = (Bitmap)Bitmap.FromFile(file.FullName))
                {
                    return CreateMissiles(bitmap);
                }
            }
        }

        public Bitmap CreateMissile(Bitmap middleLeftBitmap, MissileDirection direction)
        {
            int sideLength = middleLeftBitmap.Width < middleLeftBitmap.Height ? middleLeftBitmap.Width : middleLeftBitmap.Height;
            int top = (middleLeftBitmap.Height - sideLength) / 2;
            int left = (middleLeftBitmap.Width - sideLength) / 2;
            int width = sideLength;
            int height = sideLength;
            Bitmap centerBitmap = middleLeftBitmap.Clone(new Rectangle(left, top, width, height), middleLeftBitmap.PixelFormat);
            float middleX = (float)centerBitmap.Width / 2;
            float middleY = (float)centerBitmap.Height / 2;
            Point newPoint = new Point(left, top);
            if (direction == MissileDirection.TopLeft)
            {
                var topLeftBitmap = new Bitmap(middleLeftBitmap.Width, middleLeftBitmap.Height);
                using (Graphics g = Graphics.FromImage(centerBitmap))
                {
                    g.TranslateTransform(middleX, middleY);
                    g.RotateTransform(45f);
                    g.DrawImage(topLeftBitmap, newPoint);
                }
                return topLeftBitmap;
            }
            else if (direction == MissileDirection.TopCenter)
            {
                var topCenterBitmap = new Bitmap(middleLeftBitmap);
                topCenterBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                return topCenterBitmap;
            }
            else if (direction == MissileDirection.TopRight)
            {
                var topRightBitmap = new Bitmap(middleLeftBitmap);
                topRightBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                using (Graphics g = Graphics.FromImage(topRightBitmap))
                {
                    g.TranslateTransform(middleX, middleY);
                    g.RotateTransform(-45f);
                    g.DrawImage(topRightBitmap, Point.Empty);
                }
                return topRightBitmap;
            }
            else if (direction == MissileDirection.MiddleLeft)
            {
                return new Bitmap(middleLeftBitmap);
            }
            else if (direction == MissileDirection.MiddleRight)
            {
                var middleRightBitmap = new Bitmap(middleLeftBitmap);
                middleRightBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                return middleRightBitmap;
            }
            if (direction == MissileDirection.BottomLeft)
            {
                var bottomLeftBitmap = new Bitmap(middleLeftBitmap);
                using (Graphics g = Graphics.FromImage(bottomLeftBitmap))
                {
                    g.TranslateTransform(middleX, middleY);
                    g.RotateTransform(-45f);
                    g.DrawImage(bottomLeftBitmap, Point.Empty);
                }
                return bottomLeftBitmap;
            }
            if (direction == MissileDirection.BottomCenter)
            {
                var bottomCenterBitmap = new Bitmap(middleLeftBitmap);
                bottomCenterBitmap.RotateFlip(RotateFlipType.Rotate90FlipX);
                return bottomCenterBitmap;
            }
            if (direction == MissileDirection.BottomRight)
            {
                var bottomRightBitmap = new Bitmap(middleLeftBitmap);
                bottomRightBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);
                using (Graphics g = Graphics.FromImage(bottomRightBitmap))
                {
                    g.TranslateTransform(middleX, middleY);
                    g.RotateTransform(45f);
                    g.DrawImage(bottomRightBitmap, Point.Empty);
                }
                return bottomRightBitmap;
            }
            else
            {
                throw new NotImplementedException();
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
