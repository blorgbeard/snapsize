namespace Snapsize
{
    struct Fraction
    {
        public int Numerator { get; private set; }
        public int Denominator { get; private set; }

        public Fraction (int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }

        public static implicit operator Fraction (int i)
        {
            return new Fraction(i, 1);
        }

        public int Of(int value)
        {
            return value * Numerator / Denominator;
        }

        // todo: normalize function
    }
}
