using System.Text.RegularExpressions;
using Microsoft.AspNetCore.WebUtilities;

namespace Certiminer.Infrastructure
{
    public static class YouTubeId
    {
        private static readonly Regex IdRegex = new(@"^[a-zA-Z0-9_-]{11}$", RegexOptions.Compiled);

        /// <summary>
        /// Devuelve el ID de YouTube a partir de un ID o una URL. Si no puede, null.
        /// Acepta: watch?v=..., youtu.be/..., /embed/..., /shorts/... o el ID pelado (11 chars).
        /// </summary>
        public static string? Extract(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            input = input.Trim();

            if (IdRegex.IsMatch(input)) return input;

            if (!Uri.TryCreate(input, UriKind.Absolute, out var uri)) return null;

            var qs = QueryHelpers.ParseQuery(uri.Query);
            if (qs.TryGetValue("v", out var vVal) && vVal.Count > 0 && IdRegex.IsMatch(vVal[0]))
                return vVal[0];




            var host = uri.Host.ToLowerInvariant();
            var path = uri.AbsolutePath;

            if (host.Contains("youtu.be"))
            {
                var seg = path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (seg.Length > 0 && IdRegex.IsMatch(seg[0])) return seg[0];
            }

            var m = Regex.Match(input, @"(?:v=|be/)([A-Za-z0-9_\-]{11})");
            if (m.Success) return m.Groups[1].Value;

            // Si ya es un ID directo:
            if (Regex.IsMatch(input, @"^[A-Za-z0-9_\-]{11}$"))
                return input;

            return null;
        }
    }
}
