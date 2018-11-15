using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace ASpace_Aeon_Middleware
{
    public static class Extensions
    {
        //Reformat the series division text per archivists' request matching behavior of legacy package
        public static string FormatSeriesDivision(this string seriesText)
        {
            if (String.IsNullOrWhiteSpace(seriesText))
            {
                return "";
            }
            int outputInt;
            if (Int32.TryParse(seriesText,out outputInt))
            {
                return String.Format("Series {0}", outputInt.ToRomanNumeral());
            }
            return seriesText;
        }

        public static Dictionary<string, string> ToDictionary(this NameValueCollection collection)
        {
            //Note that this avoids duplicate key errors by overwriting if a key is reused
            var dict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string s in collection.AllKeys)
            {
                dict[s] = collection[s];
            }
            return dict;
        }

        //Tweaked from http://stackoverflow.com/a/23303475
        public static string ToRomanNumeral(this int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException("value", value, "negative integers cannot be converted to roman numerals");
            }

            StringBuilder sb = new StringBuilder();
            int remain = value;
            while (remain > 0)
            {
                if (remain >= 1000) { sb.Append("M"); remain -= 1000; }
                else if (remain >= 900) { sb.Append("CM"); remain -= 900; }
                else if (remain >= 500) { sb.Append("D"); remain -= 500; }
                else if (remain >= 400) { sb.Append("CD"); remain -= 400; }
                else if (remain >= 100) { sb.Append("C"); remain -= 100; }
                else if (remain >= 90) { sb.Append("XC"); remain -= 90; }
                else if (remain >= 50) { sb.Append("L"); remain -= 50; }
                else if (remain >= 40) { sb.Append("XL"); remain -= 40; }
                else if (remain >= 10) { sb.Append("X"); remain -= 10; }
                else if (remain >= 9) { sb.Append("IX"); remain -= 9; }
                else if (remain >= 5) { sb.Append("V"); remain -= 5; }
                else if (remain >= 4) { sb.Append("IV"); remain -= 4; }
                else if (remain >= 1) { sb.Append("I"); remain -= 1; }
                else if (remain == 0) { sb.Append(""); remain -= 1; }
                else throw new Exception("Unexpected error."); // Infinite loop insurance
            }

            return sb.ToString();
        }
    }
}