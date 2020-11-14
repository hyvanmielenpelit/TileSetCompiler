using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TileSetCompiler.Data
{
    public class TemplateData
    {
        public Color TemplateColor { get; set; }
        public int SubTypeCode { get; set; }
        public string SubTypeName { get; set; }

        public TemplateData()
        {

        }

        public TemplateData(Color templateColor, int subTypeCode, string subTypeName)
        {
            TemplateColor = templateColor;
            SubTypeCode = subTypeCode;
            SubTypeName = subTypeName;
        }
    }
}
