using System.Web.Http;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Repositories;
using TesteEscolaAPI.Services;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Controllers
{
    [RoutePrefix("api/turmas")]
    public class TurmasController : ApiController
    {
        private readonly ITurmaService _service;

        public TurmasController()
        {
            var connectionFactory = new DbConnectionFactory();
            _service = new TurmaService(new TurmaRepository(connectionFactory));
        }
        [HttpGet, Route("")]
        public IHttpActionResult Get()
        {
            return Ok(_service.GetAll());
        }
    }
}