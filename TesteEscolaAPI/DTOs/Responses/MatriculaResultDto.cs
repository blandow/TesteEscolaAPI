using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TesteEscolaAPI.DTOs.Responses
{
    public class MatriculaResultDto
    {
        public int Id { get; set; }
        public int AlunoId { get; set; }
        public int TurmaId { get; set; }
        public DateTime DataMatricula { get; set; }
        public int VagasRestantes { get; set; }
    }
}