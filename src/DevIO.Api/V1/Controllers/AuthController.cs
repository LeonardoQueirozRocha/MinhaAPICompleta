using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.Dtos;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.v1.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/")]
public class AuthController : MainController
{
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public AuthController(
        INotifier notifier,
        IUser appUser,
        IAuthService authService,
        IMapper mapper) : base(notifier, appUser)
    {
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("new-account")]
    public async Task<ActionResult> RegisterAsync(RegisterUserDto registerUser)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponse(ModelState);
        }

        var token = await _authService.CreateAsync(registerUser.Email, registerUser.Password);

        if (!IsValid())
        {
            return CustomResponse(registerUser);
        }

        return CustomResponse(_mapper.Map<LoginResponseDto>(token));
    }

    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync(LoginUserDto loginUser)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponse(ModelState);
        }

        var token = await _authService.LoginAsync(loginUser.Email, loginUser.Password);

        if (!IsValid())
        {
            return CustomResponse(loginUser);
        }

        return CustomResponse(_mapper.Map<LoginResponseDto>(token));
    }
}