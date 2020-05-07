using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace TileSetCompiler
{
    class MonsterCompiler : BitmapCompiler
    {
        const string _subDirName = "Monsters";
        const string _manifestFile = "monsters.csv";
        const string _maleSuffix = "_male";
        const string _femaleSuffix = "_female";
        const string _unknownMonsterFileName = "UnknownMonster.png";

        public MonsterCompiler() : base(_subDirName, _manifestFile, _unknownMonsterFileName)
        {

        }

        public override int GetTileNumber()
        {
            if (Manifest == null || !Manifest.Exists)
            {
                throw new Exception("Monsters Manifest File not found. Cannot compile monsters.");
            }

            int monsterNumber = 0;

            using (var stream = Manifest.OpenText())
            {
                string? line = null;
                while ((line = stream.ReadLine()) != null)
                {
                    var splitLine = line.Split(',');
                    var name = splitLine[0];
                    if (name.ToLower() != "name")
                    {
                        monsterNumber++;
                    }
                }
                stream.Close();
            }

            int monsterNumberWithGenders = monsterNumber * 2;

            return monsterNumberWithGenders;
        }


        public override void Compile()
        {
            if(BaseDirectory == null || !BaseDirectory.Exists)
            {
                throw new Exception("Monsters Manifest File not found. Cannot compile monsters.");
            }
            if(Manifest == null || !Manifest.Exists)
            {
                throw new Exception("Monsters Manifest File not found. Cannot compile monsters.");
            }
            if(UnknownFile == null || !UnknownFile.Exists)
            {
                throw new Exception("Unknown Monsters File not found. Cannot compile monsters.");
            }

            using (var stream = Manifest.OpenText())
            {
                string? line = null;
                while((line = stream.ReadLine()) != null)
                {
                    var splitLine = line.Split(',');
                    var name = splitLine[0];
                    if (name.ToLower() != "name")
                    {
                        //Real monster line, skip headers
                        var monsterDirPath = Path.Combine(BaseDirectory.FullName, name);
                        FileInfo usedMaleMonsterFile = null;
                        FileInfo usedFemaleMonsterFile = null;
                        if (!Directory.Exists(monsterDirPath))
                        {
                            Console.WriteLine("Monster directory '{0}' not found. Using Unknown Monster icon for both male and female.", monsterDirPath);
                            usedMaleMonsterFile = UnknownFile;
                            usedFemaleMonsterFile = UnknownFile;
                        }
                        else
                        {
                            var maleFileName = name + _maleSuffix + Program.ImageFileExtension;
                            var femaleFileName = name + _femaleSuffix + Program.ImageFileExtension;
                            var commonFileName = name + Program.ImageFileExtension;
                            var maleFilePath = Path.Combine(monsterDirPath, maleFileName);
                            var femaleFilePath = Path.Combine(monsterDirPath, femaleFileName);
                            var commonFilePath = Path.Combine(monsterDirPath, commonFileName);
                            FileInfo commonFile = new FileInfo(commonFilePath);
                            FileInfo maleFile = new FileInfo(maleFilePath);
                            FileInfo femaleFile = new FileInfo(femaleFilePath);
                            
                            if(maleFile.Exists)
                            {
                                usedMaleMonsterFile = maleFile;
                            }
                            else if (commonFile.Exists)
                            {
                                usedMaleMonsterFile = commonFile;
                            }
                            else
                            {
                                usedMaleMonsterFile = UnknownFile;
                            }

                            if (femaleFile.Exists)
                            {
                                usedFemaleMonsterFile = femaleFile;
                            }
                            else if (commonFile.Exists)
                            {
                                usedFemaleMonsterFile = commonFile;
                            }
                            else
                            {
                                usedFemaleMonsterFile = UnknownFile;
                            }
                        }

                        using (var maleImage = new Bitmap(Image.FromFile(usedMaleMonsterFile.FullName)))
                        {
                            DrawImageToTileSet(maleImage);
                            IncreaseCurXY();
                        }

                        using (var femaleImage = new Bitmap(Image.FromFile(usedFemaleMonsterFile.FullName)))
                        {
                            DrawImageToTileSet(femaleImage);
                            IncreaseCurXY();
                        }
                    }

                }

                stream.Close();
            }
        }        
    }
}
