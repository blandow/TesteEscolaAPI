using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TesteEscolaAPI.DTOs.Responses
{
    public class RelatorioAlunosPorTurmaDto
    {
        public string NomeTurma { get; set; }
        public int QuantidadeAlunosMatriculados { get; set; }
        public int VagasRestantes { get; set; }
    }
}