using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators;
using TileSetCompiler.Creators.Data;

namespace TileSetCompiler
{
    abstract class ItemCompiler : BitmapCompiler
    {
        protected const string _typeNormal = "normal";
        protected const string _typeLit = "lit";
        protected const string _typeMissile = "missile";
        protected const string _missileAutogenerateType = "Missile";

        protected Dictionary<string, string> _typeSuffix = new Dictionary<string, string>()
        {
            { "normal", "" },
            { "lit", "_lit" },
            { "missile", "_missile" }
        };

        protected Dictionary<string, MissileData> _missileData = new Dictionary<string, MissileData>()
        {
            { "generic",  new MissileData("", MissileDirection.MiddleLeft) },
            { "top-left", new MissileData("_top-left", MissileDirection.TopLeft) },
            { "top-center", new MissileData("_top-center", MissileDirection.TopCenter) },
            { "top-right", new MissileData("_top-right", MissileDirection.TopRight) },
            { "middle-left", new MissileData("_middle-left", MissileDirection.MiddleLeft) },
            { "middle-right", new MissileData("_middle-right", MissileDirection.MiddleRight) },
            { "bottom-left", new MissileData("_bottom-left", MissileDirection.BottomLeft) },
            { "bottom-center", new MissileData("_bottom-center", MissileDirection.BottomCenter) },
            { "bottom-right", new MissileData("_bottom-right", MissileDirection.BottomRight) }
        };

        protected const string _baseMissileDirection = "middle-left";

        public MissileCreator ItemMissileCreator { get; set; }

        protected ItemCompiler(string subDirectoryName, StreamWriter tileNameWriter) : base(subDirectoryName, tileNameWriter)
        {
            ItemMissileCreator = new MissileCreator();
        }

        protected void DrawItemToTileSet(Bitmap image, bool isFullSize, MainTileAlignment mainTileAlignment)
        {
            if(isFullSize)
            {
                if(image.Width == Program.MaxTileSize.Width && image.Height == Program.MaxTileSize.Height)
                {
                    DrawImageToTileSet(image);
                }
                else
                {
                    using (Bitmap targetBitmap = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height))
                    {

                        targetBitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                        using (Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
                        {
                            int x = 0;
                            if(image.Width == 2 * Program.MaxTileSize.Width)
                                x = mainTileAlignment == MainTileAlignment.Left ? 0: Program.MaxTileSize.Width;
                            else if (image.Width == 3 * Program.MaxTileSize.Width)
                                x = Program.MaxTileSize.Width;

                            int y = image.Height - targetBitmap.Height;
                            Rectangle srcrect = new Rectangle(x, y, Program.MaxTileSize.Width, Program.MaxTileSize.Height);
                            gTargetBitmap.DrawImage(image, 0, 0, srcrect, GraphicsUnit.Pixel);
                            DrawImageToTileSet(targetBitmap);
                        }
                    }
                }
            }
            else
            {
                using (Bitmap targetBitmap = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height))
                {
                    targetBitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
                    using (Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
                    {
                        int x = targetBitmap.Width - image.Width;
                        int y = targetBitmap.Height - image.Height;
                        gTargetBitmap.DrawImage(image, x, y);
                        DrawImageToTileSet(targetBitmap);
                    }
                }
            }
        }

        protected Bitmap CreateItemFromTemplate(FileInfo templateFile, Color templateColor, int subTypeCode, string subTypeName)
        {
            return CreateBitmapFromTemplate(templateFile, templateColor, Program.ItemSize, subTypeCode, subTypeName);
        }
    }
}
