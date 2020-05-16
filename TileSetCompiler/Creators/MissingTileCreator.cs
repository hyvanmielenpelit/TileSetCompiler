﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using TileSetCompiler.Extensions;

namespace TileSetCompiler.Creators
{
    public class MissingTileCreator
    {
        public Color TextColor { get; set; }
        public Font TextFont { get; private set; }
        public Color BackgroundColor { get; set; }
        public StringAlignment HorizontalAlignment { get; set; }
        public StringAlignment VerticalAlignment { get; set; }
        public bool Capitalize { get; set; }

        public MissingTileCreator()
        {
            TextColor = Color.Black;
            TextFont = new Font(FontFamily.GenericSansSerif, 9.0f);
            BackgroundColor = Color.Transparent;
            HorizontalAlignment = StringAlignment.Center;
            VerticalAlignment = StringAlignment.Center;
            Capitalize = true;
        }

        public void SetTextFont(FontFamily family, float size)
        {
            TextFont = new Font(family, size);
        }

        public Bitmap CreateTile(string type, string subType, string name, bool? capitalizeTexts = null)
        {
            Bitmap bmp = new Bitmap(Program.MaxTileSize.Width, Program.MaxTileSize.Height);
            using(Graphics g = Graphics.FromImage(bmp))
            {
                Brush bgBrush = new SolidBrush(BackgroundColor);
                g.FillRectangle(bgBrush, new Rectangle(Point.Empty, Program.MaxTileSize));
                Brush textBrush = new SolidBrush(TextColor);
                SizeF tileSizeF = new SizeF((float)Program.MaxTileSize.Width, (float)Program.MaxTileSize.Height);
                StringFormat sFormat = new StringFormat();
                sFormat.Alignment = HorizontalAlignment;
                sFormat.LineAlignment = VerticalAlignment;
                bool capitalize = capitalizeTexts.HasValue ? capitalizeTexts.Value : Capitalize;
                StringBuilder sb = new StringBuilder();
                if(!string.IsNullOrWhiteSpace(type))
                {
                    sb.AppendLine(capitalize ? type.ToProperCaseFirst() : type);
                }
                if (!string.IsNullOrWhiteSpace(subType))
                {
                    sb.AppendLine(capitalize ? subType.ToProperCaseFirst() : subType);
                }
                var splitName = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach(var word in splitName)
                {
                    sb.AppendLine(capitalize ? word.ToProperCaseFirst() : word);
                }
                var label = sb.ToString();
                g.DrawString(label, TextFont, textBrush, new RectangleF(PointF.Empty, tileSizeF), sFormat);
            }
            return bmp;
        }
    }
}