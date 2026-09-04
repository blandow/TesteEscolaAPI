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
    public class MatriculaRepository : IMatriculaRepository
    {
        public bool ExisteAlunoNaTurma(int alunoId, int turmaId, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM dbo.Matricula
                WHERE AlunoId = @AlunoId AND TurmaId = @TurmaId";

            var count = connection.ExecuteScalar<int>(sql, new { AlunoId = alunoId, TurmaId = turmaId }, transaction);
            return count > 0;
        }

        public int InsertMatricula(Matricula matricula, IDbConnection connection, IDbTransaction transaction)
        {
            var sql = @"
                INSERT INTO dbo.Matricula (AlunoId, TurmaId, DataMatricula)
                VALUES (@AlunoId, @TurmaId, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return connection.ExecuteScalar<int>(sql, matricula, transaction);
        }
    }
}