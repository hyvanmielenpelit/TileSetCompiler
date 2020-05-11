using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace TileSetCompiler
{
    class TileCompiler : IDisposable
    {
        const string _manifestFile = "tile_definition.csv";
        const string _objectType_monster = "monsters";
        const string _objectType_object = "objects";
        const string _objectType_artifact = "artifacts";
        const string _objectType_cmap = "cmap";
        const string _objectType_misc = "misc";
        const string _objectType_player = "player";

        public FileInfo Manifest { get; private set; }
        public DirectoryInfo BaseDirectory { get { return Program.InputDirectory; } }

        protected MonsterCompiler MonsterCompiler { get; private set; }
        protected ObjectCompiler ObjectCompiler { get; private set; }
        protected ArtifactCompiler ArtifactCompiler { get; private set; }
        protected CmapCompiler CmapCompiler { get; private set; }
        protected MiscCompiler MiscCompiler { get; private set; }
        protected PlayerCompiler PlayerCompiler { get; private set; }

        public FileInfo TileNameFile { get; set; }
        public StreamWriter TileNameWriter { get; private set; }

        public TileCompiler()
        {
            string tileNameFilePath = Path.Combine(Program.InputDirectory.FullName, Program.TileNameOutputFileName);
            TileNameFile = new FileInfo(tileNameFilePath);
            if (TileNameFile.Exists)
            {
                try
                {
                    TileNameFile.Delete();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error deleting TileNameFile '{0}'.", TileNameFile.FullName), ex);
                }

            }
            
            try
            {
                TileNameWriter = new StreamWriter(TileNameFile.FullName);
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Unable to create StreamWriter for file '{0}'.", TileNameFile.FullName), ex);
            }

            MonsterCompiler = new MonsterCompiler(TileNameWriter);
            ObjectCompiler = new ObjectCompiler(TileNameWriter);
            ArtifactCompiler = new ArtifactCompiler(TileNameWriter);
            CmapCompiler = new CmapCompiler(TileNameWriter);
            MiscCompiler = new MiscCompiler(TileNameWriter);
            PlayerCompiler = new PlayerCompiler(TileNameWriter);

            string manifestPath = Path.Combine(BaseDirectory.FullName, _manifestFile);
            Manifest = new FileInfo(manifestPath);

            if (!BaseDirectory.Exists)
            {
                throw new Exception(string.Format("Base Directory '{0}' does not exist.", BaseDirectory.FullName));
            }

            if (!Manifest.Exists)
            {
                throw new Exception(string.Format("Manifest File '{0}' not found.", Manifest.FullName));
            }

            
        }

        public int GetTileNumber()
        {
            int lineNumber = 0;

            using (var stream = Manifest.OpenText())
            {
                string? line = null;
                while ((line = stream.ReadLine()) != null)
                {
                    lineNumber++;
                }
                stream.Close();
            }

            return lineNumber;
        }


        public void Compile()
        {
            using (var stream = Manifest.OpenText())
            {
                string line = null;
                while ((line = stream.ReadLine()) != null)
                {
                    var splitLine = line.Split(',');
                    var objectType = splitLine[0];

                    if (objectType == _objectType_monster)
                    {
                        MonsterCompiler.CompileOne(splitLine);
                    }
                    else if (objectType == _objectType_object)
                    {
                        ObjectCompiler.CompileOne(splitLine);
                    }
                    else if (objectType == _objectType_artifact)
                    {
                        ArtifactCompiler.CompileOne(splitLine);
                    }
                    else if (objectType == _objectType_cmap)
                    {
                        CmapCompiler.CompileOne(splitLine);
                    }
                    else if (objectType == _objectType_misc)
                    {
                        MiscCompiler.CompileOne(splitLine);
                    }
                    else if (objectType == _objectType_player)
                    {
                        PlayerCompiler.CompileOne(splitLine);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unknown object type '{0}' in line '{1}'.", objectType, line));
                    }
                }

                // Close Manifest Steram
                stream.Close();
            }
        }

        public void Close()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (TileNameWriter != null)
            {
                TileNameWriter.Close();
            }
        }
    }
}
