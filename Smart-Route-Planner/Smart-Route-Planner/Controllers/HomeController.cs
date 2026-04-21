using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Smart_Route_Planner.Models;
using Smart_Route_Planner.ViewModels;

namespace Smart_Route_Planner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Result(double lat, double lng)
        {
            // Perform Dijkstra 

            // Dummy Data
            var apt_list = new List<ResultVM>
            {
                new ResultVM { Id = 1, Name = "Azure Residences", Type = "3-BDR", Latitude = 31.440323, Longitude = 31.678476, Distance = 125, Price = 4500 },
                new ResultVM { Id = 2, Name = "Nile View Suites", Type = "2-BDR", Latitude = 31.443909, Longitude = 31.695127, Distance = 210, Price = 3200 },
                new ResultVM { Id = 3, Name = "Pearl Towers", Type = "Studio", Latitude = 31.431979, Longitude = 31.683626, Distance = 380, Price = 1800 },
                new ResultVM { Id = 4, Name = "Harbor Gate", Type = "2-BDR", Latitude = 31.435053, Longitude = 31.654015, Distance = 460, Price = 2900 },
                new ResultVM { Id = 5, Name = "El Nour Complex", Type = "4-BDR", Latitude = 31.446104, Longitude = 31.660109, Distance = 540, Price = 6000 }
            };

            return View(apt_list);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
