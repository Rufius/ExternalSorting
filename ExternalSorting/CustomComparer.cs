using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting {
    public class CustomComparer : IComparer<string> {
        public int Compare(string? line1, string? line2) {
            if (line1 == null && line2 == null) return 0;

            if (line1 == null || line2 == null) { return line1 == null ? -1 : 1; }

            int separatorIndex1 = line1.IndexOf(Constants.Separator);
            int separatorIndex2 = line2.IndexOf(Constants.Separator);

            // split the first line into number and text parts
            int number1 = Convert.ToInt32(line1.Substring(0, separatorIndex1));
            string text1 = line1.Substring(separatorIndex1+1);

            // split the second line into number and text parts
            int number2 = Convert.ToInt32(line2.Substring(0, separatorIndex2));
            string text2 = line2.Substring(separatorIndex2+1);

            // first compare text parts
            int textCompareResult = text1.CompareTo(text2);

            return textCompareResult == 0 ? number1.CompareTo(number2) : textCompareResult;
        }
    }
}
