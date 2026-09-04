using System.Web.Http;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Repositories;
using TesteEscolaAPI.Services;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Controllers
{
    [RoutePrefix("api/relatorios")]
    public class RelatoriosController : ApiController
    {
        private readonly IRelatorioService _service;

        public RelatoriosController()
        {
            _service = new RelatorioService(new RelatorioRepository(new DbConnectionFactory()));
        }

        [HttpGet, Route("alunos-por-turma")]
        public IHttpActionResult Get()
        {
            return Ok(_service.GetAlunosPorTurma());
        }
    }
}