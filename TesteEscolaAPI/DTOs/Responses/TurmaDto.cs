
namespace TesteEscolaAPI.DTOs.Responses
{
    public class TurmaDto
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Periodo { get; set; }
        public int VagasTotal { get; set; }
        public int VagasDisponiveis { get; set; }
    }
}