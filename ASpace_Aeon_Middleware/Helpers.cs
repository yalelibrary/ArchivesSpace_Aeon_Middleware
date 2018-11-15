using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using ArchivesSpace_.Net_Client.Models;

namespace ASpace_Aeon_Middleware
{
    public static class Helpers
    {
        public static string FormatBoxText(string boxIdentifier)
        {
            return String.Format("Box {0}", boxIdentifier);  
        }

        public static string GetCallNumberFromTopContainer(TopContainer container)
        {
            if (container == null || container.Collection.Count < 1)
            {
                return "";
            }
            return FormatCallNumber(container.Collection.First().Identifier);
        }

        public static string FormatCallNumber(string callNumber)
        {
            return SafeStringOperation(callNumber, () => StripPipes(callNumber.Replace("--", " ").Replace("-", " ")));
        }

        public static string AddEmphasis(string targetString, string keywordString)
        {
            if ((String.IsNullOrWhiteSpace(targetString)) || String.IsNullOrWhiteSpace(keywordString))
            {
                return "";
            }
            var splitChar = new char[] {' '};
            var targetParts = targetString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            var keywordParts = keywordString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            for (var i = 0; i < targetParts.Count; i++)
            {
                if (keywordParts.Contains(targetParts[i], StringComparer.InvariantCultureIgnoreCase))
                {
                    targetParts[i] = AddEmphasisHtml(targetParts[i]);
                }
            }
            return String.Join(" ", targetParts);
        }

        private static string AddEmphasisHtml(string inputString)
        {
            return SafeStringOperation(inputString, () => String.Format("<b><u>{0}</u></b>", inputString));
        }

        public static string StripPipes(string inputString)
        {
            //return String.IsNullOrWhiteSpace(inputString) ? "" : inputString.Replace("|", " ");
            return SafeStringOperation(inputString, () => inputString.Replace("|", " "));
        }

        private static string SafeStringOperation(string inputString, Func<string> action)
        {
            return String.IsNullOrWhiteSpace(inputString) ? "" : action.Invoke();
        }
    }
}