using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace TileSetCompiler
{
    class Program
    {
        private static Size _tileSize = new Size(64, 96);
        private static List<int> _tileHeights = new List<int>( new int[] { 96, 72, 48, 36, 24, 18 } );
        private static string _defaultOutputFileName = "gnollhack";
        private static string _tileNameSuffix = "_tilenames";
        private static string _tileNameExtension = ".txt";

        private static Dictionary<int, Size> _tileSizes = new Dictionary<int, Size>()
        {
            { 96, new Size (64, 96) },
            { 72, new Size (48, 72) },
            { 48, new Size (32, 48) },
            { 36, new Size (24, 36) },
            { 24, new Size (16, 24) },
            { 18, new Size (12, 18) }
        };

        public static DirectoryInfo WorkingDirectory { get; set; }
        public static Dictionary<int, Bitmap> TileSets { get; set; }
        public static string ImageFileExtension { get { return ".png"; } }
        public static Size MaxTileSize { get { return _tileSize; } }
        public static Dictionary<int, Size> TileSizes { get { return _tileSizes; } }
        public static DirectoryInfo OutputDirectory { get; set; }
        public static Dictionary<int, FileInfo> OutputFiles { get; set; }
        public static string OutputFileName { get; set; }
        public static string OutputFileExtension { get { return ".bmp"; } }
        public static string TileNameOutputFileName { get; set; }
        public static int TileNumber { get; set; }
        public static int CurX { get; set; }
        public static int CurY { get; set; }
        public static int MaxX { get; set; }
        public static int MaxY { get; set; }
        protected static TileCompiler TileCompiler { get; set; }

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
                    Console.ReadKey();
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
                        Console.ReadKey();
                        return;
                    }
                }
            }
            else
            {
                OutputDirectory = WorkingDirectory;
            }

            if (args.Length >= 3)
            {
                OutputFileName = args[2];
            }
            else
            {
                OutputFileName = _defaultOutputFileName;
            }
            TileNameOutputFileName = OutputFileName + _tileNameSuffix + _tileNameExtension;

            InitializeOutputFiles();

            using (TileCompiler = new TileCompiler())
            {
                try
                {
                    InitializeTileSets();

                    TileCompiler.Compile();
                    TileCompiler.Close();

                    SaveFiles();

                    Console.WriteLine("Finished.");
                }
                catch(Exception ex)
                {
                    //Error occurred
                    Console.WriteLine(string.Format("Exception occurred: {0}", ex.Message));
                    if(ex.InnerException != null)
                    {
                        Console.WriteLine(string.Format("Inner Exception: {0}", ex.InnerException.Message));
                    }
                    Console.WriteLine("Exiting.");
                }
                finally
                {
                    Console.ReadKey();
                }
            }
        }

        private static void SaveFiles()
        {
            foreach (var kvp in TileSets)
            {
                var tileHeight = kvp.Key;
                var tileSet = kvp.Value;
                var outputFile = OutputFiles[tileHeight];

                try
                {
                    if (outputFile.Exists)
                    {
                        outputFile.Delete();
                    }

                    tileSet.Save(outputFile.FullName);
                }
                catch (Exception ex)
                {
                    throw new Exception("Saving output file '" + outputFile.FullName + "' failed.", ex);
                }   
            }            
        }

        protected static void InitializeTileSets()
        {
            TileNumber += TileCompiler.GetTileNumber();

            int bitmapSideNumber = (int)Math.Ceiling(Math.Sqrt(TileNumber));
            CurX = 0;
            MaxX = bitmapSideNumber - 1;
            CurY = 0;
            MaxY = bitmapSideNumber - 1;

            TileSets = new Dictionary<int, Bitmap>();
            foreach (int tileheight in _tileHeights)
            {
                int bitMapWidth = Program.TileSizes[tileheight].Width * bitmapSideNumber;
                int bitMapHeight = Program.TileSizes[tileheight].Height * bitmapSideNumber;

                TileSets.Add(tileheight, new Bitmap(bitMapWidth, bitMapHeight));
            }
        }

        protected static void InitializeOutputFiles()
        {
            OutputFiles = new Dictionary<int, FileInfo>();

            foreach (int height in _tileHeights)
            {
                int width = height / 3 * 2;
                string filename = string.Format("{0}{1}x{2}{3}", OutputFileName, width, height, OutputFileExtension);
                OutputFiles.Add(height, new FileInfo(Path.Combine(OutputDirectory.FullName, filename)));
            }
        }
    }
}
