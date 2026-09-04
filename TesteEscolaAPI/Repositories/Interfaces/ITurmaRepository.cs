using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteEscolaAPI.Models;

namespace TesteEscolaAPI.Repositories.Interfaces
{
    internal interface ITurmaRepository
    {
        Turma GetByIdComTransacao(int id, IDbConnection connection, IDbTransaction transaction);
        bool DecrementarVaga(int turmaId, IDbConnection connection, IDbTransaction transaction);
    }
}
