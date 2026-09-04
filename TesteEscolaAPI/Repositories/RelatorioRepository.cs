using System.Collections.Generic;
using System.Linq;
using Dapper;
using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.Infrastructure;
using TesteEscolaAPI.Repositories.Interfaces;

namespace TesteEscolaAPI.Repositories
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public RelatorioRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<RelatorioAlunosPorTurmaDto> GetAlunosPorTurma()
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                connection.Open();

                var sql = @"
                    SELECT
                        t.Nome AS NomeTurma,
                        COUNT(m.Id) AS QuantidadeAlunosMatriculados,
                        t.VagasDisponiveis AS VagasRestantes
                    FROM dbo.Turma t
                    LEFT JOIN dbo.Matricula m ON m.TurmaId = t.Id
                    GROUP BY t.Id, t.Nome, t.VagasDisponiveis
                    ORDER BY t.Nome";

                return connection.Query<RelatorioAlunosPorTurmaDto>(sql).ToList();
            }
        }
    }
}