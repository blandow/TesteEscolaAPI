using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Repositories;
using TesteEscolaAPI.Services;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Controllers
{
    [RoutePrefix("api/matriculas")]
    public class MatriculasController : ApiController
    {
        private readonly IMatriculaService _service;

        public MatriculasController()
        {
            var connectionFactory = new DbConnectionFactory();
            _service = new MatriculaService(
                connectionFactory,
                new AlunoRepository(connectionFactory),
                new TurmaRepository(connectionFactory),
                new MatriculaRepository());
        }

        [HttpPost, Route("")]
        public IHttpActionResult Post(MatriculaCreateDto dto)
        {
            try
            {
                var resultado = _service.Matricular(dto);
                return Content(HttpStatusCode.Created, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Content(HttpStatusCode.Conflict, new { erro = ex.Message });
            }
        }
    }
}