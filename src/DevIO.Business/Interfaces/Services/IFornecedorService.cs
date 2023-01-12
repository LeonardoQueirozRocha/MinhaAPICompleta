using DevIO.Business.Models;

namespace DevIO.Business.Interfaces.Services
{
    public interface IFornecedorService : IDisposable
    {
        Task<IEnumerable<Fornecedor>> GetAllAsync();
        Task<Fornecedor> GetFornecedorEnderecoAsync(Guid id);
        Task<Fornecedor> GetFornecedorProdutosEnderecoAsync(Guid id);
        Task<bool> AddAsync(Fornecedor fornecedor);
        Task<bool> UpdateAsync(Fornecedor fornecedor);
        Task<bool> DeleteAsync(Guid id);
        Task UpdateEnderecoAsync(Endereco endereco);
    }
}
