using AutoMapper;
using DevIO.Api.Controllers.Base;
using DevIO.Api.Dtos;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers.v1;

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

        return CustomResponse(_mapper.Map<LoginResponseDto>(token));
    }
}