
using System.Collections.Generic;
using TesteEscolaAPI.DTOs.Responses;

namespace TesteEscolaAPI.Repositories.Interfaces
{
    public interface IRelatorioRepository
    {
        IEnumerable<RelatorioAlunosPorTurmaDto> GetAlunosPorTurma();
    }
}
