using System.Text;

namespace SortingToolClass
{
    public class HappyPanda : IComparable<HappyPanda>
    {
        private Int64 number;
        private String key;
        private readonly static String separator = ". ";

        public HappyPanda(Int64 number, String key)
        {
            this.number = number;
            this.key = key;
        }

        public HappyPanda(String stringToParse)
        {
            int splitIndex = stringToParse.IndexOf(separator);
            Int64 parsedNumber = 0;
            String parsedKey = String.Empty;
            if (splitIndex > 0)
            {
                if (Int64.TryParse(stringToParse.Substring(0, splitIndex), out parsedNumber))
                {
                    parsedKey = stringToParse.Substring(splitIndex + separator.Length, stringToParse.Length - splitIndex - separator.Length);

                }
            }
            this.number = parsedNumber;
            this.key = parsedKey;
        }

        public static bool TryParse(String stringToParse, out HappyPanda? happyPanda)
        {
            int splitIndex = stringToParse.IndexOf(separator);
            Int64 parsedNumber;
            if (splitIndex > 0)
            {
                if (Int64.TryParse(stringToParse.Substring(0, splitIndex), out parsedNumber))
                {                    
                    String parsedKey = stringToParse.Substring(splitIndex + separator.Length, stringToParse.Length - splitIndex - separator.Length);
                    happyPanda = new HappyPanda(parsedNumber, parsedKey);
                    return true;
                }
                else
                {
                    happyPanda = null;
                    return false;
                }
            }
            else
            {
                happyPanda = null;
                return false;
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(number);
            sb.Append(separator);
            sb.Append(key);

            return sb.ToString();
        }

        public int CompareTo(HappyPanda? other)
        {
            if (other == null) return 1;
            else
            {
                int compareValue = key.CompareTo(other.key);
                if (compareValue != 0)
                    return compareValue;
                else
                    return number.CompareTo(other.number); 
            }
        }
    }
}
