using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SessionStateDemo.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        private const string sessionKey = "FirstVisitTimestamp";

        public DateTimeOffset DateFirstSeen;
        public DateTimeOffset Now = DateTimeOffset.Now;

        public async Task OnGet()
        {
            await HttpContext.Session.LoadAsync();

            if (HttpContext.Session.TryGetValue(sessionKey, out var sessionDate))
            {
                DateFirstSeen = JsonSerializer.Deserialize<DateTimeOffset>(sessionDate);
            }
            else
            {
                DateFirstSeen = Now;
                HttpContext.Session.Set(sessionKey, JsonSerializer.SerializeToUtf8Bytes(DateFirstSeen));
            }
        }
    }
}
