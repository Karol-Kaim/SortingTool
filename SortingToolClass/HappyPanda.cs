using System.Text;

namespace SortingToolClass
{
    public class HappyPanda : IComparable<HappyPanda>
    {
        private Int64 number;
        private String key;
        private readonly String separator = ". ";

        public HappyPanda(Int64 number, String key)
        {
            this.number = number;
            this.key = key;
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
