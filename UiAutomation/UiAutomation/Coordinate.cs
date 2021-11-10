// Copyright 2019-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

using System;
using System.Globalization;
using static System.FormattableString;

namespace UiAutomation
{
    /// <summary>Coordinate pair x,y</summary>
    public class Coordinate
    {
        /// <summary>Initialize coordinates with X and Y parameters</summary>
        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Initialize coordinate with a string to be parsed. If empty string, will become 0,0</summary>
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

        /// <summary>X-coordinate</summary>
        public int X { get; }

        /// <summary>Y-coordinate</summary>
        public int Y { get; }

        /// <summary>Coordinate object is considered equal to another one if the X and Y values are equal.</summary>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) return false;
            var p = (Coordinate)obj;
            return X == p.X && Y == p.Y;
        }

        /// <summary>Base hash code on the values of X and Y</summary>
        public override int GetHashCode() => Tuple.Create(X, Y).GetHashCode();

        /// <summary>Enable Coordinates to be used as parameters in fixtures</summary>
        public static Coordinate Parse(string input) => new Coordinate(input);

        /// <summary>Show X and Y coordinates</summary>
        public override string ToString() => Invariant($"{X}, {Y}");
    }
}
