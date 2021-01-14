using System.Linq;

namespace PeanutButter.EasyArgs
{
    internal class StringPair
    {
        public string Left { get; }
        public string Right { get; }

        internal StringPair(
            string left,
            string right
        )
        {
            var sorted = new[] { left, right }.OrderBy(s => s).ToArray();
            Left = sorted[0];
            Right = sorted[1];
        }

        public override bool Equals(object obj)
        {
            var other = obj as StringPair;
            if (other is null)
            {
                return false;
            }
            return Equals(other);
        }

        protected bool Equals(StringPair other)
        {
            return Left == other.Left && Right == other.Right;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Left != null
                    ? Left.GetHashCode()
                    : 0) * 397) ^ (Right != null
                    ? Right.GetHashCode()
                    : 0);
            }
        }
    }
}