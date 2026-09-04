using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using TesteEscolaAPI.Models;
using TesteEscolaAPI.Repositories.Interfaces;

namespace TesteEscolaAPI.Repositories
{
    public class TurmaRepository : ITurmaRepository
    {
        public Turma GetByIdComTransacao(int id, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = "SELECT Id, Nome, Periodo, VagasTotal, VagasDisponiveis FROM dbo.Turma WHERE Id = @Id";
            return connection.QueryFirstOrDefault<Turma>(sql, new { Id = id }, transaction);

        }

        public bool DecrementarVaga(int turmaId, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
        UPDATE dbo.Turma
        SET VagasDisponiveis = VagasDisponiveis - 1
        WHERE Id = @TurmaId AND VagasDisponiveis > 0";

            var linhasAfetadas = connection.Execute(sql, new { TurmaId = turmaId }, transaction);
            return linhasAfetadas > 0;
        }
    }
}