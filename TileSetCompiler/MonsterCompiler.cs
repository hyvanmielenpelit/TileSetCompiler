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
        const string _unknownStatueFileName = "UnknownMonsterStatue.png";
        const int _monsterLineLength = 4;
        const string _type_normal = "normal";
        const string _type_statue = "statue";
        const string _statueDirName = "statues";
        const string _normalDirName = "normal";

        private Dictionary<string, string> _genderSuffix = new Dictionary<string, string>()
        {
            { "male", "_male" },
            { "female", "_female" },
            { "base", "" }
        };

        public static string MonsterDirectoryName { get { return _subDirName; } }
        public static string StatueDirectoryName { get { return _statueDirName; } }

        public FileInfo UnknownStatueFile { get; private set; }
        public StatueCreator StatueCreator { get; private set; }

        public MonsterCompiler(StreamWriter tileNameWriter) : base(_subDirName, _unknownMonsterFileName, tileNameWriter)
        {
            UnknownStatueFile = new FileInfo(Path.Combine(BaseDirectory.FullName, _unknownStatueFileName));

            if(!UnknownStatueFile.Exists)
            {
                throw new Exception(string.Format("Could not find Unknown Monster Statue icon '{0}'.", UnknownStatueFile.FullName));
            }

            StatueCreator = new StatueCreator(_subDirName, _unknownStatueFileName);
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
            if (type == _type_normal)
            {
                var subDir2 = Path.Combine(_normalDirName, name.ToLower());

                var monsterDirPath = Path.Combine(BaseDirectory.FullName, subDir2);
                FileInfo usedMonsterFile = null;
                var fileName = name.ToLower() + genderSuffix + Program.ImageFileExtension;
                var relativePath = Path.Combine(_subDirName, subDir2, fileName);

                if (!Directory.Exists(monsterDirPath))
                {
                    Console.WriteLine("Monster directory '{0}' not found. Using Unknown Monster icon.", monsterDirPath);
                    usedMonsterFile = UnknownFile;
                    WriteTileNameErrorDirectoryNotFound(relativePath, "Using Unknown Monster icon");
                }
                else
                {
                    var filePath = Path.Combine(monsterDirPath, fileName);
                    FileInfo file = new FileInfo(filePath);

                    if (file.Exists)
                    {
                        usedMonsterFile = file;
                        WriteTileNameSuccess(relativePath);
                    }
                    else
                    {
                        Console.WriteLine("Monster file '{0}' not found. Using Unknown Monster icon.", file.FullName);
                        usedMonsterFile = UnknownFile;
                        WriteTileNameErrorFileNotFound(relativePath, "Using Unknown Monster icon");
                    }
                }

                using (var image = new Bitmap(Image.FromFile(usedMonsterFile.FullName)))
                {
                    DrawImageToTileSet(image);
                    IncreaseCurXY();
                }
            }
            else if (type == _type_statue)
            {
                var sourceSubDir2 = Path.Combine(_normalDirName, name.ToLower());

                var sourceMonsterDirPath = Path.Combine(BaseDirectory.FullName, sourceSubDir2);
                var sourceFileName = name.ToLower() + genderSuffix + Program.ImageFileExtension;
                var sourceRelativePath = Path.Combine(_subDirName, sourceSubDir2, sourceFileName);
                FileInfo sourceFile = new FileInfo(Path.Combine(sourceMonsterDirPath, sourceFileName));

                var destDir = Program.StatueOutputDirectory ?? Program.WorkingDirectory;
                var destSubDirPath = Path.Combine(_statueDirName, name.ToLower());
                string destFileName = _type_statue + "_" + name.ToLower() + genderSuffix + Program.ImageFileExtension;
                var destFileRelativePath = Path.Combine(destSubDirPath, destFileName);

                string destDirPath = null;
                DirectoryInfo destSubDir = null;
                FileInfo destFile = null;

                if (Program.CreateStatuesOnDisk)
                {
                    destDirPath = Path.Combine(destDir.FullName, destSubDirPath);
                    destSubDir = new DirectoryInfo(destDirPath);
                    if (!destSubDir.Exists)
                    {
                        try
                        {
                            destSubDir.Create();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Could not create statue sub directory '{0}'.", destSubDir.FullName), ex);
                        }
                    }
                    destFile = new FileInfo(Path.Combine(destSubDir.FullName, destFileName));

                    if (destFile.Exists)
                    {
                        try
                        {
                            destFile.Decrypt();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(string.Format("Could not delete statue file '{0}'.", destFile.FullName), ex);
                        }
                    }

                    bool isUnknown = false;
                    StatueCreator.CreateStatue(sourceFile, destFile, out isUnknown);

                    if(isUnknown)
                    {
                        Console.WriteLine("Monster statue file '{0}' not found. Using Unknown Monster Statue icon.", sourceFile.FullName);
                    }
                    else
                    {
                        Console.WriteLine("Created statue from '{0}' to '{1}'.", sourceFile.FullName, destFile.FullName);
                    }
                }

                if(Program.ReadStatuesFromDisk)
                {
                    FileInfo usedStatueFile = destFile;
                    if(destFile.Exists)
                    {
                        WriteTileNameStatueSuccessDisk(sourceRelativePath, destFileRelativePath);
                    }
                    else
                    {
                        Console.WriteLine("Monster statue file '{0}' not found. Using Unknown Monster Statue icon.", destFile.FullName);
                        usedStatueFile = UnknownStatueFile;
                        WriteTileNameErrorFileNotFound(sourceRelativePath, "Using Unknown Monster Statue icon");
                    }
                    using (var image = new Bitmap(Image.FromFile(usedStatueFile.FullName)))
                    {
                        DrawImageToTileSet(image);
                        IncreaseCurXY();
                    }
                }

                if(!Program.CreateStatuesOnDisk && !Program.ReadStatuesFromDisk)
                {
                    //Create and read to memory
                    bool isUnknown;
                    using (var image = StatueCreator.CreateStatueBitmapFromFile(sourceFile, out isUnknown))
                    {
                        if(!isUnknown)
                        {
                            WriteTileNameStatueSuccessMemory(sourceRelativePath, destFileRelativePath);
                        }
                        else
                        {
                            Console.WriteLine("Monster statue file '{0}' not found. Using Unknown Monster Statue icon.", sourceFile.FullName);
                            WriteTileNameErrorFileNotFound(sourceRelativePath, "Using Unknown Monster Statue icon");
                        }
                        DrawImageToTileSet(image);
                        IncreaseCurXY();
                    }
                }
            }
        }        
    }
}
