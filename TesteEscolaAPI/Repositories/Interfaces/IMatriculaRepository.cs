using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TesteEscolaAPI.Repositories.Interfaces
{
    public interface IMatriculaRepository
    {
        bool ExisteAlunoNaTurma(int alunoId, int turmaId, IDbConnection connection, IDbTransaction transaction);
    }
}
