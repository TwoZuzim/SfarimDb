using System.Text.RegularExpressions;

namespace Sfarim.Proxies
{
    public class ApiSefer
    {
        public string Status { get; set; }
        public string VersionTitle { get; set; }
        public string[] SectionNames { get; set; }
        public string Language { get; set; }
        public string Title { get; set; }
        public string[][] Text { get; set; }

        public string GetNormalizedPageText(int pageIndex)
        {
            var pageText = string.Join(' ', Text[pageIndex]);
            var withoutHtml = Regex.Replace(pageText, @"<[^>]*>", string.Empty);
            return Regex.Replace(withoutHtml, @"(\.|:)", string.Empty);
        }
    }
}
