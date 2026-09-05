using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;
using TesteEscolaAPI.Services;

namespace TesteEscolaAPI.Tests
{
    [TestClass]
    public class MatriculaServiceTests
    {
        private Mock<IDbConnectionFactory> _connectionFactoryMock;
        private Mock<IDbConnection> _connectionMock;
        private Mock<IDbTransaction> _transactionMock;
        private Mock<IAlunoRepository> _alunoRepositoryMock;
        private Mock<ITurmaRepository> _turmaRepositoryMock;
        private Mock<IMatriculaRepository> _matriculaRepositoryMock;
        private MatriculaService _service;

        [TestInitialize]
        public void Setup()
        {
            // General Arrange 
            _connectionMock = new Mock<IDbConnection>();
            _transactionMock = new Mock<IDbTransaction>();

            _connectionMock
                .Setup(c => c.BeginTransaction())
                .Returns(_transactionMock.Object);

            _connectionFactoryMock = new Mock<IDbConnectionFactory>();
            _connectionFactoryMock
                .Setup(f => f.CreateConnection())
                .Returns(_connectionMock.Object);

            _alunoRepositoryMock = new Mock<IAlunoRepository>();
            _turmaRepositoryMock = new Mock<ITurmaRepository>();
            _matriculaRepositoryMock = new Mock<IMatriculaRepository>();

            _service = new MatriculaService(
                _connectionFactoryMock.Object,
                _alunoRepositoryMock.Object,
                _turmaRepositoryMock.Object,
                _matriculaRepositoryMock.Object);
        }

        [TestMethod]
        public void Matricular_ComDadosValidos_DeveRealizarMatriculaComSucesso()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 1, TurmaId = 1 };
            var aluno = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turma = new Turma { Id = 1, Nome = "3A - Ensino Medio", VagasDisponiveis = 5 };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(aluno);

            _turmaRepositoryMock
                .Setup(r => r.GetByIdComTransacao(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(turma);

            _matriculaRepositoryMock
                .Setup(r => r.ExisteAlunoNaTurma(1, 1, _connectionMock.Object, _transactionMock.Object))
                .Returns(false);

            _turmaRepositoryMock
                .Setup(r => r.DecrementarVaga(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(true);

            _matriculaRepositoryMock
                .Setup(r => r.InsertMatricula(It.IsAny<Matricula>(), _connectionMock.Object, _transactionMock.Object))
                .Returns(100);

            // Act
            var resultado = _service.Matricular(dto);

            // Assert
            Assert.IsNotNull(resultado);
            Assert.AreEqual(100, resultado.Id);
            Assert.AreEqual(1, resultado.AlunoId);
            Assert.AreEqual(1, resultado.TurmaId);
            Assert.AreEqual(4, resultado.VagasRestantes); // 5 - 1
            _transactionMock.Verify(t => t.Commit(), Times.Once);
            _transactionMock.Verify(t => t.Rollback(), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Matricular_ComAlunoInexistente_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 999, TurmaId = 1 };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(999, _connectionMock.Object, _transactionMock.Object))
                .Returns((Aluno)null);

            // Act
            _service.Matricular(dto);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Matricular_ComAlunoInativo_DeveLancarInvalidOperationException()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 4, TurmaId = 1 };
            var alunoInativo = new Aluno { Id = 4, Nome = "Diego Ferreira", Ativo = false };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(4, _connectionMock.Object, _transactionMock.Object))
                .Returns(alunoInativo);

            // Act
            _service.Matricular(dto);

        }

        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void Matricular_ComTurmaInexistente_DeveLancarKeyNotFoundException()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 1, TurmaId = 999 };
            var aluno = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(aluno);

            _turmaRepositoryMock
                .Setup(r => r.GetByIdComTransacao(999, _connectionMock.Object, _transactionMock.Object))
                .Returns((Turma)null);

            // Act
            _service.Matricular(dto);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Matricular_ComTurmaSemVaga_DeveLancarInvalidOperationException()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 1, TurmaId = 4 };
            var aluno = new Aluno { Id = 1, Nome = "Ana Souza", Ativo = true };
            var turmaLotada = new Turma { Id = 4, Nome = "Turma Lotada", VagasDisponiveis = 0 };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(aluno);

            _turmaRepositoryMock
                .Setup(r => r.GetByIdComTransacao(4, _connectionMock.Object, _transactionMock.Object))
                .Returns(turmaLotada);

            _matriculaRepositoryMock
                .Setup(r => r.ExisteAlunoNaTurma(1, 4, _connectionMock.Object, _transactionMock.Object))
                .Returns(false);

            _turmaRepositoryMock
                .Setup(r => r.DecrementarVaga(4, _connectionMock.Object, _transactionMock.Object))
                .Returns(false); 

            // Act
            _service.Matricular(dto);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Matricular_ComAlunoJaMatriculadoNaTurma_DeveLancarInvalidOperationException()
        {
            // Arrange
            var dto = new MatriculaCreateDto { AlunoId = 2, TurmaId = 1 };
            var aluno = new Aluno { Id = 2, Nome = "Bruno Lima", Ativo = true };
            var turma = new Turma { Id = 1, Nome = "3A - Ensino Medio", VagasDisponiveis = 5 };

            _alunoRepositoryMock
                .Setup(r => r.GetByIdComTransacao(2, _connectionMock.Object, _transactionMock.Object))
                .Returns(aluno);

            _turmaRepositoryMock
                .Setup(r => r.GetByIdComTransacao(1, _connectionMock.Object, _transactionMock.Object))
                .Returns(turma);

            _matriculaRepositoryMock
                .Setup(r => r.ExisteAlunoNaTurma(2, 1, _connectionMock.Object, _transactionMock.Object))
                .Returns(true); 

            // Act
            _service.Matricular(dto);

        }
    }
}