using System.Net;
using Melek.Api.Repositories.Interfaces;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Melek.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class ApiController : Controller
    {
        private IApplicationEnvironment _ApplicationEnvironment;
        private IHostingEnvironment _HostingEnvironment;
        private IMelekRepository _MelekRepository;

        public ApiController(IApplicationEnvironment app, IHostingEnvironment hosting, IMelekRepository melekRepo)
        {
            _ApplicationEnvironment = app;
            _HostingEnvironment = hosting;
            _MelekRepository = melekRepo;
        }

        [HttpGet]
        public ActionResult AllData()
        {
            return Redirect("/Data/melek-data-store.zip");
            // TODO: find out why the below results in a FileNotFound exception, despite the file definitely being there
            //return File(_HostingEnvironment.MapPath(@"Data\melek-data-store.zip"), "application/zip", "melek-data-store.zip");
        }

        [HttpGet]
        public ViewResult AreWeUp()
        {
            return View();
        }

        [HttpGet("{name}")]
        public ActionResult Card(string name)
        {
            // this is weird. when i run the application on weblistener or iis, the name comes in url decoded. on kestrel, it doesn't. 
            // it seems like that would be a part of the mvc middleware, not the web server. i'm confused.
            ICard card = _MelekRepository.GetCardByName(WebUtility.UrlDecode(name));
            if (card != null) return Content(JsonConvert.SerializeObject(card), MediaTypeHeaderValue.Parse("application/json"));

            return HttpBadRequest();
        }

        [HttpGet]
        public string Version()
        {
            return _MelekRepository.GetVersion();
        }
    }
}