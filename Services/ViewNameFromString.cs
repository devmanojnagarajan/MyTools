using System.Text.RegularExpressions;

namespace MyTools.Services
{
    public class ViewNameFromString
    {
        public static string TitleText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            // This regex captures the content between the first set of double quotes
            Match match = Regex.Match(input, "\"([^\"]*)\"");

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
