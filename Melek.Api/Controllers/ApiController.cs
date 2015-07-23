using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Melek.Api.Repositories.Interfaces;

namespace Melek.Api.Controllers
{
    public class ApiController : Controller
    {
        private IMelekRepository _MelekRepository;

        public ApiController(IMelekRepository melekRepo)
        {
            _MelekRepository = melekRepo;
        }
        
        [HttpGet]
        public ContentResult AllData()
        {
            return Content(_MelekRepository.GetAllData(), "application/json");
        }

        [HttpGet]
        public ContentResult Version()
        {
            return Content(_MelekRepository.GetVersion(), "text/plain");
        }
    }
}