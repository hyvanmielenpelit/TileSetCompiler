using System;
using System.Collections.Generic;
using System.Text;

namespace TileSetCompiler.Data
{
    class CategoryInfo
    {
        public string Suffix { get; set; }
        public string Description { get; set; }

        public CategoryInfo()
        {

        }

        public CategoryInfo(string suffix, string description)
        {
            Suffix = suffix;
            Description = description;
        }
    }
}
