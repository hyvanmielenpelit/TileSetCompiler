using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using TileSetCompiler.Creators.Data;

namespace TileSetCompiler.Creators
{
    class SpellbookRecolorer : Recolorer
    {
        private Dictionary<string, RecolorData> _spellbookRecolorData = new Dictionary<string, RecolorData>()
        {
            { "red", new RecolorData("baseFile1", "maskFile1", new Dictionary<Color, Color>() { 
                { Color.FromArgb(102, 82, 55), Color.Red } }) }
        };

        public DirectoryInfo BaseDirectory { get; set; }

        public SpellbookRecolorer(string baseDirectoryPath)
        {
            if(baseDirectoryPath == null)
            {
                throw new ArgumentNullException("baseDirectoryPath");
            }
            BaseDirectory = new DirectoryInfo(baseDirectoryPath);
        }

        public Bitmap RecolorSpellbook(string description)
        {
            if (_spellbookRecolorData.ContainsKey(description))
            {
                var spellBookRecolorData = _spellbookRecolorData[description];
                var baseFilePath = Path.Combine(BaseDirectory.FullName, spellBookRecolorData.BaseFileName);
                var baseFile = new FileInfo(baseFilePath);
                using (Bitmap sourceBitmap = (Bitmap)Image.FromFile(baseFile.FullName))
                {
                    FileInfo maskFile = null;
                    if (!string.IsNullOrEmpty(spellBookRecolorData.MaskFileName))
                    {
                        var maskFilePath = Path.Combine(BaseDirectory.FullName, spellBookRecolorData.MaskFileName);
                        maskFile = new FileInfo(maskFilePath);
                    }
                    Bitmap maskBitmap = null;
                    try
                    {
                        if (maskFile != null)
                        {
                            maskBitmap = (Bitmap)Image.FromFile(maskFile.FullName);
                        }
                        return RecolorBitmap(sourceBitmap, spellBookRecolorData.ColorMappings, maskBitmap);
                    }
                    finally
                    {
                        if (maskBitmap != null)
                        {
                            maskBitmap.Dispose();
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("description", string.Format("Spellbook description '{0}' not found.", description));
            }
        }
    }
}
