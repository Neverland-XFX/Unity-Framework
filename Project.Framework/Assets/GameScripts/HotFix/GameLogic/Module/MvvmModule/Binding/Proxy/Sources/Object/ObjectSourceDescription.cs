using System;
using GameLogic.Binding.Paths;

namespace GameLogic.Binding.Proxy.Sources.Object
{
    [Serializable]
    public class ObjectSourceDescription : SourceDescription
    {
        private Path path;

        public ObjectSourceDescription()
        {
            this.IsStatic = false;
        }

        public ObjectSourceDescription(Path path)
        {
            this.Path = path;
        }

        public virtual Path Path
        {
            get { return this.path; }
            set
            {
                this.path = value;
                if (this.path != null)
                    this.IsStatic = this.path.IsStatic;
            }
        }

        public override string ToString()
        {
            return this.path == null ? "Path:null" : "Path:" + this.path.ToString();
        }
    }
}