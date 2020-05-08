using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace TileSetCompiler
{
    class MonsterCompiler : BitmapCompiler
    {
        const string _subDirName = "Monsters";
        const string _unknownMonsterFileName = "UnknownMonster.png";
        const int _monsterLineLength = 4;

        private Dictionary<string, string> _genderSuffix = new Dictionary<string, string>()
        {
            { "male", "_male" },
            { "female", "_female" },
            { "base", "" }
        };

        public MonsterCompiler() : base(_subDirName, _unknownMonsterFileName)
        {

        }

        public override void CompileOne(string[] splitLine)
        {
            if(splitLine.Length < _monsterLineLength)
            {
                throw new Exception(string.Format("Monster line '{0}' has too few elements.", string.Join(',', splitLine)));
            }

            var gender = splitLine[1];
            if(!_genderSuffix.ContainsKey(gender))
            {
                throw new Exception(string.Format("Invalid gender '{0}' in monster line '{1}'.", gender, string.Join(',', splitLine)));
            }
            string genderSuffix = _genderSuffix[gender];

            var type = splitLine[2];
            var name = splitLine[3];

            var monsterDirPath = Path.Combine(BaseDirectory.FullName, name.ToLower());
            FileInfo usedMonsterFile = null;
            if (!Directory.Exists(monsterDirPath))
            {
                Console.WriteLine("Monster directory '{0}' not found. Using Unknown Monster icon.", monsterDirPath);
                usedMonsterFile = UnknownFile;
            }
            else
            {
                var fileName = name.ToLower() + genderSuffix + Program.ImageFileExtension;
                var filePath = Path.Combine(monsterDirPath, fileName);
                FileInfo file = new FileInfo(filePath);
                            
                if(file.Exists)
                {
                    usedMonsterFile = file;
                }
                else
                {
                    Console.WriteLine("Monster file '{0}' not found. Using Unknown Monster icon.", file.FullName);
                    usedMonsterFile = UnknownFile;
                }
            }

            using (var image = new Bitmap(Image.FromFile(usedMonsterFile.FullName)))
            {
                DrawImageToTileSet(image);
                IncreaseCurXY();
            }                    
        }        
    }
}
