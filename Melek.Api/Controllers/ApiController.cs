using Melek.Api.Repositories.Interfaces;
using Melek.Domain;
using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Melek.Api.Controllers
{
    [Route("[controller]/[action]")]
    public class ApiController : Controller
    {
        private IMelekRepository _MelekRepository;

        public ApiController(IMelekRepository melekRepo)
        {
            _MelekRepository = melekRepo;
        }
        
        [HttpGet]
        public string AllData()
        {
            return _MelekRepository.GetAllData();
        }

        [HttpGet("{name}")]
        public ActionResult CardByName(string name)
        {
            ICard card = _MelekRepository.GetCardByName(name);
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