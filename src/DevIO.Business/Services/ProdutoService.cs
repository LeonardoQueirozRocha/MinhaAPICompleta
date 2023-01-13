using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using DevIO.Business.Models.Validations;
using DevIO.Business.Services.Base;

namespace DevIO.Business.Services
{
    public class ProdutoService : BaseService, IProdutoService
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IFileService _fileService;

        public ProdutoService(
            IProdutoRepository produtoRepository,
            IFileService fileService,
            INotifier notifier) : base(notifier)
        {
            _produtoRepository = produtoRepository;
            _fileService = fileService;
        }

        public async Task<IEnumerable<Produto>> GetProdutosFornecedoresAsync()
        {
            return await _produtoRepository.GetProdutosFornecedoresAsync();
        }

        public async Task<Produto> GetProdutoFornecedorAsync(Guid id)
        {
            return await _produtoRepository.GetProdutoFornecedorAsync(id);
        }

        public async Task AddAsync(Produto produto)
        {
            if (!Validate(new ProdutoValidator(), produto)) return;
               
            await _produtoRepository.AddAsync(produto);
        }

        public async Task UpdateAsync(Produto produto)
        {
            if (!Validate(new ProdutoValidator(), produto)) return;

            await _produtoRepository.UpdateAsync(produto);
        }

        public async Task DeleteAsync(Guid id)
        {
            _fileService.Delete(_produtoRepository.GetByIdAsync(id).Result.Imagem);
            await _produtoRepository.DeleteAsync(id);
        }

        public void Dispose()
        {
            _produtoRepository?.Dispose();
        }
    }
}
