using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using static System.FormattableString;

namespace UiAutomation
{
    public enum GridItemType
    {
        Cell,
        Column,
        Row
    }
    [Documentation("Element in a grid: cell, row, or column")]
    public class GridItem
    {
        public GridItem(int row, int column)
        {
            Row = row;
            Column = column;
            GridItemType = GridItemType.Cell;
        }

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

        public int Column { get; private set; }
        public GridItemType GridItemType { get; set; }
        public int Row { get; private set; }
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

        public override string ToString()
        {
            switch (GridItemType)
            {
                case GridItemType.Cell: return Invariant($"row {Row}, column {Column}");
                case GridItemType.Column: return Invariant($"column {Column}");
                case GridItemType.Row: return Invariant($"row {Row}");
                default: return string.Empty;
            }
        }
    }
}