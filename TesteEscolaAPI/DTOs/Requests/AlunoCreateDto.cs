using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TesteEscolaAPI.DTOs.Requests
{
    public class AlunoCreateDto
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public DateTime DataNascimento { get; set; }
    }
}