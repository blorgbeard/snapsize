namespace Snapsize
{
    class Area
    {
        public Fraction Left { get; private set; }
        public Fraction Right { get; private set; }
        public Fraction Top { get; private set; }
        public Fraction Bottom { get; private set; }

        public Area(Fraction left, Fraction top, Fraction right, Fraction bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
