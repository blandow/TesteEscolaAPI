using TesteEscolaAPI.DTOs.Requests;
using TesteEscolaAPI.DTOs.Responses;

namespace TesteEscolaAPI.Services.Interfaces
{
    public interface IMatriculaService
    {
        MatriculaResultDto Matricular(MatriculaCreateDto dto);
    }
}
