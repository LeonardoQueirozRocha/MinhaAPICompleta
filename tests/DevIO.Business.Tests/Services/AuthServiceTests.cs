using DevIO.Business.Interfaces.Notifications;
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
    private const string ClassName = nameof(AuthServiceTests);

    private readonly Mock<SignInManager<IdentityUser>> _signInManager;
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<INotifier> _notifier;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var mocker = new AutoMocker();
        mocker.Use(AppSettingsBuilder.Instance.Build());
        _authService = mocker.CreateInstance<AuthService>();
        _signInManager = mocker.GetMock<SignInManager<IdentityUser>>();
        _userManager = mocker.GetMock<UserManager<IdentityUser>>();
        _notifier = mocker.GetMock<INotifier>();
    }

    [Fact(DisplayName = $"{ClassName} CreateAsync Should Generate Jwt Successfully")]
    public async Task CreateAsync_ShouldGenerateJwtSuccessfully()
    {
        // Arrange
        const string password = "Teste@123";

        var identityUser = IdentityUserBuilder.Instance.Build();
        var claims = ClaimBuilder.Instance.BuildCollection().ToList();
        var userRoles = claims.Select(claim => claim.Value).ToList();

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
        var result = await _authService.CreateAsync(identityUser.Email, password);

        // Assert
        result.Should().NotBeNull();
        result.UserToken.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.UserToken.Email.Should().Be(identityUser.Email);
        result.UserToken.Id.Should().Be(identityUser.Id);
        result.UserToken.Claims
            .Select(userClaim => userClaim.Value)
            .Should()
            .Contain(claims.Select(x => x.Value));

        _userManager.Verify(
            service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()),
            Times.Once());

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

    [Fact(DisplayName = $"{ClassName} CreateAsync Should notify error")]
    public async Task CreateAsync_ShouldNotifyError()
    {
        // Arrange
        const string password = "test";

        var identityUser = IdentityUserBuilder.Instance.Build();
        var identityError = IdentityErrorBuilder.Instance.Build();

        _ = _userManager
                .Setup(service => service.CreateAsync(
                    It.IsAny<IdentityUser>(),
                    It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityError));

        _ = _notifier
                .Setup(service => service.Handle(It.IsAny<Notification>()))
                .Callback((Notification notificationCb) =>
                {
                    notificationCb.Should().NotBeNull();
                    notificationCb.Message.Should().Be(identityError.Description);
                });

        // Act
        var result = await _authService.CreateAsync(identityUser.Email, password);

        // Assert
        result.Should().Be(null);

        _userManager.Verify(
            service => service.CreateAsync(
                It.IsAny<IdentityUser>(),
                It.IsAny<string>()),
            Times.Once());

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