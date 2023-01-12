using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.ViewModels;
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
            IMapper mapper)
        {
            _fornecedorService = fornecedorService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<FornecedorViewModel>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<FornecedorViewModel>>(await _fornecedorService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> GetbyIdAsync(Guid id)
        {
            var fornecedorViewModel = await GetFornecedorProdutosEnderecoAsync(id);

            if (fornecedorViewModel == null) return NotFound();

            return fornecedorViewModel;
        }

        [HttpPost]
        public async Task<ActionResult<FornecedorViewModel>> CreateAsync(FornecedorViewModel fornecedorViewModel)
        {
            if (!ModelState.IsValid) return BadRequest();

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            var result = await _fornecedorService.AddAsync(fornecedor);

            if (!result) return BadRequest();

            return Ok(fornecedorViewModel);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorViewModel>> UpdateAsync(Guid id, FornecedorViewModel fornecedorViewModel)
        {
            if (id != fornecedorViewModel.Id || !ModelState.IsValid) return BadRequest();

            var fornecedor = _mapper.Map<Fornecedor>(fornecedorViewModel);
            var result = await _fornecedorService.UpdateAsync(fornecedor);

            if (!result) return BadRequest();

            return Ok(fornecedor);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<Fornecedor>> DeleteAsync(Guid id)
        {
            var fornecedorViewModel = await GetFornecedorEnderecoAsync(id);

            if (fornecedorViewModel == null) return NotFound();

            var result = await _fornecedorService.DeleteAsync(id);

            if (!result) return BadRequest();

            return Ok(fornecedorViewModel);
        }

        private async Task<FornecedorViewModel> GetFornecedorProdutosEnderecoAsync(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorService.GetFornecedorProdutosEnderecoAsync(id));
        }

        private async Task<FornecedorViewModel> GetFornecedorEnderecoAsync(Guid id)
        {
            return _mapper.Map<FornecedorViewModel>(await _fornecedorService.GetFornecedorEnderecoAsync(id));
        }
    }
}
