using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SortingTool
{
    internal class HappyStringComparer : IComparer<string>
    {
        String _separator = ". ";
        int IComparer<string>.Compare(string? x, string? y)
        {
            if (x == null || y == null)
                return 0;

            int splitIndexX = x.IndexOf(_separator);
            int splitIndexY = y.IndexOf(_separator);
            Int64 parsedNumberX = 0;
            String parsedKeyX = String.Empty;
            Int64 parsedNumberY = 0;
            String parsedKeyY = String.Empty;

            if (splitIndexX > 0 && splitIndexY > 0)
            {

                parsedKeyX = x.Substring(splitIndexX + _separator.Length, x.Length - splitIndexX - _separator.Length);
                parsedKeyY = y.Substring(splitIndexY + _separator.Length, y.Length - splitIndexY - _separator.Length);

                int initialComparison = parsedKeyX.CompareTo(parsedKeyY);

                if (initialComparison == 0)
                {
                    if (Int64.TryParse(x.Substring(0, splitIndexX), out parsedNumberX) && Int64.TryParse(y.Substring(0, splitIndexY), out parsedNumberY))
                    {
                        return parsedNumberX.CompareTo(parsedNumberY);
                    }
                    else
                        return 0;
                }
                else
                    return initialComparison;
            }
            else
                return 0;
        }
    }
}
