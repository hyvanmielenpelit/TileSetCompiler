using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;

namespace TileSetCompiler
{
    class TileCompiler
    {
        const string _manifestFile = "tile_definition.csv";
        const string _objectType_monster = "monsters";
        const string _objectType_object = "objects";
        const string _objectType_artifact = "artifacts";
        const string _objectType_cmap = "cmap";

        public FileInfo Manifest { get; private set; }
        public DirectoryInfo BaseDirectory { get { return Program.WorkingDirectory; } }

        protected MonsterCompiler MonsterCompiler { get; private set; }
        protected ObjectCompiler ObjectCompiler { get; private set; }
        protected ArtifactCompiler ArtifactCompiler { get; private set; }
        protected CmapCompiler CmapCompiler { get; private set; }

        public TileCompiler()
        {
            MonsterCompiler = new MonsterCompiler();
            ObjectCompiler = new ObjectCompiler();
            ArtifactCompiler = new ArtifactCompiler();
            CmapCompiler = new CmapCompiler();

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
                string? line = null;
                while ((line = stream.ReadLine()) != null)
                {
                    var splitLine = line.Split(',');
                    var objectType = splitLine[0];

                    if(objectType == _objectType_monster)
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
                }

                stream.Close();
            }
        }        
    }
}
