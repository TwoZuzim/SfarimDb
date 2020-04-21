using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Sfarim.Proxies;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sfarim.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string SelectedMasechta { get; set; }
        [BindProperty(SupportsGet = true)]
        public int SelectedPage { get; set; }
        public SelectList Masectos { get; set; }
        public SelectList Pages { get; set; }

        private readonly ILogger<IndexModel> _logger;
        private readonly ISeferProxy _seferProxy;
        private readonly IDbProxy _dbProxy;

        public IndexModel(ILogger<IndexModel> logger, ISeferProxy proxy, IDbProxy dbProxy)
        {
            _logger = logger;
            _dbProxy = dbProxy;
            _seferProxy = proxy;
            SelectedMasechta = masechtos.First().Name;
            SelectedPage = -1;
        }

        public async Task OnGetAsync()
        {
            Masectos = new SelectList(masechtos, "Name", "Name");

            var masecta = masechtos.First(m => m.Name == SelectedMasechta);
            var gemara = await _seferProxy.GetGemara(masecta.Seder, masecta.Name).ConfigureAwait(false);
            var firstPage = Array.IndexOf(gemara.Text, gemara.Text.First(t => t.Any()));
            var lastPage = gemara.Text.Length;

            Pages = new SelectList(Enumerable.Range(firstPage, lastPage - firstPage));
            if (SelectedPage == -1)
            {
                SelectedPage = firstPage;
            }
            var normalizedText = gemara.GetNormalizedPageText(SelectedPage);
            var distinctWordsOnPage = normalizedText.Split((char[])null, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
            var dvarim = await _dbProxy.CreateDevarim(distinctWordsOnPage).ConfigureAwait(false);
        }

        private readonly Masechta[] masechtos = new[]
        {
            new Masechta("Zeraim", "Berakhot"),
            new Masechta("Moed", "Beitzah"),
            new Masechta("Moed", "Chagigah"),
            new Masechta("Moed", "Eruvin"),
            new Masechta("Moed", "Megillah"),
            new Masechta("Moed", "Moed Katan"),
            new Masechta("Moed", "Pesachim"),
            new Masechta("Moed", "Rosh Hashanah"),
            new Masechta("Moed", "Shabbat"),
            new Masechta("Moed", "Sukkah"),
            new Masechta("Moed", "Taanit"),
            new Masechta("Moed", "Yoma"),
            new Masechta("Nashim", "Gittin"),
            new Masechta("Nashim", "Ketubot"),
            new Masechta("Nashim", "Kiddushin"),
            new Masechta("Nashim", "Nazir"),
            new Masechta("Nashim", "Nedarim"),
            new Masechta("Nashim", "Sotah"),
            new Masechta("Nashim", "Yevamot"),
            new Masechta("Nezikin", "Avodah Zarah"),
            new Masechta("Nezikin", "Bava Batra"),
            new Masechta("Nezikin", "Bava Kamma"),
            new Masechta("Nezikin", "Bava Metzia"),
            new Masechta("Nezikin", "Horayot"),
            new Masechta("Nezikin", "Makkot"),
            new Masechta("Nezikin", "Sanhedrin"),
            new Masechta("Nezikin", "Shevuot"),
            new Masechta("Kodashim", "Arakhin"),
            new Masechta("Kodashim", "Bekhorot"),
            new Masechta("Kodashim", "Chullin"),
            new Masechta("Kodashim", "Keritot"),
            new Masechta("Kodashim", "Meilah"),
            new Masechta("Kodashim", "Menachot"),
            new Masechta("Kodashim", "Tamid"),
            new Masechta("Kodashim", "Temurah"),
            new Masechta("Kodashim", "Zevachim"),
            new Masechta("Tahorot", "Niddah"),
        };
    }

    public class Masechta
    {
        public string Seder { get; set; }
        public string Name { get; set; }

        public Masechta(string seder, string masechta)
        {
            Seder = seder;
            Name = masechta;
        }
    }
}
