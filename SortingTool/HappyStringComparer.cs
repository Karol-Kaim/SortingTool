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
            if (x != null && y != null)
            {
                string[] first = x.Split(_separator);
                string[] second = y.Split(_separator);

                if (first.Length == 2 && second.Length == 2)
                {
                    string firstKey = first[1];
                    string secondKey = second[1];
                    int initialComparer = firstKey.CompareTo(secondKey);
                    if (initialComparer == 0)
                    {
                        Int32 firstNumber = 0;
                        Int32 secondNumber = 0;
                        if (Int32.TryParse(firstKey, out firstNumber) && Int32.TryParse(secondKey, out secondNumber))
                        {
                            return firstNumber - secondNumber;
                        }
                        else
                            return 0;
                    }
                    else
                        return initialComparer;
                }
                else return 0;
            }
            else return 0;
        }
    }
}
