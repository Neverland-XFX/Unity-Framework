using System;
using System.Text;

namespace GameLogic.Binding.Paths
{
    [Serializable]
    public struct PathToken
    {
        private Path path;
        private int index;
        public PathToken(Path path, int index)
        {
            this.path = path;
            this.index = index;
        }

        public Path Path
        {
            get { return this.path; }
        }

        public int Index { get { return this.index; } }

        public IPathNode Current
        {
            get { return path[index]; }
        }

        public bool HasNext()
        {
            if (index + 1 < path.Count)
                return true;
            return false;
        }

        public PathToken NextToken()
        {
            if (!HasNext())
                throw new IndexOutOfRangeException();
            return new PathToken(path, index + 1);
        }

        public override string ToString()
        {
            StringBuilder buf = new StringBuilder();
            this.Current.ToString();
            buf.Append(this.Current.ToString()).Append(" Index:").Append(this.index);
            return buf.ToString();
        }
    }
}