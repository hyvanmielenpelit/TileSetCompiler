﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Schema;
using TileSetCompiler.Creators;
using TileSetCompiler.Data;
using TileSetCompiler.Exceptions;

namespace TileSetCompiler
{
    public enum TransparencyMode { Color, Real };

    class Program
    {
        private static Size _tileSize = new Size(64, 96);
        private static Size _itemSize = new Size(64, 48);
        private static List<int> _tileHeights = new List<int>(new int[] { 96 }); //, 72, 48, 36, 24, 18
        private static string _tileNameSuffix = "_tilenames";
        private static string _tileNameExtension = ".txt";
        private static Dictionary<TransparencyMode, string> _transparencyModeSuffix = new Dictionary<TransparencyMode, string>()
        {
            { TransparencyMode.Color, "_colored" },
            { TransparencyMode.Real, "_transparent" }
        };
        private static Dictionary<BitDepth, string> _bitDepthSuffix = new Dictionary<BitDepth, string>()
        {
            { BitDepth.BitDepth32, "_32bits" },
            { BitDepth.BitDepth24, "_24bits" }
        };

        private static List<OutputFileFormatData> _outputFileFormats = new List<OutputFileFormatData>()
        {
            new OutputFileFormatData() { Extension = ".png", TransparencyMode= TransparencyMode.Real, BitDepth = BitDepth.BitDepth32 },
            new OutputFileFormatData() { Extension = ".png", TransparencyMode= TransparencyMode.Color, BitDepth = BitDepth.BitDepth32 },
        };

        private static Dictionary<int, Size> _tileSizes = new Dictionary<int, Size>()
        {
            { 96, new Size (64, 96) } //,
            //{ 72, new Size (48, 72) },
            //{ 48, new Size (32, 48) },
            //{ 36, new Size (24, 36) },
            //,{ 24, new Size (16, 24) }
            //,{ 18, new Size (12, 18) }
        };

        public static List<OutputFileFormatData> OutputFileFormats { get { return _outputFileFormats; } }
        public static DirectoryInfo InputDirectory { get; set; }
        public static Size TileSetSize { get; set; }
        public static Dictionary<int, Dictionary<OutputFileFormatData, Bitmap>> TileSets { get; set; }
        public static string ImageFileExtension { get { return ".png"; } }
        public static Size MaxTileSize { get { return _tileSize; } }
        public static Size ItemSize { get { return _itemSize; } }
        public static Dictionary<int, Size> TileSizes { get { return _tileSizes; } }
        public static DirectoryInfo OutputDirectory { get; set; }
        public static Dictionary<int, Dictionary<OutputFileFormatData, FileInfo>> OutputFiles { get; set; }
        public static string OutputFileName { get; set; }
        public static List<string> OutputFileExtensions { get; set; }
        public static string TileNameOutputFileName { get; set; }
        public static int TileNumber { get; set; }
        public static int FoundTileNumber { get; set; }
        public static int MissingTileNumber { get; set; }
        public static int AutoGeneratedTileNumber { get; set; }
        public static int AutoGeneratedMissingTileNumber { get; set; }
        public static int CurX { get; set; }
        public static int CurY { get; set; }
        public static int MaxX { get; set; }
        public static int MaxY { get; set; }
        public static int CurrentCount { get; set; }
        public static Dictionary<int, TileData> TileFileData { get; set; }

        protected static TileCompiler TileCompiler { get; set; }

        static void Main(string[] args)
        {
            //TestCode();
            //return;

            //-----------------------------------------------------------
            // First argument is input directory
            //-----------------------------------------------------------

            if (args.Length == 0)
            {
                Console.WriteLine("Too few arguments. The first argument must be the input directory.");
                Console.ReadKey();
                return;
            }

            try
            {
                DirectoryInfo[] dirs = new DirectoryInfo[3];
                dirs[0] = new DirectoryInfo(args[0]);
                dirs[1] = new DirectoryInfo(args[0].Replace("Jaetut Drivet", "Shared drives"));
                dirs[2] = new DirectoryInfo(args[0].Replace("Shared drives", "Jaetut Drivet"));
                DirectoryInfo usedDir = null;
                foreach(var dir in dirs)
                {
                    usedDir = dir;
                    if(usedDir.Exists)
                    {
                        break;
                    }
                }
                if (!usedDir.Exists)
                {
                    Console.WriteLine("Input directories '{0}' does not exist.", string.Join<DirectoryInfo>(", ", dirs));
                    Console.ReadKey();
                    return;
                }
                Console.WriteLine("Found Directory '{0}'.", args[0]);
                Program.InputDirectory = usedDir;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input directory '{0}' is invalid.", args[0]);
                Console.WriteLine(ex.Message);
                Console.ReadKey();
                return;
            }


            //-----------------------------------------------------------
            // Second argument is output directory
            //-----------------------------------------------------------

            if (args.Length < 2)
            {
                Console.WriteLine("Too few arguments. The second argument must be the output directory.");
                Console.ReadKey();
                return;
            }

            OutputDirectory = new DirectoryInfo(args[1]);

            if (!OutputDirectory.Exists)
            {
                try
                {
                    OutputDirectory.Create();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating directory '{0}':", OutputDirectory.FullName);
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                    return;
                }
            }

            //-----------------------------------------------------------
            // Third argument is the output file name without extension
            //-----------------------------------------------------------

            if (args.Length < 3)
            {
                Console.WriteLine("Too few arguments. The third argument must be the output file name (without extension).");
                Console.ReadKey();
                return;
            }

            OutputFileName = args[2];

            //This is the name of the file, where the program writes the names of all tiles in the tileset
            TileNameOutputFileName = OutputFileName + _tileNameSuffix + _tileNameExtension;

            InitializeOutputFiles();

            TileFileData = new Dictionary<int, TileData>();

            using (TileCompiler = new TileCompiler())
            {
                try
                {
                    InitializeTileSets();

                    TileCompiler.Compile();
                    TileCompiler.Close();

                    SaveFiles();

                    Console.WriteLine();
                    Console.WriteLine("Source Directory: {0}", InputDirectory.FullName);
                    Console.WriteLine("Target Directory: {0}", OutputDirectory.FullName);
                    Console.WriteLine();
                    Console.WriteLine("Total Tiles: {0}", TileNumber);
                    Console.WriteLine("Found Tiles: {0}", FoundTileNumber);
                    Console.WriteLine("Missing Tiles: {0}", MissingTileNumber);
                    Console.WriteLine("Auto-Generated Tiles: {0}", AutoGeneratedTileNumber);
                    Console.WriteLine("Missing Auto-Generated Tiles: {0}", AutoGeneratedMissingTileNumber);
                    Console.WriteLine("Output Bitmap Size: {0}x{1} tiles, {2}x{3} pixels.", TileSetSize.Width, TileSetSize.Height,
                        TileSetSize.Width * _tileSize.Width, TileSetSize.Height * _tileSize.Height);
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
                var outputFormats = OutputFiles[tileHeight];
                foreach (var kvp2 in outputFormats)
                {
                    var outputFormat = kvp2.Key;
                    var outputFile = kvp2.Value;
                    try
                    {
                        if (outputFile.Exists)
                        {
                            outputFile.Delete();
                        }

                        tileSet[kvp2.Key].Save(outputFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Saving output file '" + outputFile.FullName + "' failed.", ex);
                    }
                }
            }            
        }

        protected static void InitializeTileSets()
        {
            TileNumber += TileCompiler.GetTileNumber();

            int widthInTiles = (int)Math.Ceiling(Math.Sqrt(TileNumber * 1.5d));
            // Make divisible by 3
            if (widthInTiles % 3 > 0)
            {
                widthInTiles += 3 - widthInTiles % 3;
            }
            int heightInTiles = widthInTiles / 3 * 2;

            CurX = 0;
            MaxX = widthInTiles - 1;
            CurY = 0;
            MaxY = heightInTiles - 1;
            TileSetSize = new Size(widthInTiles, heightInTiles);

            TileSets = new Dictionary<int, Dictionary<OutputFileFormatData, Bitmap>>();
            foreach (int tileheight in _tileHeights)
            {
                int bitMapWidth = Program.TileSizes[tileheight].Width * widthInTiles;
                int bitMapHeight = Program.TileSizes[tileheight].Height * heightInTiles;

                var dic = new Dictionary<OutputFileFormatData, Bitmap>();
                foreach(var outputFormat in OutputFileFormats)
                {
                    Bitmap bmp = null;
                    if(outputFormat.BitDepth == BitDepth.BitDepth32)
                    {
                        bmp = new Bitmap(bitMapWidth, bitMapHeight);
                    }
                    else
                    {
                        bmp = new Bitmap(bitMapWidth, bitMapHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    }
                        
                    dic.Add(outputFormat, bmp);
                }

                TileSets.Add(tileheight, dic);
            }
        }

        protected static void InitializeOutputFiles()
        {
            OutputFiles = new Dictionary<int, Dictionary<OutputFileFormatData, FileInfo>>();

            foreach (int height in _tileHeights)
            {
                int width = height / 3 * 2;
                var outputFormats = new Dictionary<OutputFileFormatData, FileInfo>();
                foreach (var outputFileFormat in OutputFileFormats)
                {
                    var outputTransparencySuffix = _transparencyModeSuffix[outputFileFormat.TransparencyMode];
                    var bitDepthSuffix = _bitDepthSuffix[outputFileFormat.BitDepth];
                    var fileExtension = outputFileFormat.Extension;
                    string filename = string.Format("{0}_{1}x{2}{3}{4}{5}", OutputFileName, width, height, outputTransparencySuffix, bitDepthSuffix, fileExtension);
                    outputFormats.Add(outputFileFormat, new FileInfo(Path.Combine(OutputDirectory.FullName, filename)));
                }
                OutputFiles.Add(height, outputFormats);
            }
        }

        public static Point GetMainTileLocation(int widthInTiles, int heightInTiles, MainTileAlignment mainTileAlignment, out bool isOneTile)
        {
            int xTile = 0;
            int yTile = 0;
            isOneTile = false;
            if (widthInTiles == 1 && heightInTiles == 1)
            {
                isOneTile = true;
                return Point.Empty;
            }
            else if (widthInTiles == 1 && heightInTiles == 2)
            {
                xTile = 0;
                yTile = 1;
            }
            else if (widthInTiles == 2)
            {
                if (mainTileAlignment == MainTileAlignment.Left)
                {
                    xTile = 0;
                }
                else
                {
                    xTile = 1;
                }
                if (heightInTiles == 1)
                {
                    yTile = 0;
                }
                else if (heightInTiles == 2)
                {
                    yTile = 1;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (widthInTiles == 3)
            {
                xTile = 1;
                if (heightInTiles == 1)
                {
                    yTile = 0;
                }
                else if (heightInTiles == 2)
                {
                    yTile = 1;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

            return new Point(xTile, yTile);
        }

        public static Point GetMainTileLocationInPixels(int widthInTiles, int heightInTiles, MainTileAlignment mainTileAlignment, out bool isOneTile)
        {
            var pointInTiles = GetMainTileLocation(widthInTiles, heightInTiles, mainTileAlignment, out isOneTile);
            return new Point(pointInTiles.X * MaxTileSize.Width, pointInTiles.Y * MaxTileSize.Height);
        }

        public static Point GetEnlargementTileLocation(EnlargementTilePosition tilePosition, int enlargementWidthInTiles, int enlargementHeightInTiles, MainTileAlignment mainTileAlignment)
        {
            int xTile = 0;
            int yTile = 0;            

            if (tilePosition == EnlargementTilePosition.TopLeft)
            {
                xTile = 0;
                yTile = 0;
            }
            else if (tilePosition == EnlargementTilePosition.TopCenter)
            {
                yTile = 0;
                if (enlargementWidthInTiles == 1)
                {
                    xTile = 0;
                }
                else if (enlargementWidthInTiles == 2)
                {
                    if (mainTileAlignment == MainTileAlignment.Left)
                    {
                        xTile = 0;
                    }
                    else
                    {
                        xTile = 1;
                    }
                }
                else
                {
                    xTile = 1;
                }
            }
            else if (tilePosition == EnlargementTilePosition.TopRight)
            {
                yTile = 0;
                if (enlargementWidthInTiles == 2)
                {
                    xTile = 1;
                }
                else if (enlargementWidthInTiles == 3)
                {
                    xTile = 2;
                }
            }
            else if (tilePosition == EnlargementTilePosition.MiddleLeft)
            {
                if (enlargementHeightInTiles == 1)
                {
                    yTile = 0;
                }
                else
                {
                    yTile = 1;
                }
                xTile = 0;
            }
            else if (tilePosition == EnlargementTilePosition.MiddleRight)
            {
                if (enlargementHeightInTiles == 1)
                {
                    yTile = 0;
                }
                else
                {
                    yTile = 1;
                }
                if (enlargementWidthInTiles == 2)
                {
                    xTile = 1;
                }
                else if (enlargementWidthInTiles == 3)
                {
                    xTile = 2;
                }
            }

            return new Point(xTile, yTile);
        }

        public static Point GetEnlargementTileLocationInPixels(EnlargementTilePosition tilePosition, int enlargementWidthInTiles, int enlargementHeightInTiles, MainTileAlignment mainTileAlignment)
        {
            var tilePoint = GetEnlargementTileLocation(tilePosition, enlargementWidthInTiles, enlargementHeightInTiles, mainTileAlignment);
            int x = tilePoint.X * MaxTileSize.Width;
            int y = tilePoint.Y * MaxTileSize.Height;
            return new Point(x, y);

        }

            //public static string TestSourceFile { get { return @"G:\Jaetut Drivet\Hyvän mielen pelit projektit\GnollHack\Tileset_Test\Objects\missile\weapons\arrow\weapon_arrow_missile_middle-left.png"; } }
            //public static string TestTargetFileFormat { get { return @"C:\Users\tommi\source\GnollHackTileSetOutput\test-{0}.png"; } }

            //private static void TestCode()
            //{
            //    MissileCreator missileCreator = new MissileCreator();
            //    using (var bmp = new Bitmap(TestSourceFile))
            //    {
            //        using (var bmp2 = missileCreator.CreateMissile(bmp, MissileDirection.TopLeft))
            //        {
            //            SaveTestBitmap(bmp2, "final");
            //        }
            //    }
            //}

            //public static void SaveTestBitmap(Bitmap bmp, string suffix)
            //{
            //    string filePath = string.Format(TestTargetFileFormat, suffix);

            //    if (File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //    }

            //    bmp.Save(filePath);
            //}
        }
    }
