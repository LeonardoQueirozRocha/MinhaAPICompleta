using DevIO.Business.Models;

namespace DevIO.Business.Interfaces.Services
{
    public interface IEnderecoService : IDisposable
    {
        Task<Endereco> GetByIdAsync(Guid id);
        Task UpdateAsync(Endereco endereco);
    }
}
