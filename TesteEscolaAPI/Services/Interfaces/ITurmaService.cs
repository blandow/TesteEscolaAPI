using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TesteEscolaAPI.DTOs.Responses;

namespace TesteEscolaAPI.Services.Interfaces
{
    public interface ITurmaService
    {
        IEnumerable<TurmaDto> GetAll();
    }
}
