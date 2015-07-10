using System.Web.Mvc;

namespace Melek.Api.Controllers
{
    public class CardsController : Controller
    {
        // GET: Cards
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Card(string slug)
        {
            
            return View();
        }
    }
}