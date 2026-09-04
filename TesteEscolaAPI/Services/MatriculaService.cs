using System;
using System.Collections.Generic;
using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;
using TesteEscolaAPI.Services.Interfaces;

namespace TesteEscolaAPI.Services
{
    public class MatriculaService : IMatriculaService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IAlunoRepository _alunoRepository;
        private readonly ITurmaRepository _turmaRepository;
        private readonly IMatriculaRepository _matriculaRepository;

        public MatriculaService(
            IDbConnectionFactory connectionFactory,
            IAlunoRepository alunoRepository,
            ITurmaRepository turmaRepository,
            IMatriculaRepository matriculaRepository)
        {
            _connectionFactory = connectionFactory;
            _alunoRepository = alunoRepository;
            _turmaRepository = turmaRepository;
            _matriculaRepository = matriculaRepository;
        }

        public MatriculaResultDto Matricular(MatriculaCreateDto dto)
        {
            if (dto.AlunoId <= 0 || dto.TurmaId <= 0)
                throw new ArgumentException("AlunoId e TurmaId são obrigatórios.");

            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var aluno = _alunoRepository.GetByIdComTransacao(dto.AlunoId, connection, transaction);
                        if (aluno == null)
                            throw new KeyNotFoundException("Aluno não encontrado.");
                        if (!aluno.Ativo)
                            throw new InvalidOperationException("Aluno inativo não pode ser matriculado.");

                        var turma = _turmaRepository.GetByIdComTransacao(dto.TurmaId, connection, transaction);
                        if (turma == null)
                            throw new KeyNotFoundException("Turma não encontrada.");

                        var isMatriculado = _matriculaRepository.ExisteAlunoNaTurma(dto.AlunoId, dto.TurmaId, connection, transaction);
                        if (isMatriculado)
                            throw new InvalidOperationException("Aluno já matriculado nesta turma.");

                        var vagaDecrementada = _turmaRepository.DecrementarVaga(dto.TurmaId, connection, transaction);
                        if (!vagaDecrementada)
                            throw new InvalidOperationException("Turma sem vaga disponível.");

                        var matricula = new Matricula
                        {
                            AlunoId = dto.AlunoId,
                            TurmaId = dto.TurmaId,
                            DataMatricula = DateTime.Now
                        };
                        matricula.Id = _matriculaRepository.InsertMatricula(matricula, connection, transaction);

                        transaction.Commit();

                        return new MatriculaResultDto
                        {
                            Id = matricula.Id,
                            AlunoId = matricula.AlunoId,
                            TurmaId = matricula.TurmaId,
                            DataMatricula = matricula.DataMatricula,
                            VagasRestantes = turma.VagasDisponiveis - 1
                        };
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}