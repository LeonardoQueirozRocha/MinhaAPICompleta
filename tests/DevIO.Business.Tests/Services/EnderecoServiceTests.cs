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
        result.ToList().ForEach(address => address.Fornecedor.Should().BeNull());

        _enderecoRepository.Verify(
            service => service.GetAllAsync(),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return empty")]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        // Arrange
        var enderecos = EnderecoBuilder.Instance
            .BuildCollection(0)
            .ToList();

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

    [Theory(DisplayName = $"{ClassName} UpdateAsync Numero Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec placerat ligula tellus, eget iaculis arcu congue ut. Etiam quis commodo ipsum. Praesent sed metus consectetur, vestibulum felis non, dictum arcu. Nam a nulla ac purus faucibus vulputate. In pharetra ligula non posuere laoreet. Aliquam in commodo eros. Fusce condimentum auctor quam. Sed vitae augue vitae mauris pellentesque interdum. Fusce auctor risus eleifend nisl bibendum iaculis. Pellentesque varius dolor et luctus consectetur. Duis et enim at tellus tempus aliquam eget a dolor. Proin sit amet lacus at augue tincidunt pellentesque nec non elit. Nulla malesuada maximus dui, quis pretium dolor dapibus nec.", 1)]
    public async Task UpdateAsync_Numero_ShouldNotifyErrorMessage(
        string invalidNumero,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Numero precisa ser fornecido",
            "O campo Numero precisa ter entre 1 e 50 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Numero = invalidNumero;

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

    [Theory(DisplayName = $"{ClassName} UpdateAsync Cep Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("9999999", 1)]
    [InlineData("999999999", 1)]
    public async Task UpdateAsync_Cep_ShouldNotifyErrorMessage(
        string invalidCep,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Cep precisa ser fornecido",
            "O campo Cep precisa ter 8 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Cep = invalidCep;

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

    [Theory(DisplayName = $"{ClassName} UpdateAsync Bairro Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("L", 1)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec placerat ligula tellus, eget iaculis arcu congue ut. Etiam quis commodo ipsum. Praesent sed metus consectetur, vestibulum felis non, dictum arcu. Nam a nulla ac purus faucibus vulputate. In pharetra ligula non posuere laoreet. Aliquam in commodo eros. Fusce condimentum auctor quam. Sed vitae augue vitae mauris pellentesque interdum. Fusce auctor risus eleifend nisl bibendum iaculis. Pellentesque varius dolor et luctus consectetur. Duis et enim at tellus tempus aliquam eget a dolor. Proin sit amet lacus at augue tincidunt pellentesque nec non elit. Nulla malesuada maximus dui, quis pretium dolor dapibus nec.", 1)]
    public async Task UpdateAsync_Bairro_ShouldNotifyErrorMessage(
        string invalidBairro,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Bairro precisa ser fornecido",
            "O campo Bairro precisa ter entre 2 e 100 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Bairro = invalidBairro;

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

    [Theory(DisplayName = $"{ClassName} UpdateAsync Cidade Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("L", 1)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec placerat ligula tellus, eget iaculis arcu congue ut. Etiam quis commodo ipsum. Praesent sed metus consectetur, vestibulum felis non, dictum arcu. Nam a nulla ac purus faucibus vulputate. In pharetra ligula non posuere laoreet. Aliquam in commodo eros. Fusce condimentum auctor quam. Sed vitae augue vitae mauris pellentesque interdum. Fusce auctor risus eleifend nisl bibendum iaculis. Pellentesque varius dolor et luctus consectetur. Duis et enim at tellus tempus aliquam eget a dolor. Proin sit amet lacus at augue tincidunt pellentesque nec non elit. Nulla malesuada maximus dui, quis pretium dolor dapibus nec.", 1)]
    public async Task UpdateAsync_Cidade_ShouldNotifyErrorMessage(
        string invalidCidade,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Cidade precisa ser fornecido",
            "O campo Cidade precisa ter entre 2 e 100 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Cidade = invalidCidade;

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

    [Theory(DisplayName = $"{ClassName} UpdateAsync Estado Should notify error message")]
    [InlineData("", 2)]
    [InlineData(null, 1)]
    [InlineData("L", 1)]
    [InlineData("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec placerat ligula tellus, eget iaculis arcu congue ut. Etiam quis commodo ipsum. Praesent sed metus consectetur, vestibulum felis non, dictum arcu. Nam a nulla ac purus faucibus vulputate. In pharetra ligula non posuere laoreet. Aliquam in commodo eros. Fusce condimentum auctor quam. Sed vitae augue vitae mauris pellentesque interdum. Fusce auctor risus eleifend nisl bibendum iaculis. Pellentesque varius dolor et luctus consectetur. Duis et enim at tellus tempus aliquam eget a dolor. Proin sit amet lacus at augue tincidunt pellentesque nec non elit. Nulla malesuada maximus dui, quis pretium dolor dapibus nec.", 1)]
    public async Task UpdateAsync_Estado_ShouldNotifyErrorMessage(
        string invalidEstado,
        int expectedTimes)
    {
        // Arrange
        var expectedErrorMessages = new List<string>()
        {
            "O campo Estado precisa ser fornecido",
            "O campo Estado precisa ter entre 2 e 50 caracteres"
        };

        var endereco = EnderecoBuilder.Instance.Build();
        endereco.Estado = invalidEstado;

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