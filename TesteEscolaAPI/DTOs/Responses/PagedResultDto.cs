using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TesteEscolaAPI.DTOs.Responses
{
    public class PagedResultDto<T>
    {
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int TamanhoPagina { get; set; }
        public List<T> Itens { get; set; }
    }
}