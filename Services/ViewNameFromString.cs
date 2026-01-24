using System.Text.RegularExpressions;

namespace MyTools.Services
{
    /// <summary>
    /// Extracts view names from text content.
    /// </summary>
    public static class ViewNameFromString
    {
        private static readonly Regex QuotedTextPattern = new Regex("\"([^\"]*)\"", RegexOptions.Compiled);

        /// <summary>
        /// Extracts the first quoted text from the input string.
        /// </summary>
        public static string TitleText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            Match match = QuotedTextPattern.Match(input);
            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
