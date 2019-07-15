using System;
using System.Globalization;
using static System.FormattableString;

namespace UiAutomation
{
    public class Coordinate
    {

            public static Coordinate Parse(string input) => new Coordinate(input);

            public Coordinate(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Coordinate(string input)
            {
                if (string.IsNullOrEmpty(input))
                {
                    X = 0;
                    Y = 0;
                    return;
                }
                var list = input.Split(',');
                if (list.Length != 2) throw new ArgumentException("Could not parse a coordinate as 'int,int'");
                X = Convert.ToInt32(list[0], CultureInfo.InvariantCulture);
                Y = Convert.ToInt32(list[1], CultureInfo.InvariantCulture);
            }

            public int X { get; }
            public int Y { get; }
            public override string ToString() => Invariant($"{X}, {Y}");

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;
                var p = (Coordinate)obj;
                return X == p.X && Y == p.Y;
            }

            public override int GetHashCode() => Tuple.Create(X, Y).GetHashCode(); // (X << 2) ^ Y;
    }
}
