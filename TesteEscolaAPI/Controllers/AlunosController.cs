using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Repositories;
using TesteEscolaAPI.Services;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Controllers
{
    [RoutePrefix("api/alunos")]
    public class AlunosController : ApiController
    {
        private readonly IAlunoService _service;

        public AlunosController()
        {

            _service = new AlunoService(new AlunoRepository(new DbConnectionFactory()));
        }

        [HttpGet, Route("")]
        public IHttpActionResult Get(string nome = null, int pagina = 1, int tamanhoPagina = 10)
        {
            return Ok(_service.GetAll(nome, pagina, tamanhoPagina));
        }

        [HttpGet, Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try { return Ok(_service.GetById(id)); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost, Route("")]
        public IHttpActionResult Post(AlunoCreateDto dto)
        {
            try { return Content(HttpStatusCode.Created, _service.Create(dto)); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut, Route("{id:int}")]
        public IHttpActionResult Put(int id, AlunoUpdateDto dto)
        {
            try { _service.Update(id, dto); return Ok(); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpDelete, Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try { _service.Delete(id); return Ok(); }
            catch (KeyNotFoundException) { return NotFound(); }
        }
    }

}
