using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyTools
{
    public class ViewNameFromString
    {
        public string TitleText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // This regex captures the content between the first set of double quotes
            Match match = Regex.Match(input, "\"([^\"]*)\"");

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
