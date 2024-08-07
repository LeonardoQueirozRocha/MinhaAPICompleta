using System.Security.Claims;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Models.Auth;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Configurations;
using DevIO.Utils.Tests.Builders.Business.Models.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.AutoMock;

namespace DevIO.Business.Tests.Services;

public class AuthServiceTests
{
    private const string ClassName = nameof(AuthService);

    private readonly Mock<SignInManager<IdentityUser>> _signInManager;
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<INotifier> _notifier;
    private readonly AppSettings _appSettings;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var mocker = new AutoMocker();
        mocker.Use(AppSettingsBuilder.Instance.Build());
        _authService = mocker.CreateInstance<AuthService>();
        _signInManager = mocker.GetMock<SignInManager<IdentityUser>>();
        _userManager = mocker.GetMock<UserManager<IdentityUser>>();
        _notifier = mocker.GetMock<INotifier>();
        _appSettings = mocker.Get<AppSettings>();
    }

    [Fact(DisplayName = $"{ClassName} CreateAsync Should Generate Jwt Successfully")]
    public async Task CreateAsync_ShouldGenerateJwtSuccessfully()
    {
        // Arrange
        const string password = "Teste@123";

        var identityUser = IdentityUserBuilder.Instance.Build();
        var claims = ClaimBuilder.Instance.BuildCollection().ToList();
        var userRoles = claims.Select(claim => claim.Value).ToList();

        _userManager
            .Setup(service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        MockSuccessIdentity(identityUser, claims, userRoles);

        // Act
        var result = await _authService.CreateAsync(identityUser.Email, password);

        // Assert
        _userManager.Verify(
            service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()),
            Times.Once());

        VerifySuccessIdentity(result, identityUser.Email, identityUser.Id, claims);
    }

    [Fact(DisplayName = $"{ClassName} CreateAsync Should notify error")]
    public async Task CreateAsync_ShouldNotifyError()
    {
        // Arrange
        const string password = "test";

        var identityUser = IdentityUserBuilder.Instance.Build();
        var identityError = IdentityErrorBuilder.Instance.Build();

        _userManager
            .Setup(service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        MockIdentityError(identityError.Description);

        // Act
        var result = await _authService.CreateAsync(identityUser.Email, password);

        // Assert
        _userManager.Verify(
            service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()),
            Times.Once());

        VerifyIdentityError(result);
    }

    [Fact(DisplayName = $"{ClassName} LoginAsync Should Generate Jwt Successfully")]
    public async Task LoginAsync_ShouldGenerateJwtSuccessfully()
    {
        // Arrange
        const string password = "Teste@123";

        var identityUser = IdentityUserBuilder.Instance.Build();
        var claims = ClaimBuilder.Instance.BuildCollection().ToList();
        var userRoles = claims.Select(claim => claim.Value).ToList();

        _signInManager
            .Setup(service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        MockSuccessIdentity(identityUser, claims, userRoles);

        // Act
        var result = await _authService.LoginAsync(identityUser.Email, password);

        // Assert
        _signInManager.Verify(
            service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Once());

        VerifySuccessIdentity(result, identityUser.Email, identityUser.Id, claims);
    }

    [Fact(DisplayName = $"{ClassName} LoginAsync Should return null when user is lockout")]
    public async Task LoginAsync_ShouldReturnNullWhenUserIsLockout()
    {
        // Arrange
        const string email = "teste@teste.com";
        const string password = "Teste@123";

        _signInManager
            .Setup(service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.LockedOut);

        MockIdentityError(_appSettings.ValidationMessages.LockedOutMessage);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        _signInManager.Verify(
            service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Once());

        VerifyIdentityError(result);
    }

    [Fact(DisplayName = $"{ClassName} LoginAsync Should return null when login failed")]
    public async Task LoginAsync_ShouldReturnNullWhenLoginFailed()
    {
        // Arrange
        const string email = "teste@teste.com";
        const string password = "Teste@123";

        _signInManager
            .Setup(service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        MockIdentityError(_appSettings.ValidationMessages.IncorrectUserOrPasswordMessage);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        _signInManager.Verify(
            service => service.PasswordSignInAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Once());

        VerifyIdentityError(result);
    }

    private void MockSuccessIdentity(
        IdentityUser identityUser,
        List<Claim> claims,
        List<string> userRoles)
    {
        _userManager
            .Setup(service => service.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(identityUser);

        _userManager
            .Setup(service => service.GetClaimsAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(claims);

        _userManager
            .Setup(service => service.GetRolesAsync(It.IsAny<IdentityUser>()))
            .ReturnsAsync(userRoles);
    }

    private void VerifySuccessIdentity(
        UserLogin result,
        string email,
        string userTokenId,
        List<Claim> claims)
    {
        result.Should().NotBeNull();
        result.UserToken.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.UserToken.Email.Should().Be(email);
        result.UserToken.Id.Should().Be(userTokenId);
        result.UserToken.Claims
            .Select(userClaim => userClaim.Value)
            .Should()
            .Contain(claims.Select(x => x.Value));

        _notifier.Verify(
            service => service.Handle(It.IsAny<Notification>()),
            Times.Never());

        _userManager.Verify(
            service => service.FindByEmailAsync(It.IsAny<string>()),
            Times.Once());

        _userManager.Verify(
            service => service.GetClaimsAsync(It.IsAny<IdentityUser>()),
            Times.Once());

        _userManager.Verify(
            service => service.GetRolesAsync(It.IsAny<IdentityUser>()),
            Times.Once());
    }

    private void MockIdentityError(string expectedErrorMessage)
    {
        _notifier
            .Setup(service => service.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
            {
                notificationCb.Should().NotBeNull();
                notificationCb.Message.Should().Be(expectedErrorMessage);
            });
    }

    private void VerifyIdentityError(UserLogin result)
    {
        result.Should().BeNull();

        _notifier.Verify(
            service => service.Handle(It.IsAny<Notification>()),
            Times.Once());

        _userManager.Verify(
            service => service.FindByEmailAsync(It.IsAny<string>()),
            Times.Never());

        _userManager.Verify(
            service => service.GetClaimsAsync(It.IsAny<IdentityUser>()),
            Times.Never());

        _userManager.Verify(
            service => service.GetRolesAsync(It.IsAny<IdentityUser>()),
            Times.Never());
    }
}