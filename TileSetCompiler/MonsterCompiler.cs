using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace TileSetCompiler
{
    class MonsterCompiler
    {
        const string _manifestFile = "monsters.csv";
        const string _maleSuffix = "_male";
        const string _femaleSuffix = "_female";
        const string _unknownMonsterFileName = "UnknownMonster.png";
        const string _outputFileName = "monsters.bmp";

        public MonsterCompiler()
        {
            var dirs = Program.WorkingDirectory.GetDirectories("Monsters");
            if(dirs.Length == 0)
            {
                Console.WriteLine("Monsters directory '{0}' not found.", Program.WorkingDirectory.FullName + "\\" + "Monsters");
            }
            else 
            {
                BaseDirectory = dirs[0];
                var files = BaseDirectory.GetFiles(_manifestFile);
                if (files.Length == 0)
                {
                    Console.WriteLine("Manifest file '{0}' not found in directory '{1}'.", _manifestFile, BaseDirectory.FullName);
                }
                else
                {
                    Manifest = files[0];
                }

                UnknownMonsterFile = new FileInfo(Path.Combine(BaseDirectory.FullName, _unknownMonsterFileName));
                if(!UnknownMonsterFile.Exists)
                {
                    Console.WriteLine("Unknown monster file '{0}' not found in directory '{1}'.", _unknownMonsterFileName, BaseDirectory.FullName);
                }

                OutputFile = new FileInfo(Path.Combine(Program.OutputDirectory.FullName, _outputFileName));
            }

        }

        public DirectoryInfo BaseDirectory { get; set; }
        public Bitmap TileSet { get; set; }
        public FileInfo Manifest { get; set; }
        public FileInfo UnknownMonsterFile { get; set; }

        public FileInfo OutputFile { get; set; }

        public void Compile()
        {
            if(BaseDirectory == null || !BaseDirectory.Exists)
            {
                Console.WriteLine("Monsters Base Directory not found. Cannot compile monsters.");
                return;
            }
            if(Manifest == null || !Manifest.Exists)
            {
                Console.WriteLine("Monsters Manifest File not found. Cannot compile monsters.");
                return;
            }
            if(UnknownMonsterFile == null || !UnknownMonsterFile.Exists)
            {
                Console.WriteLine("Unknown Monsters File not found. Cannot compile monsters.");
                return;
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

            int bitmapSideNumber = (int)Math.Ceiling(Math.Sqrt(monsterNumberWithGenders));
            int bitMapWidth = Program.TileSize.Width * bitmapSideNumber;
            int bitMapHeight = Program.TileSize.Height * bitmapSideNumber;

            TileSet = new Bitmap(bitMapWidth, bitMapHeight);
            int curX = 0, maxX = bitmapSideNumber - 1;
            int curY = 0, maxY = bitmapSideNumber - 1;

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
                            usedMaleMonsterFile = UnknownMonsterFile;
                            usedFemaleMonsterFile = UnknownMonsterFile;
                        }
                        else
                        {
                            var maleFileName = name + _maleSuffix + Program.ImageFileExtension;
                            var femaleFileName = name + _maleSuffix + Program.ImageFileExtension;
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
                                usedMaleMonsterFile = UnknownMonsterFile;
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
                                usedFemaleMonsterFile = UnknownMonsterFile;
                            }
                        }

                        using (var maleImage = new Bitmap(Image.FromFile(usedMaleMonsterFile.FullName)))
                        {
                            DrawImageToTileSet(maleImage, curX, curY);

                            IncreaseCurXY(ref curX, ref curY, maxX, maxY);
                        }

                        using (var femaleImage = new Bitmap(Image.FromFile(usedFemaleMonsterFile.FullName)))
                        {

                            DrawImageToTileSet(femaleImage, curX, curY);

                            IncreaseCurXY(ref curX, ref curY, maxX, maxY);
                        }
                    }

                }

                stream.Close();
            }

            if(OutputFile.Exists)
            {
                OutputFile.Delete();
            }

            TileSet.Save(OutputFile.FullName);

        }

        private void IncreaseCurXY(ref int curX, ref int curY, int maxX, int maxY)
        {
            curX++;
            if (curX > maxX)
            {
                curX = 0;
                curY++;
            }
            if (curY > maxY)
            {
                Console.WriteLine("curY '{0}' is greater than maxY '{1}'.", curY, maxY);
                throw new Exception("Aborting.");
            }
        }

        private void DrawImageToTileSet(Bitmap bmp, int tileX, int tileY)
        {
            int tileSetX = 0, tileSetY = 0;
            for(int x = 0; x < bmp.Width; x++)
            {
                for(int y = 0; y < bmp.Height; y++)
                {
                    tileSetX = tileX * Program.TileSize.Width + x;
                    tileSetY = tileY * Program.TileSize.Height + y;
                    Color c = bmp.GetPixel(x, y);
                    TileSet.SetPixel(tileSetX, tileSetY, c);
                }
            }
        }
    }
}
