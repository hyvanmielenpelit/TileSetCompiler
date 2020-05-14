using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace TileSetCompiler.Extensions
{
    public static class StringExtensions
    {
        public static string ToProperCaseFirst(this string s)
        {
            if(s.Length > 0)
            {
                return s[0].ToString().ToUpper() + s.Substring(1);
            }
            return s;
        }

        public static string ToProperCase(this string s)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var split in s.Split(' '))
            {
                if(sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(split.ToProperCaseFirst());
            }
            return sb.ToString();
        }
    }
}
