using System.Collections.Generic;
using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.Repositories.Interfaces;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly IRelatorioRepository _repository;

        public RelatorioService(IRelatorioRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<RelatorioAlunosPorTurmaDto> GetAlunosPorTurma()
        {
            return _repository.GetAlunosPorTurma();
        }
    }
}