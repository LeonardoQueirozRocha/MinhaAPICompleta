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
            var fornecedorViewModel = await GetFornecedorProdutosEnderecoAsync(id);

            if (fornecedorViewModel == null) return NotFound();

            return fornecedorViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorDto>> CreateAsync(FornecedorDto fornecedorViewModel)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.AddAsync(_mapper.Map<Fornecedor>(fornecedorViewModel));

            return CustomResponse(fornecedorViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDto>> UpdateAsync(Guid id, FornecedorDto fornecedorViewModel)
        {
            if (id != fornecedorViewModel.Id)
            {
                ReportError("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(fornecedorViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.UpdateAsync(_mapper.Map<Fornecedor>(fornecedorViewModel));

            return CustomResponse(fornecedorViewModel);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Fornecedor>> DeleteAsync(Guid id)
        {
            var fornecedorViewModel = await GetFornecedorEnderecoAsync(id);

            if (fornecedorViewModel == null) return NotFound();

            await _fornecedorService.DeleteAsync(id);

            return CustomResponse(fornecedorViewModel);
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
