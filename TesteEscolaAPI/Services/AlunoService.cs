using System;
using System.Collections.Generic;
using System.Linq;
using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Services
{
    public class AlunoService : IAlunoService
    {

        private readonly IAlunoRepository _repository;

        public AlunoService(IAlunoRepository repository)
        {
            _repository = repository;
        }

        public PagedResultDto<AlunoDto> GetAll(string nome, int pagina, int tamanhoPagina)
        {
            if (pagina < 1) pagina = 1;
            if (tamanhoPagina < 1 || tamanhoPagina > 100) tamanhoPagina = 10;

            var alunos = _repository.GetAll(nome, pagina, tamanhoPagina, out int total);

            return new PagedResultDto<AlunoDto>
            {
                Total = total,
                Pagina = pagina,
                TamanhoPagina = tamanhoPagina,
                Itens = alunos.Select(MapToDto).ToList()
            };
        }

        public AlunoDto GetById(int id)
        {
            var aluno = _repository.GetById(id);
            if (aluno == null) throw new KeyNotFoundException("Aluno não encontrado.");
            return MapToDto(aluno);
        }

        public AlunoDto Create(AlunoCreateDto dto)
        {
            Validate(dto.Nome, dto.Email, dto.DataNascimento);

            var aluno = new Aluno { Nome = dto.Nome, Email = dto.Email, DataNascimento = dto.DataNascimento };
            aluno.Id = _repository.Insert(aluno);
            return MapToDto(aluno);
        }

        public void Update(int id, AlunoUpdateDto dto)
        {
            Validate(dto.Nome, dto.Email, dto.DataNascimento);

            var existente = _repository.GetById(id);
            if (existente == null) throw new KeyNotFoundException("Aluno não encontrado.");

            existente.Nome = dto.Nome;
            existente.Email = dto.Email;
            existente.DataNascimento = dto.DataNascimento;

            _repository.Update(existente);
        }

        public void Delete(int id)
        {
            var existente = _repository.GetById(id);
            if (existente == null) throw new KeyNotFoundException("Aluno não encontrado.");

            _repository.SoftDelete(id);
        }

        private void Validate(string nome, string email, DateTime dataNascimento)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Email inválido.");
            if (dataNascimento >= DateTime.Today)
                throw new ArgumentException("Data de nascimento inválida.");
        }

        private static AlunoDto MapToDto(Aluno aluno)
        {
            return new AlunoDto
            {
                Id = aluno.Id,
                Nome = aluno.Nome,
                Email = aluno.Email,
                DataNascimento = aluno.DataNascimento,
                Ativo = aluno.Ativo
            };
        }
    }
}

