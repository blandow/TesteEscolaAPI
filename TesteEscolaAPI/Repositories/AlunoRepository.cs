using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;

namespace TesteEscolaAPI.Repositories
{
    public class AlunoRepository : IAlunoRepository
    {

        private readonly IDbConnectionFactory _connectionFactory;

        public AlunoRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<Aluno> GetAll(string nameFilter, int page, int size, out int total)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var temFiltro = !string.IsNullOrWhiteSpace(nameFilter);
                var whereClause = temFiltro ? "WHERE Nome LIKE @Nome" : "";

                var parametros = new DynamicParameters();
                if (temFiltro) parametros.Add("Nome", $"%{nameFilter}%");

                total = connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.Aluno {whereClause}", parametros);

                parametros.Add("Offset", (page - 1) * size);
                parametros.Add("TamanhoPagina", size);

                var sql = $@"
                SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro
                FROM dbo.Aluno
                {whereClause}
                ORDER BY Nome
                OFFSET @Offset ROWS FETCH NEXT @TamanhoPagina ROWS ONLY";

                return connection.Query<Aluno>(sql, parametros).ToList();
            }
        }

        public Aluno GetById(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                var sql = "SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro FROM dbo.Aluno WHERE Id = @Id";
                return connection.QueryFirstOrDefault<Aluno>(sql, new { Id = id });
            }
        }

        public int Insert(Aluno aluno)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                var sql = @"
                INSERT INTO dbo.Aluno (Nome, Email, DataNascimento, Ativo, DataCadastro)
                VALUES (@Nome, @Email, @DataNascimento, 1, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";
                return connection.ExecuteScalar<int>(sql, aluno);
            }
        }

        public bool Update(Aluno aluno)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                var sql = "UPDATE dbo.Aluno SET Nome = @Nome, Email = @Email, DataNascimento = @DataNascimento WHERE Id = @Id";
                return connection.Execute(sql, aluno) > 0;
            }
        }

        public bool SoftDelete(int id)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();
                return connection.Execute("UPDATE dbo.Aluno SET Ativo = 0 WHERE Id = @Id", new { Id = id }) > 0;
            }
        }
        public Aluno GetByIdComTransacao(int id, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "SELECT Id, Nome, Email, DataNascimento, Ativo, DataCadastro FROM dbo.Aluno WHERE Id = @Id";
            return connection.QueryFirstOrDefault<Aluno>(sql, new { Id = id }, transaction);
        }
    }
}
