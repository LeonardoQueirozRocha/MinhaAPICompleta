using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.Dtos;
using DevIO.Api.Extensions.Authorization;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorService _fornecedorService;
        private readonly IMapper _mapper;

        public FornecedoresController(
            IFornecedorService fornecedorService,
            IMapper mapper,
            INotifier notifier) : base(notifier)
        {
            _fornecedorService = fornecedorService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<FornecedorDto>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<FornecedorDto>>(await _fornecedorService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> GetbyIdAsync(Guid id)
        {
            var fornecedorDto = await GetFornecedorProdutosEnderecoAsync(id);

            if (fornecedorDto == null) return NotFound();

            return fornecedorDto;
        }

        [ClaimsAuthorize("Fornecedor", "Create")]
        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> CreateAsync(FornecedorDto fornecedorDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.AddAsync(_mapper.Map<Fornecedor>(fornecedorDto));

            return CustomResponse(fornecedorDto);
        }

        [ClaimsAuthorize("Fornecedor", "Update")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> UpdateAsync(Guid id, FornecedorDto fornecedorDto)
        {
            if (id != fornecedorDto.Id)
            {
                ReportError("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(fornecedorDto);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.UpdateAsync(_mapper.Map<Fornecedor>(fornecedorDto));

            return CustomResponse(fornecedorDto);
        }

        [ClaimsAuthorize("Fornecedor", "Delete")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Fornecedor>> DeleteAsync(Guid id)
        {
            var fornecedorDto = await GetFornecedorEnderecoAsync(id);

            if (fornecedorDto == null) return NotFound();

            await _fornecedorService.DeleteAsync(id);

            return CustomResponse(fornecedorDto);
        }

        private async Task<FornecedorDto> GetFornecedorProdutosEnderecoAsync(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorService.GetFornecedorProdutosEnderecoAsync(id));
        }

        private async Task<FornecedorDto> GetFornecedorEnderecoAsync(Guid id)
        {
            return _mapper.Map<FornecedorDto>(await _fornecedorService.GetFornecedorEnderecoAsync(id));
        }
    }
}
