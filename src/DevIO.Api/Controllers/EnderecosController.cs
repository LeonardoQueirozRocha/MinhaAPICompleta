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
    public class EnderecosController : MainController
    {
        private readonly IEnderecoService _enderecoService;
        private readonly IMapper _mapper;

        public EnderecosController(
            IEnderecoService enderecoService, 
            IMapper mapper, 
            INotifier notifier) : base(notifier)
        {
            _enderecoService = enderecoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<EnderecoDto>> GetAllAsync()
        {
            return _mapper.Map<IEnumerable<EnderecoDto>>(await _enderecoService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        public async Task<EnderecoDto> GetByIdAsync(Guid id)
        {
            return _mapper.Map<EnderecoDto>(await _enderecoService.GetByIdAsync(id));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, EnderecoDto enderecoViewModel)
        {
            if (id != enderecoViewModel.Id)
            {
                ReportError("O id informado não é o mesmo que foi passado na query");
                return CustomResponse(enderecoViewModel);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _enderecoService.UpdateAsync(_mapper.Map<Endereco>(enderecoViewModel));

            return CustomResponse(enderecoViewModel);
        }
    }
}
