using System.Security.Claims;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Configurations;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.AutoMock;

namespace DevIO.Business.Tests.Services;

public class AuthServiceTests
{
    private const string ClassName = nameof(AuthServiceTests);

    private readonly Mock<SignInManager<IdentityUser>> _signInManager;
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<INotifier> _notifier;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var mocker = new AutoMocker();
        mocker.Use(AppSettingsBuilder.Build());
        _authService = mocker.CreateInstance<AuthService>();
        _signInManager = mocker.GetMock<SignInManager<IdentityUser>>();
        _userManager = mocker.GetMock<UserManager<IdentityUser>>();
        _notifier = mocker.GetMock<INotifier>();
    }


    [Fact(DisplayName = $"{ClassName} CreateAsync Should Generate Jwt Successfully")]
    public async Task CreateAsync_ShouldGenerateJwtSuccessfully()
    {
        // Arrange
        var email = "teste@teste.com";
        var password = "Teste@123";
        var identityUser = new IdentityUser
        {
            Email = email,
            Id = Guid.NewGuid().ToString(),
        };
        var claims = new List<Claim>() { new("teste", "teste") };
        var userRoles = new List<string> { "teste" };

        _ = _userManager
                .Setup(service => service.CreateAsync(
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

        _ = _userManager
                .Setup(service => service.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(identityUser);

        _ = _userManager
                .Setup(service => service.GetClaimsAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(claims);

        _ = _userManager
                .Setup(service => service.GetRolesAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(userRoles);
        // Act
        var result = await _authService.CreateAsync(email, password);

        // Assert
        result
            .Should()
            .NotBeNull();

        result.UserToken
            .Should()
            .NotBeNull();

        result.AccessToken
            .Should()
            .NotBeNullOrEmpty();

        result.UserToken.Email
            .Should()
            .Be(email);

        result.UserToken.Id
            .Should()
            .Be(identityUser.Id);

        result.UserToken.Claims
            .Select(x => x.Value)
            .Should()
            .Contain(claims.Select(x => x.Value));
    }
}