using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoService _produtoService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProdutosController(
            IProdutoService produtoService,
            IFileService fileService,
            IMapper mapper,
            INotifier notifier) : base(notifier)
        {
            _produtoService = produtoService;
            _fileService = fileService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoService.GetProdutosFornecedoresAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> GetByIdAsync(Guid id)
        {
            var produtoViewModel = await GetProdutoAsync(id);

            if (produtoViewModel == null) return NotFound();

            return produtoViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> CreateAsync(ProdutoViewModel produtoViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imageName = Guid.NewGuid() + "_" + produtoViewModel.Imagem;

            if (!await _fileService.UploadAsync(produtoViewModel.ImagemUpload, imageName))
                return CustomResponse(produtoViewModel);

            produtoViewModel.Imagem = imageName;

            await _produtoService.AddAsync(_mapper.Map<Produto>(produtoViewModel));

            return CustomResponse(produtoViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> DeleteAsync(Guid id)
        {
            var produtoViewModel = await GetProdutoAsync(id);

            if (produtoViewModel == null) return NotFound();

            _fileService.Delete(produtoViewModel.Imagem);

            await _produtoService.DeleteAsync(id);

            return CustomResponse(produtoViewModel);
        }

        private async Task<ProdutoViewModel> GetProdutoAsync(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoService.GetProdutoFornecedorAsync(id));
        }
    }
}
