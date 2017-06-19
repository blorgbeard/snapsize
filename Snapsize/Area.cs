using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Snapsize
{
    class Area
    {
        public Fraction Left { get; private set; }
        public Fraction Top { get; private set; }
        public Fraction Right { get; private set; }
        public Fraction Bottom { get; private set; }

        public Area(Fraction left, Fraction top, Fraction right, Fraction bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public string Serialize()
        {
            var bits = new[] { Left, Top, Right, Bottom };
            return string.Join(", ", bits.Select(t => t.Serialize()));
        }

        public static Area Deserialize(string input)
        {
            var parts = input.Split(',').Select(Fraction.Deserialize).ToArray();
            if (parts.Length != 4) throw new SerializationException("Invalid area format");
            return new Area(parts[0], parts[1], parts[2], parts[3]);
        }
    }
}
