using System.Text.RegularExpressions;

namespace core.Helpers
{
    public class GitHubUrlParser
    {
        // Regex to capture server (with scheme), organization, and repository
        private static readonly Regex GitHubUrlPattern = new(
            @"^(?:(?<scheme>https?:\/\/)|git@)(?<server>[^\/@:]+)[\/:](?<org>[^\/]+)\/(?<repo>[^\/]+?)(?:\.git)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static (string Server, string Organization, string Repository) Parse(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL cannot be null or empty.", nameof(url));

            var match = GitHubUrlPattern.Match(url);
            if (!match.Success)
                throw new FormatException($"Invalid GitHub URL format: {url}");

            var scheme = match.Groups["scheme"].Success ? match.Groups["scheme"].Value : "ssh://";
            var server = match.Groups["server"].Value;
            var org = match.Groups["org"].Value;
            var repo = match.Groups["repo"].Value;

            return ($"{scheme}{server}", org, repo);
        }
    }
}