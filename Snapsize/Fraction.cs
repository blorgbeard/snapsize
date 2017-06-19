using System;
using System.Linq;
using System.Runtime.Serialization;

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
        
        public string Serialize()
        {
            if (Denominator == 1)
            {
                return Numerator.ToString();
            }
            return string.Format("{0}/{1}", Numerator, Denominator);
        }

        public static Fraction Deserialize(string input)
        {
            var parts = input.Split('/').Select(t => t.Trim()).ToArray();
                ;
            if (parts.Length < 1 || parts.Length > 2) throw new SerializationException("Invalid fraction format");
            if (!int.TryParse(parts[0], out int numerator)) throw new SerializationException("Invalid fraction format");
            if (parts.Length == 1)
            {
                return new Fraction(numerator, 1);
            }
            else
            {
                if (!int.TryParse(parts[1], out int denominator)) throw new SerializationException("Invalid fraction format");
                return new Fraction(numerator, denominator);
            }
        }
    }
}
