using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using DevIO.Business.Models.Validations;
using DevIO.Business.Services.Base;

namespace DevIO.Business.Services;

public class FornecedorService : BaseService, IFornecedorService
{
    private readonly IFornecedorRepository _fornecedorRepository;
    private readonly IEnderecoRepository _enderecoRepository;
    private readonly ValidationMessages _validationMessages;

    public FornecedorService(
        IFornecedorRepository fornecedorRepository,
        IEnderecoRepository enderecoRepository,
        INotifier notifier,
        ValidationMessages validationMessages) : base(notifier)
    {
        _fornecedorRepository = fornecedorRepository;
        _enderecoRepository = enderecoRepository;
        _validationMessages = validationMessages;
    }

    public async Task<IEnumerable<Fornecedor>> GetAllAsync()
    {
        return await _fornecedorRepository.GetAllAsync();
    }

    public async Task<Fornecedor> GetFornecedorEnderecoAsync(Guid id)
    {
        return await _fornecedorRepository.GetFornecedorEnderecoAsync(id);
    }

    public async Task<Fornecedor> GetFornecedorProdutosEnderecoAsync(Guid id)
    {
        return await _fornecedorRepository.GetFornecedorProdutosEnderecoAsync(id);
    }

    public async Task<bool> AddAsync(Fornecedor fornecedor)
    {
        if (!Validate(new FornecedorValidator(), fornecedor) ||
            !Validate(new EnderecoValidator(), fornecedor.Endereco)) return false;

        var fornecedores = await _fornecedorRepository.SearchAsync(f => f.Documento == fornecedor.Documento);

        if (fornecedores.Any())
        {
            Notify(_validationMessages.SupplierAlreadyExist);
            return false;
        }

        await _fornecedorRepository.AddAsync(fornecedor);

        return true;
    }

    public async Task<bool> UpdateAsync(Fornecedor fornecedor)
    {
        if (!Validate(new FornecedorValidator(), fornecedor)) return false;

        var fornecedorDb = await _fornecedorRepository.GetByIdAsync(fornecedor.Id);

        if (fornecedorDb is null)
        {
            Notify(_validationMessages.SupplierNotFound);
            return false;
        }

        if (fornecedorDb.Documento.Equals(fornecedor.Documento))
        {
            Notify(_validationMessages.SupplierAlreadyExist);
            return false;
        }

        await _fornecedorRepository.UpdateAsync(fornecedor);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (_fornecedorRepository.GetFornecedorProdutosEnderecoAsync(id).Result.Produtos.Any())
        {
            Notify(_validationMessages.SupplierHasRegisteredProducts);
            return false;
        }

        var endereco = await _enderecoRepository.GetEnderecoByFornecedorAsync(id);

        if (endereco != null)
        {
            await _enderecoRepository.DeleteAsync(endereco.Id);
        }

        await _fornecedorRepository.DeleteAsync(id);

        return true;
    }

    public void Dispose()
    {
        _fornecedorRepository?.Dispose();
        _enderecoRepository?.Dispose();
    }
}
