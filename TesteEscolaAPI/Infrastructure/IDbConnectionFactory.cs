
using System.Data;

namespace TesteEscolaAPI.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
