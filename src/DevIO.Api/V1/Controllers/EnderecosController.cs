using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Dtos;
using DevIO.Api.Extensions.Authorization;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.v1.Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/enderecos")]
public class EnderecosController : MainController
{
    private readonly IEnderecoService _enderecoService;
    private readonly IMapper _mapper;
    private readonly ValidationMessages _validationMessages;

    public EnderecosController(
        IEnderecoService enderecoService,
        IMapper mapper,
        INotifier notifier,
        IUser appUser,
        ValidationMessages validationMessages) : base(notifier, appUser)
    {
        _enderecoService = enderecoService;
        _mapper = mapper;
        _validationMessages = validationMessages;
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

    [ClaimsAuthorize("Fornecedor", "Update")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(Guid id, EnderecoDto enderecoDto)
    {
        if (id != enderecoDto.Id)
        {
            ReportError(_validationMessages.QueryError);
            return CustomResponse(enderecoDto);
        }

        if (!ModelState.IsValid) return CustomResponse(ModelState);

        await _enderecoService.UpdateAsync(_mapper.Map<Endereco>(enderecoDto));

        return CustomResponse(enderecoDto);
    }
}
