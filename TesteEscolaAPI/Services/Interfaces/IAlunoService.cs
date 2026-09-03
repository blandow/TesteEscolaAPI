using TesteEscolaAPI.DTOs.Responses;
using TesteEscolaAPI.DTOs.Requests;


namespace TesteEscolaAPI.Services.Interfaces
{
    public interface IAlunoService
    {
        PagedResultDto<AlunoDto> GetAll(string nome, int pagina, int tamanhoPagina);
        AlunoDto GetById(int id);
        AlunoDto Create(AlunoCreateDto dto);
        void Update(int id, AlunoUpdateDto dto);
        void Delete(int id);
    }
}
