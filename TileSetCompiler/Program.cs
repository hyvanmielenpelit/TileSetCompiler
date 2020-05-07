using System;
using System.Drawing;
using System.IO;

namespace TileSetCompiler
{
    class Program
    {
        private static Size _tileSize = new Size(64, 96);

        public static DirectoryInfo WorkingDirectory { get; set; }
        public static Bitmap TileSet { get; set; }
        public static string ImageFileExtension { get { return ".png"; } }
        public static Size TileSize { get { return _tileSize; } }
        public static DirectoryInfo OutputDirectory { get; set; }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Program.WorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            }
            else
            {
                try
                {
                    var dir = new DirectoryInfo(args[0]);
                    if (!dir.Exists)
                    {
                        Console.WriteLine("Directory '{0}' does not exists.", args[0]);
                        return;
                    }
                    Console.WriteLine("Found Directory '{0}'.", args[0]);
                    Program.WorkingDirectory = dir;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Directory is invalid: " + args[0]);
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
            Directory.SetCurrentDirectory(Program.WorkingDirectory.FullName);

            if(args.Length >= 2)
            {
                OutputDirectory = new DirectoryInfo(args[1]);

                if(!OutputDirectory.Exists)
                {
                    try
                    {
                        OutputDirectory.Create();
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Error creating directory '{0}':", OutputDirectory.FullName);
                        Console.WriteLine(ex.Message);
                        return;
                    }
                }
            }
            else
            {
                OutputDirectory = WorkingDirectory;
            }

            var monsterCompiler = new MonsterCompiler();
            monsterCompiler.Compile();

            Console.WriteLine("Finished.");
            Console.ReadKey();
        }
    }
}
