using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Dtos;
using DevIO.Api.Extensions.Authorization;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.V1.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/produtos")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoService _produtoService;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProdutosController(
            IProdutoService produtoService,
            IFileService fileService,
            IMapper mapper,
            INotifier notifier,
            IUser appUser) : base(notifier, appUser)
        {
            _produtoService = produtoService;
            _fileService = fileService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoDto>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<ProdutoDto>>(await _produtoService.GetProdutosFornecedoresAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> GetByIdAsync(Guid id)
        {
            var produtoDto = await GetProdutoAsync(id);

            if (produtoDto == null) return NotFound();

            return produtoDto;
        }

        [ClaimsAuthorize("Produto", "Create")]
        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> CreateAsync(ProdutoDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imageName = $"{Guid.NewGuid()}_{produtoDto.Imagem}";

            if (!await _fileService.UploadAsync(produtoDto.ImagemUpload, imageName)) return CustomResponse(produtoDto);

            produtoDto.Imagem = imageName;

            await _produtoService.AddAsync(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [ClaimsAuthorize("Produto", "Create")]
        [HttpPost("streaming")]
        public async Task<ActionResult<ProdutoDto>> CreateStreamingAsync(ProdutoImagemDto produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imgPrefix = $"{Guid.NewGuid()}_";

            if (!await _fileService.UploadStreamingAsync(produtoDto.ImagemUpload, imgPrefix)) return CustomResponse(produtoDto);

            produtoDto.Imagem = imgPrefix + produtoDto.ImagemUpload.FileName;
            await _produtoService.AddAsync(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [ClaimsAuthorize("Produto", "Update")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, ProdutoDto produtoDto)
        {
            if (id != produtoDto.Id)
            {
                ReportError("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(produtoDto);
            }

            var produtoUpdate = await GetProdutoAsync(id);
            produtoDto.Imagem = produtoUpdate.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoUpdate.ImagemUpload != null)
            {
                var imgName = Guid.NewGuid() + "_" + produtoDto.Imagem;

                if (!await _fileService.UploadAsync(produtoDto.ImagemUpload, imgName)) return CustomResponse(produtoDto);

                produtoUpdate.Imagem = imgName;
            }

            produtoUpdate.Nome = produtoDto.Nome;
            produtoUpdate.Descricao = produtoDto.Descricao;
            produtoUpdate.Valor = produtoDto.Valor;
            produtoUpdate.Ativo = produtoDto.Ativo;

            await _produtoService.UpdateAsync(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [ClaimsAuthorize("Produto", "Delete")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoDto>> DeleteAsync(Guid id)
        {
            var produtoDto = await GetProdutoAsync(id);

            if (produtoDto == null) return NotFound();

            _fileService.Delete(produtoDto.Imagem);

            await _produtoService.DeleteAsync(id);

            return CustomResponse(produtoDto);
        }

        private async Task<ProdutoDto> GetProdutoAsync(Guid id)
        {
            return _mapper.Map<ProdutoDto>(await _produtoService.GetProdutoFornecedorAsync(id));
        }
    }
}
