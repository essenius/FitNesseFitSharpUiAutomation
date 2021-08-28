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
using System.Linq;
using System.Text.RegularExpressions;
using static System.FormattableString;

namespace UiAutomation
{
    /// <summary>Whether the grid item is a cell, column or row</summary>
    public enum GridItemType
    {
        /// <summary>one cell in the grid</summary>
        Cell,

        /// <summary>one column in the grid</summary>
        Column,

        /// <summary>one row in the grid</summary>
        Row
    }

    /// <summary>Element in a grid: cell, row, or column</summary>
    public class GridItem
    {
        /// <summary>Initialize GridItem with row, column</summary>
        public GridItem(int row, int column)
        {
            Row = row;
            Column = column;
            GridItemType = GridItemType.Cell;
        }

        /// <summary>Initialize GridItem with row, column in a string</summary>
        /// <param name="input">"x,y" or "row x,column y" or "row x" or "column y" with x and y positive int</param>
        public GridItem(string input)
        {
            const string message = "Could not parse GridItem. Expected: 'x,y' or 'row x,column y' or 'row x' or 'column y'";
            if (string.IsNullOrEmpty(input)) throw new ArgumentException(message);
            if (ParsePair(input)) return;
            var list = input.Split(',');
            if (list.Any(entry => !ParseGridItemDimension(entry)))
            {
                throw new ArgumentException(message);
            }
        }

        /// <summary>The column</summary>
        public int Column { get; private set; }

        /// <summary>Grid Item Type (Cell, Row or Column)</summary>
        public GridItemType GridItemType { get; private set; }

        /// <summary>The row</summary>
        public int Row { get; private set; }

        /// <summary>The way to make the object parsable for FitSharp</summary>
        public static GridItem Parse(string input) => new GridItem(input);

        private bool ParseGridItemDimension(string input)
        {
            var rowMatch = Regex.Match(input, @"[r|row]\s*(\d+)", RegexOptions.IgnoreCase);
            if (rowMatch.Success)
            {
                Row = Convert.ToInt32(rowMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                GridItemType = GridItemType == GridItemType.Column ? GridItemType.Cell : GridItemType.Row;
                return true;
            }
            var match = Regex.Match(input, @"col[^\d]*(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            Column = Convert.ToInt32(match.Groups[1].Value, CultureInfo.InvariantCulture);
            GridItemType = GridItemType == GridItemType.Row ? GridItemType.Cell : GridItemType.Column;
            return true;
        }

        private bool ParsePair(string input)
        {
            var match = Regex.Match(input, @"(\d+)\s*,\s*(\d+)", RegexOptions.IgnoreCase);
            if (!match.Success) return false;
            Row = Convert.ToInt32(match.Groups[1].Value, CultureInfo.InvariantCulture);
            Column = Convert.ToInt32(match.Groups[2].Value, CultureInfo.InvariantCulture);
            GridItemType = GridItemType.Cell;
            return true;
        }

        /// <summary>Shown in FitNesse if returned as an object</summary>
        public override string ToString()
        {
            return GridItemType switch
            {
                GridItemType.Cell => Invariant($"row {Row}, column {Column}"),
                GridItemType.Column => Invariant($"column {Column}"),
                GridItemType.Row => Invariant($"row {Row}"),
                _ => string.Empty
            };
        }
    }
}
