using System;


namespace TesteEscolaAPI.DTOs.Requests
{
    public class AlunoUpdateDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}