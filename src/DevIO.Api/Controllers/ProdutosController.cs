using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.Dtos;
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

        [RequestSizeLimit(40000000)]
        [HttpPost("image")]
        public async Task<ActionResult> AddImageAsync(IFormFile file)
        {
            return Ok(file);
        }

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
