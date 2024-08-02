using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Models;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Models;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace DevIO.Business.Tests.Services;

public class EnderecoServiceTests
{
    private const string ClassName = nameof(EnderecoService);

    private readonly Mock<IEnderecoRepository> _enderecoRepository;
    private readonly Mock<INotifier> _notifier;
    private readonly EnderecoService _enderecoService;

    public EnderecoServiceTests()
    {
        var mocker = new AutoMocker();
        _enderecoService = mocker.CreateInstance<EnderecoService>();
        _enderecoRepository = mocker.GetMock<IEnderecoRepository>();
        _notifier = mocker.GetMock<INotifier>();
    }

    #region GetAllAsync

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return all addresses")]
    public async Task GetAllAsync_ShouldReturnAllAddresses()
    {
        // Arrange
        var enderecos = EnderecoBuilder.Instance
            .BuildCollection()
            .ToList();

        _enderecoRepository
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(enderecos);

        // Act
        var result = await _enderecoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Any().Should().BeTrue();
        result.Count().Should().Be(enderecos.Count);

        _enderecoRepository.Verify(
            service => service.GetAllAsync(),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return empty")]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        // Arrange
        var enderecos = new List<Endereco>();

        _enderecoRepository
            .Setup(service => service.GetAllAsync())
            .ReturnsAsync(enderecos);

        // Act
        var result = await _enderecoService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Any().Should().BeFalse();
        result.Count().Should().Be(enderecos.Count);

        _enderecoRepository.Verify(
            service => service.GetAllAsync(),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return null")]
    public async Task GetAllAsync_ShouldReturnNull()
    {
        // Arrange && Act
        var result = await _enderecoService.GetAllAsync();

        // Assert
        result.Should().BeNull();

        _enderecoRepository.Verify(
            service => service.GetAllAsync(),
            Times.Once());
    }

    #endregion

    #region GetByIdAsync

    [Fact(DisplayName = $"{ClassName} GetByIdAsync Should return an address")]
    public async Task GetByIdAsync_ShouldReturnAnAddress()
    {
        // Arrange
        var endereco = EnderecoBuilder.Instance.Build();

        _enderecoRepository
            .Setup(service => service.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(endereco);

        // Act
        var result = await _enderecoService.GetByIdAsync(endereco.Id);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(endereco.Id);

        _enderecoRepository.Verify(
            service => service.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetByIdAsync Should return null")]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        // Arrange && Act
        var result = await _enderecoService.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();

        _enderecoRepository.Verify(
            service => service.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = $"{ClassName} UpdateAsync Should update successfully")]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        // Arrange
        var endereco = EnderecoBuilder.Instance.Build();

        _enderecoRepository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Endereco>()))
            .Callback((Endereco enderecoCb) =>
            {
                enderecoCb.Should().NotBeNull();
                enderecoCb.Should().Be(endereco);
            });

        // Act
        await _enderecoService.UpdateAsync(endereco);

        // Assert
        _enderecoRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Endereco>()),
            Times.Once());

        _notifier.Verify(
            service => service.Handle(It.IsAny<Notification>()),
            Times.Never());
    }

    [Theory(DisplayName = $"{ClassName} UpdateAsync Logradouro Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("L", 1)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec placerat ligula tellus, eget iaculis arcu congue ut. Etiam quis commodo ipsum. Praesent sed metus consectetur, vestibulum felis non, dictum arcu. Nam a nulla ac purus faucibus vulputate. In pharetra ligula non posuere laoreet. Aliquam in commodo eros. Fusce condimentum auctor quam. Sed vitae augue vitae mauris pellentesque interdum. Fusce auctor risus eleifend nisl bibendum iaculis. Pellentesque varius dolor et luctus consectetur. Duis et enim at tellus tempus aliquam eget a dolor. Proin sit amet lacus at augue tincidunt pellentesque nec non elit. Nulla malesuada maximus dui, quis pretium dolor dapibus nec.", 1)]
    public async Task UpdateAsync_Logradouro_ShouldNotifyErrorMessage(
        string invalidLogradouro,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Logradouro precisa ser fornecido",
            "O campo Logradouro precisa ter entre 2 e 200 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Logradouro = invalidLogradouro;

        _notifier
            .Setup(serive => serive.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
            {
                notificationCb.Should().NotBeNull();
                expectedErrorMessages.Should().Contain(notificationCb.Message);
            });

        // Act
        await _enderecoService.UpdateAsync(endereco);

        // Assert
        _enderecoRepository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Endereco>()),
            Times.Never());

        _notifier.Verify(
            service => service.Handle(It.IsAny<Notification>()),
            Times.Exactly(expectedTimes));
    }

    #endregion 
}