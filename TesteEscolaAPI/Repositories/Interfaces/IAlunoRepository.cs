using System.Collections.Generic;
using TesteEscolaAPI.Models;

namespace TesteEscolaAPI.Repositories.Interfaces
{
    public interface IAlunoRepository
    {

        IEnumerable<Aluno> GetAll(string nomeFiltro, int pagina, int tamanhoPagina, out int total);
        Aluno GetById(int id);
        int Insert(Aluno aluno);
        bool Update(Aluno aluno);
        bool SoftDelete(int id);

    }
}
