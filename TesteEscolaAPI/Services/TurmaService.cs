using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Services
{
    public class TurmaService : ITurmaService
    {
        private readonly ITurmaRepository _repository;

        public TurmaService(ITurmaRepository repository)
        {
            _repository = repository;
        }
        public IEnumerable<TurmaDto> GetAll()
        {
            var turmas = _repository.GetAll();
            return turmas.Select(MapToDto).ToList();
        }

        private static TurmaDto MapToDto(Turma turma)
        {
            return new TurmaDto
            {
                Id = turma.Id,
                Nome = turma.Nome,
                Periodo = turma.Periodo,
                VagasTotal = turma.VagasTotal,
                VagasDisponiveis = turma.VagasDisponiveis
            };
        }
    }
}