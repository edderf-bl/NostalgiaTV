using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NostalgiaTV.Models;
using NostalgiaTV.Models.Configuration;

namespace NostalgiaTV.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptionsMonitor<List<ChannelConfiguration>>  _channelsConfig;

        public HomeController(IOptionsMonitor<List<ChannelConfiguration>> channelsConfiguration)
        {
            _channelsConfig = channelsConfiguration;
        }

        public IActionResult Index()
        {
            ViewData["ChannelsConfig"] = _channelsConfig.CurrentValue;
            return View();
        }

        [Route("{*url}", Order = 999)] // Order = 999 ensures this is the last route to be matched
        public IActionResult Player(string url)
        {
            var channel = _channelsConfig.CurrentValue.FirstOrDefault(x => x.WebPath.TrimStart('/') == url);
            if (channel != null)
            {
                ViewData["ChannelConfig"] = channel;
                return View();
            }
            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
