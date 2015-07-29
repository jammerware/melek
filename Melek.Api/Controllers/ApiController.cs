﻿using System.IO;
using System.Net;
using Melek.Api.Repositories.Interfaces;
using Melek.Domain;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Runtime;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Melek.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class ApiController : Controller
    {
        private IHostingEnvironment _HostingEnvironment;
        private IMelekRepository _MelekRepository;

        public ApiController(IApplicationEnvironment app, IHostingEnvironment hosting, IMelekRepository melekRepo)
        {
            _HostingEnvironment = hosting;
            _MelekRepository = melekRepo;
        }
        
        [HttpGet]
        public ActionResult AllData()
        {
            //return File(Path.Combine(_HostingEnvironment.WebRootPath, @"Data\melek-data-store.7z"), "application/x-7z-compressed", "melek-data-store.7z");
            return File(Path.Combine(_HostingEnvironment.WebRootPath, @"Data\melek-data-store.zip"), "application/zip", "melek-data-store.zip");
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