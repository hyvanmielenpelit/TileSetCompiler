using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using TileSetCompiler.Extensions;

namespace TileSetCompiler.Creators
{
    public class MissingSubTileCreator
    {
        public Color TextColor { get; set; }
        public Font TextFont { get; private set; }
        public Color BackgroundColor { get; set; }
        public StringAlignment HorizontalAlignment { get; set; }
        public StringAlignment VerticalAlignment { get; set; }
        public bool Capitalize { get; set; }

        public MissingSubTileCreator()
        {
            TextColor = Color.White;
            TextFont = new Font(FontFamily.GenericSansSerif, 8.0f);
            BackgroundColor = Color.Black;
            HorizontalAlignment = StringAlignment.Center;
            VerticalAlignment = StringAlignment.Center;
            Capitalize = true;
        }

        public void SetTextFont(FontFamily family, float size)
        {
            TextFont = new Font(family, size);
        }

        public Bitmap CreateSubTile(int width, int height, string text)
        {
            return CreateSubTile(new Size(width, height), text);
        }

        public Bitmap CreateSubTile(Size size, string text)
        {
            int maxRows = size.Height / 16;
            int maxCols = size.Width / 16;
            if(maxCols % 2 == 0)
            {
                maxCols = maxCols / 2 * 3;
            }
            int maxChars = maxRows * maxCols;
            if(maxChars < 1)
            {
                maxChars = 1;
            }

            Bitmap targetBitmap = new Bitmap(size.Width, size.Height);
            using(Graphics gTargetBitmap = Graphics.FromImage(targetBitmap))
            {
                SolidBrush bgBrush = new SolidBrush(BackgroundColor);
                gTargetBitmap.FillRectangle(bgBrush, new Rectangle(Point.Empty, size));

                string text2 = Capitalize ? text.ToProperCase() : text;
                if(text2.Length > maxChars)
                {
                    if(text2.Contains("-"))
                    {
                        var split = text.Split('-');
                        StringBuilder sb = new StringBuilder();
                        foreach(var word in split)
                        {
                            if (sb.Length >= maxChars)
                            {
                                break;
                            }
                            if(word.Length == 0)
                            {
                                continue;
                            }
                            sb.Append(Capitalize ? char.ToUpper(word[0]) : word[0]);
                        }
                        text2 = sb.ToString();
                    }
                    else
                    {
                        text2 = text2.Substring(0, maxChars);
                    }
                }
                SolidBrush textBrush = new SolidBrush(TextColor);
                StringFormat sFormat = new StringFormat();
                sFormat.Alignment = HorizontalAlignment;
                sFormat.LineAlignment = VerticalAlignment;
                gTargetBitmap.DrawString(text2, TextFont, textBrush, 
                    new RectangleF(new PointF(0f, 0f), 
                    new SizeF((float)size.Width, (float)size.Height)), 
                    sFormat);
            }

            return targetBitmap;
        }
    }
}
