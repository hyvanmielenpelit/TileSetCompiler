using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Data
{
    public enum BitDepth
    {
        BitDepth24,
        BitDepth32
    }

    public class OutputFileFormatData
    {
        public string Extension { get; set; }
        public TransparencyMode TransparencyMode { get; set; }
        public BitDepth BitDepth { get; set; }

        public OutputFileFormatData()
        {
            BitDepth = BitDepth.BitDepth32;
            TransparencyMode = TransparencyMode.Real;
        }

        public OutputFileFormatData(string extension, TransparencyMode transparencyMode, BitDepth bits)
        {
            Extension = extension;
            TransparencyMode = transparencyMode;
            BitDepth = bits;
        }
    }
}
