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
        const string _tileType_monster = "monsters";
        const string _tileType_object = "objects";
        const string _tileType_artifact = "artifacts";
        const string _tileType_cmap = "cmap";
        const string _tileType_misc = "misc";
        const string _tileType_player = "player";
        const string _tileType_cmap_variation = "cmap-variations";
        const string _tileType_UI = "user-interface";
        const string _tileType_animation = "animation";
        const string _tileType_enlargement = "enlargement";

        public FileInfo Manifest { get; private set; }
        public DirectoryInfo BaseDirectory { get { return Program.InputDirectory; } }

        protected MonsterCompiler MonsterCompiler { get; private set; }
        protected ObjectCompiler ObjectCompiler { get; private set; }
        protected ArtifactCompiler ArtifactCompiler { get; private set; }
        protected CmapCompiler CmapCompiler { get; private set; }
        protected CmapVariationCompiler CmapVariationCompiler { get; private set; }
        protected MiscCompiler MiscCompiler { get; private set; }
        protected PlayerCompiler PlayerCompiler { get; private set; }
        protected UICompiler UICompiler { get; private set; }
        protected AnimationCompiler AnimationCompiler { get; private set; }
        protected EnlargementCompiler EnlargementCompiler { get; private set; }

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
                TileNameWriter = new StreamWriter(new FileStream(TileNameFile.FullName, FileMode.CreateNew, FileAccess.Write, FileShare.None));
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Unable to create StreamWriter for file '{0}'.", TileNameFile.FullName), ex);
            }

            MonsterCompiler = new MonsterCompiler(TileNameWriter);
            ObjectCompiler = new ObjectCompiler(TileNameWriter);
            ArtifactCompiler = new ArtifactCompiler(TileNameWriter);
            CmapCompiler = new CmapCompiler(TileNameWriter);
            CmapVariationCompiler = new CmapVariationCompiler(TileNameWriter);
            MiscCompiler = new MiscCompiler(TileNameWriter);
            PlayerCompiler = new PlayerCompiler(TileNameWriter);
            UICompiler = new UICompiler(TileNameWriter);
            AnimationCompiler = new AnimationCompiler(TileNameWriter);
            EnlargementCompiler = new EnlargementCompiler(TileNameWriter);

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
                string line = null;
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
                    var tileType = splitLine[0];

                    if (tileType == _tileType_monster)
                    {
                        MonsterCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_object)
                    {
                        ObjectCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_artifact)
                    {
                        ArtifactCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_cmap)
                    {
                        CmapCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_misc)
                    {
                        MiscCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_player)
                    {
                        PlayerCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_cmap_variation)
                    {
                        CmapVariationCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_UI)
                    {
                        UICompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_animation)
                    {
                        AnimationCompiler.CompileOne(splitLine);
                    }
                    else if (tileType == _tileType_enlargement)
                    {
                        EnlargementCompiler.CompileOne(splitLine);
                    }
                    else
                    {
                        throw new Exception(string.Format("Unknown tile type '{0}' in line '{1}'.", tileType, line));
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
