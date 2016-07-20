using System.Net;
using Melek.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Melek.Api.Controllers
{
    [Route("api/[controller]")]
    public class MelekController : Controller
    {
        private IHostingEnvironment _HostingEnvironment;
        private IMelekRepository _MelekRepository;

        public MelekController(IHostingEnvironment hosting, IMelekRepository melekRepo)
        {
            _HostingEnvironment = hosting;
            _MelekRepository = melekRepo;
        }
        
        [HttpGet("alldata")]
        public ActionResult AllData()
        {
            return Redirect("/Data/melek-data-store.zip");
            // TODO: figure out why this (or something cleaner) doesn't work
            //var path = Path.Combine(_HostingEnvironment.ContentRootPath, @"wwwroot\Data\melek-data-store.zip");
            //return File(path, "application/zip", "melek-data-store.zip");
        }

        [HttpGet("are-we-up")]
        public ViewResult AreWeUp()
        {
            return View();
        }

        [HttpGet("card/{name}")]
        public ActionResult Card(string name)
        {
            // this is weird. when i run the application on weblistener or iis, the name comes in url decoded. on kestrel, it doesn't. 
            // it seems like that would be a part of the mvc middleware, not the web server. i'm confused.
            var card = _MelekRepository.GetCardByName(WebUtility.UrlDecode(name));
            if (card != null) return Content(JsonConvert.SerializeObject(card), MediaTypeHeaderValue.Parse("application/json"));

            Response.StatusCode = 400;
            return Content($@"Couldn't find the card ""{name}"".");
        }

        [HttpGet("version")]
        public string Version()
        {
            return _MelekRepository.GetVersion();
        }
    }
}