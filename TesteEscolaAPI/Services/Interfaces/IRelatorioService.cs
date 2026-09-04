
using System.Collections.Generic;
using TesteEscolaAPI.DTOs.Responses;

namespace TesteEscolaAPI.Services.Interfaces
{
    public interface IRelatorioService
    {
        IEnumerable<RelatorioAlunosPorTurmaDto> GetAlunosPorTurma();
    }
}
