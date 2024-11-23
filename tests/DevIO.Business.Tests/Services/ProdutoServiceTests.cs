using Bogus;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Interfaces.User;
using DevIO.Business.Models;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Models;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace DevIO.Business.Tests.Services;

public class ProdutoServiceTests
{
    private const string ClassName = nameof(ProdutoService);

    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly Mock<INotifier> _notifierMock;
    private readonly IProdutoService _produtoService;
    private readonly Faker _faker;

    public ProdutoServiceTests()
    {
        var mocker = new AutoMocker();
        _produtoService = mocker.CreateInstance<ProdutoService>();
        _produtoRepositoryMock = mocker.GetMock<IProdutoRepository>();
        mocker.GetMock<IUser>();
        _notifierMock = mocker.GetMock<INotifier>();
        _faker = new Faker("pt_BR");
    }

    #region GetProdutosFornecedoresAsync

    [Fact(DisplayName = $"{ClassName} GetProdutosFornecedoresAsync should return produtos fornecedores")]
    public async Task GetProdutosFornecedoresAsync_ShouldReturnProdutosFornecedores()
    {
        // Arrange
        var produtos = ProdutoBuilder.Instance.BuildCollection(1).ToList();
        produtos.ForEach(produto => produto.Fornecedor = FornecedorBuilder.Instance.Build());

        _produtoRepositoryMock
            .Setup(repository => repository.GetProdutosFornecedoresAsync())
            .ReturnsAsync(produtos);

        // Act
        var result = await _produtoService.GetProdutosFornecedoresAsync();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().HaveCount(produtos.Count);
        result.Should().BeEquivalentTo(produtos);

        _produtoRepositoryMock.Verify(
            repository => repository.GetProdutosFornecedoresAsync(),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} GetProdutosFornecedoresAsync should return empty when none exists")]
    public async Task GetProdutosFornecedoresAsync_ShouldReturnEmpty_WhenNoneExists()
    {
        // Arrange
        var produtos = Enumerable.Empty<Produto>().ToList();

        _produtoRepositoryMock
            .Setup(repository => repository.GetProdutosFornecedoresAsync())
            .ReturnsAsync(produtos);

        // Act
        var result = await _produtoService.GetProdutosFornecedoresAsync();

        // Assert
        result.Should().BeEmpty();
        result.Should().HaveCount(produtos.Count);
        result.Should().BeEquivalentTo(produtos);

        _produtoRepositoryMock.Verify(
            repository => repository.GetProdutosFornecedoresAsync(),
            Times.Once);
    }

    #endregion

    #region GetProdutoFornecedorAsync

    [Fact(DisplayName = $"{ClassName} GetProdutoFornecedorAsync should return produto fornecedore")]
    public async Task GetProdutoFornecedorAsync_ShouldReturnProdutoFornecedore()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();
        produto.Fornecedor = FornecedorBuilder.Instance.Build();

        _produtoRepositoryMock
            .Setup(repository => repository.GetProdutoFornecedorAsync(It.IsAny<Guid>()))
            .Callback((Guid produtoIdCb) => produtoIdCb.Should().Be(produto.Id))
            .ReturnsAsync(produto);

        // Act
        var result = await _produtoService.GetProdutoFornecedorAsync(produto.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(produto);

        _produtoRepositoryMock.Verify(
            repository => repository.GetProdutoFornecedorAsync(It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} GetProdutosFornecedoresAsync should return null when produto does not exist")]
    public async Task GetProdutoFornecedorAsync_ShouldReturnNull_WhenProdutoDoesNotExist()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        Produto? produto = null;

        _produtoRepositoryMock
            .Setup(repository => repository.GetProdutoFornecedorAsync(It.IsAny<Guid>()))
            .Callback((Guid produtoIdCb) => produtoIdCb.Should().Be(produtoId))
            .ReturnsAsync(produto);

        // Act
        var result = await _produtoService.GetProdutoFornecedorAsync(produtoId);

        // Assert
        result.Should().BeNull();

        _produtoRepositoryMock.Verify(
            repository => repository.GetProdutoFornecedorAsync(It.IsAny<Guid>()),
            Times.Once);
    }

    #endregion

    #region AddAsync

    [Fact(DisplayName = $"{ClassName} AddAsync should not add produto when nome is empty")]
    public async Task AddAsync_ShouldNotAddProduto_WhenNomeIsEmpty()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();
        produto.Nome = string.Empty;

        string[] expectedErrorMessages =
        {
            "O campo Nome precisa ser fornecido",
            "O campo Nome precisa ter entre 2 e 200 caracteres"
        };

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                expectedErrorMessages.Should().Contain(notificationCb.Message));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Exactly(2));
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should not add produto when nome length is invalid")]
    [InlineData(1)]
    [InlineData(201)]
    public async Task AddAsync_ShouldNotAddProduto_WhenNomeLengthIsInvalid(int lenght)
    {
        // Arrange
        const string expectedErrorMessage = "O campo Nome precisa ter entre 2 e 200 caracteres";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Nome = _faker.Random.String2(lenght);

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should not add produto when descrição is empty")]
    public async Task AddAsync_ShouldNotAddProduto_WhenDescricaoIsEmpty()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();
        produto.Descricao = string.Empty;

        string[] expectedErrorMessages =
        {
            "O campo Descricao precisa ser fornecido",
            "O campo Descricao precisa ter entre 2 e 1000 caracteres"
        };

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                expectedErrorMessages.Should().Contain(notificationCb.Message));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Exactly(2));
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should not add produto when descrição length is invalid")]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task AddAsync_ShouldNotAddProduto_WhenDescricaoLengthIsInvalid(int lenght)
    {
        // Arrange
        const string expectedErrorMessage = "O campo Descricao precisa ter entre 2 e 1000 caracteres";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Descricao = _faker.Random.String2(lenght);

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should not add produto when valor is zero")]
    public async Task AddAsync_ShouldNotAddProduto_WhenValorIsZero()
    {
        // Arrange
        const string expectedErrorMessage = "O campo Valor precisa ser maior que 0";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Valor = decimal.Zero;

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should add produto successfully")]
    public async Task AddAsync_ShouldAddProdutoSuccessfully()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();

        _produtoRepositoryMock
            .Setup(repository => repository.AddAsync(It.IsAny<Produto>()))
            .Callback((Produto produtoCb) => produtoCb.Should().BeEquivalentTo(produto));

        // Act
        await _produtoService.AddAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.AddAsync(It.IsAny<Produto>()),
            Times.Once);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Never);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = $"{ClassName} UpdateAsync should not update produto when nome is empty")]
    public async Task UpdateAsync_ShouldNotUpdateProduto_WhenNomeIsEmpty()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();
        produto.Nome = string.Empty;

        string[] expectedErrorMessages =
        {
            "O campo Nome precisa ser fornecido",
            "O campo Nome precisa ter entre 2 e 200 caracteres"
        };

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                expectedErrorMessages.Should().Contain(notificationCb.Message));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Exactly(2));
    }

    [Theory(DisplayName = $"{ClassName} UpdateAsync should not update produto when nome length is invalid")]
    [InlineData(1)]
    [InlineData(201)]
    public async Task UpdateAsync_ShouldNotUpdateProduto_WhenNomeLengthIsInvalid(int lenght)
    {
        // Arrange
        const string expectedErrorMessage = "O campo Nome precisa ter entre 2 e 200 caracteres";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Nome = _faker.Random.String2(lenght);

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should not update produto when descrição is empty")]
    public async Task UpdateAsync_ShouldNotUpdateProduto_WhenDescricaoIsEmpty()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();
        produto.Descricao = string.Empty;

        string[] expectedErrorMessages =
        {
            "O campo Descricao precisa ser fornecido",
            "O campo Descricao precisa ter entre 2 e 1000 caracteres"
        };

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                expectedErrorMessages.Should().Contain(notificationCb.Message));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Exactly(2));
    }

    [Theory(DisplayName = $"{ClassName} UpdateAsync should not update produto when descrição length is invalid")]
    [InlineData(1)]
    [InlineData(1001)]
    public async Task UpdateAsync_ShouldNotUpdateProduto_WhenDescricaoLengthIsInvalid(int lenght)
    {
        // Arrange
        const string expectedErrorMessage = "O campo Descricao precisa ter entre 2 e 1000 caracteres";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Descricao = _faker.Random.String2(lenght);

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should not update produto when valor is zero")]
    public async Task UpdateAsync_ShouldNotUpdateProduto_WhenValorIsZero()
    {
        // Arrange
        const string expectedErrorMessage = "O campo Valor precisa ser maior que 0";
        var produto = ProdutoBuilder.Instance.Build();
        produto.Valor = decimal.Zero;

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(expectedErrorMessage));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Never);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should update produto successfully")]
    public async Task UpdateAsync_ShouldUpdateProdutoSuccessfully()
    {
        // Arrange
        var produto = ProdutoBuilder.Instance.Build();

        _produtoRepositoryMock
            .Setup(repository => repository.UpdateAsync(It.IsAny<Produto>()))
            .Callback((Produto produtoCb) => produtoCb.Should().BeEquivalentTo(produto));

        // Act
        await _produtoService.UpdateAsync(produto);

        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.UpdateAsync(It.IsAny<Produto>()),
            Times.Once);

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Never);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = $"{ClassName} DeleteAsync should delete produto successfully")]
    public async Task DeleteAsync_ShouldDeleteProdutoSuccessfully()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        
        _produtoRepositoryMock
            .Setup(repository => repository.DeleteAsync(It.IsAny<Guid>()))
            .Callback((Guid produtoIdCb) => produtoIdCb.Should().Be(produtoId));
        
        // Act
        await _produtoService.DeleteAsync(produtoId);
        
        // Assert
        _produtoRepositoryMock.Verify(
            repository => repository.DeleteAsync(It.IsAny<Guid>()), 
            Times.Once);
    }

    #endregion
}