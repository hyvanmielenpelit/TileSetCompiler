using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace TileSetCompiler.Creators
{
    class StatueCreator
    {
        const string _unknownFile = "UnknownStatue.png";

        public ColorMatrix GrayScaleMatrix { get; set; }
        public DirectoryInfo BaseDirectory { get; set; }
        public FileInfo UnknownFile { get; private set; }

        public StatueCreator(string subDirName = null, string unknownFileName = null)
        {
            if (string.IsNullOrEmpty(subDirName))
            {
                BaseDirectory = Program.InputDirectory;
            }
            else
            {
                BaseDirectory = new DirectoryInfo(Path.Combine(Program.InputDirectory.FullName, subDirName));
            }

            if(string.IsNullOrEmpty(unknownFileName))
            {
                UnknownFile = new FileInfo(Path.Combine(BaseDirectory.FullName, _unknownFile));
            }
            else
            {
                UnknownFile = new FileInfo(Path.Combine(BaseDirectory.FullName, unknownFileName));
            }

            if (!UnknownFile.Exists)
            {
                throw new Exception(string.Format("Unknown Statue File '{0}' not found.", UnknownFile.FullName));
            }

            float red = 0.8f;
            float green = 0.8f;
            float blue = 0.8f;

            GrayScaleMatrix = new ColorMatrix(
              new float[][]
              {
                 new float[] {red,   red,   red,   0, 0},
                 new float[] {green, green, green, 0, 0},
                 new float[] {blue,  blue,  blue,  0, 0},
                 new float[] {    0,     0,     0, 1, 0},
                 new float[] {    0,     0,     0, 0, 1}
              });

            //GrayScaleMatrix = new ColorMatrix(
            //  new float[][]
            //  {
            //     new float[] {0.30f, 0.30f, 0.30f, 0, 0},
            //     new float[] {0.59f, 0.59f, 0.59f, 0, 0},
            //     new float[] {0.11f, 0.11f, 0.11f, 0, 0},
            //     new float[] {    0,     0,     0, 1, 0},
            //     new float[] {    0,     0,     0, 0, 1}
            //  });
        }

        public void CreateStatue(FileInfo sourceFile, FileInfo destFile, out bool isUnknown)
        {
            FileInfo usedSourceFile = null;
            isUnknown = false;
            if(!sourceFile.Exists)
            {
                usedSourceFile = UnknownFile;
                isUnknown = true;
            }
            else
            {
                usedSourceFile = sourceFile;
            }
            if(!destFile.Directory.Exists)
            {
                try
                {
                    destFile.Directory.Create();
                }
                catch(Exception ex)
                {
                    throw new Exception(string.Format("Creating Directory '{0}' failed.", destFile.Directory.FullName), ex);
                }
            }

            using(Bitmap sourceBitmap = (Bitmap)Image.FromFile(usedSourceFile.FullName))
            {
                using(Bitmap destBitmap = CreateStatueBitmap(sourceBitmap))
                {
                    destBitmap.Save(destFile.FullName);
                }
            }
        }

        public Bitmap CreateStatueBitmap(Bitmap sourceBitmap)
        {
            Bitmap destBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);
            
            using (Graphics g = Graphics.FromImage(destBitmap))
            {
                ImageAttributes attr = new ImageAttributes();
                attr.SetColorMatrix(GrayScaleMatrix);

                g.DrawImage(sourceBitmap, new Rectangle(0, 0, sourceBitmap.Width, sourceBitmap.Height),
                    0, 0, sourceBitmap.Width, sourceBitmap.Height, GraphicsUnit.Pixel, attr);
            }

            return destBitmap;            
        }

        public Bitmap CreateStatueBitmapFromFile(FileInfo sourceFile, out bool isUnknown)
        {
            FileInfo usedSourceFile = null;
            isUnknown = false;
            if (!sourceFile.Exists)
            {
                Console.WriteLine("Source File '{0}' not found. Using Unknown Statue file.", sourceFile.FullName);
                usedSourceFile = UnknownFile;
                isUnknown = true;
            }
            else
            {
                usedSourceFile = sourceFile;
            }
            Bitmap sourceBitmap = (Bitmap)Image.FromFile(usedSourceFile.FullName);
            if (!isUnknown)
            {
                var statueBitmap = CreateStatueBitmap(sourceBitmap);
                sourceBitmap.Dispose();
                return statueBitmap;
            }
            else
            {
                return sourceBitmap;
            }
            
        }
    }
}
