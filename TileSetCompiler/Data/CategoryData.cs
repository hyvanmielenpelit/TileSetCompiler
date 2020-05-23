using System;
using System.Collections.Generic;
using System.Text;

namespace TileSetCompiler.Data
{
    class CategoryData
    {
        public string Suffix { get; set; }
        public string Description { get; set; }

        public CategoryData()
        {

        }

        public CategoryData(string suffix, string description)
        {
            Suffix = suffix;
            Description = description;
        }
    }
}
