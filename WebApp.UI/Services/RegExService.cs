using System.Text.RegularExpressions;

    namespace WebApp.UI.Services
{
    public class RegExService
    {
        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        }
        public string[] SplitCsv(string input)
        {
            string pattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";
            return Regex.Split(input, pattern);
        }
        public string ReplaceDigits(string input)
        {
            string pattern = @"\d";
            string replacement = "#";
            return Regex.Replace(input, pattern, replacement);
        }
        public void ParseDate(string date)
        {
            string pattern = @"^(?<Year>\d{4})-(?<Month>\d{2})-(?<Day>\d{2})$";
            Match match = Regex.Match(date, pattern);
            if (match.Success)
            {
                string year = match.Groups["Year"].Value;
                string month = match.Groups["Month"].Value;
                string day = match.Groups["Day"].Value;
                // Utilize extracted data
            }
        }
    }
}

