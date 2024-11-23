using System.Linq.Expressions;
using Bogus;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Models;
using DevIO.Business.Models.Enums;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Configurations;
using DevIO.Utils.Tests.Builders.Business.Models;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace DevIO.Business.Tests.Services;

public class FornecedorServiceTests
{
    private const string ClassName = nameof(FornecedorService);

    private readonly Mock<IFornecedorRepository> _fornecedorRespository;
    private readonly Mock<IEnderecoRepository> _enderecoRepository;
    private readonly Mock<INotifier> _notifier;
    private readonly ValidationMessages _validationMessages;
    private readonly FornecedorService _fornecedorService;
    private readonly Faker _faker;

    public FornecedorServiceTests()
    {
        _faker = new Faker("pt_BR");
        var mocker = new AutoMocker();
        mocker.Use(ValidationMessagesBuilder.Instance.Build());
        _fornecedorService = mocker.CreateInstance<FornecedorService>();
        _fornecedorRespository = mocker.GetMock<IFornecedorRepository>();
        _enderecoRepository = mocker.GetMock<IEnderecoRepository>();
        _notifier = mocker.GetMock<INotifier>();
        _validationMessages = mocker.Get<ValidationMessages>();
    }

    #region GetAllAsync

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return all suppliers")]
    public async Task GetAllAsync_ShouldReturnAllSuppliers()
    {
        // Arrange
        var fornecedores = FornecedorBuilder.Instance
            .BuildCollection()
            .ToList();

        fornecedores.ForEach(fornecedor =>
        {
            fornecedor.Endereco = null;
            fornecedor.Produtos = Enumerable.Empty<Produto>();
        });

        _fornecedorRespository
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(fornecedores);

        // Act
        var result = await _fornecedorService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Any().Should().BeTrue();
        result.Count().Should().Be(fornecedores.Count);
        result.ToList().ForEach(supplier => supplier.Endereco.Should().BeNull());
        result.ToList().ForEach(supplier => supplier.Produtos.Should().BeNullOrEmpty());

        _fornecedorRespository.Verify(
            repository => repository.GetAllAsync(),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return empty")]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        // Arrange
        var fornecedores = FornecedorBuilder.Instance
            .BuildCollection(0)
            .ToList();

        _fornecedorRespository
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(fornecedores);

        // Act
        var result = await _fornecedorService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Any().Should().BeFalse();
        result.Count().Should().Be(fornecedores.Count);

        _fornecedorRespository.Verify(
            repository => repository.GetAllAsync(),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetAllAsync Should return null")]
    public async Task GetAllAsync_ShouldReturnNull()
    {
        // Arrange && Act
        var result = await _fornecedorService.GetAllAsync();

        // Assert
        result.Should().BeNull();

        _fornecedorRespository.Verify(
            service => service.GetAllAsync(),
            Times.Once());
    }

    #endregion

    #region GetFornecedorEnderecoAsync

    [Fact(DisplayName = $"{ClassName} GetFornecedorEnderecoAsync Should return supplier with address")]
    public async Task GetFornecedorEnderecoAsync_ShouldReturnSupplierWithAddress()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco = EnderecoBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.GetFornecedorEnderecoAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fornecedor);

        // Act
        var result = await _fornecedorService.GetFornecedorEnderecoAsync(fornecedor.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(fornecedor);
        result.Endereco.Should().NotBeNull();
        result.Endereco.Should().Be(fornecedor.Endereco);

        _fornecedorRespository.Verify(
            service => service.GetFornecedorEnderecoAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    [Fact(DisplayName = $"{ClassName} GetFornecedorEnderecoAsync Should return null")]
    public async Task GetFornecedorEnderecoAsync_ShouldReturnNull()
    {
        // Arrange
        var fornecedorId = Guid.NewGuid();

        // Act
        var result = await _fornecedorService.GetFornecedorEnderecoAsync(fornecedorId);

        // Assert
        result.Should().BeNull();

        _fornecedorRespository.Verify(
            service => service.GetFornecedorEnderecoAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    #endregion

    #region GetFornecedorProdutosEnderecoAsync

    [Fact(DisplayName =
        $"{ClassName} GetFornecedorProdutosEnderecoAsync Should return supplier with products and address")]
    public async Task GetFornecedorProdutosEnderecoAsync_ShouldReturnSupplierWithProductsAndAddress()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco = EnderecoBuilder.Instance.Build();
        fornecedor.Produtos = ProdutoBuilder.Instance.BuildCollection(3);

        _fornecedorRespository
            .Setup(repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()))
            .ReturnsAsync(fornecedor);

        // Act
        var result = await _fornecedorService.GetFornecedorProdutosEnderecoAsync(fornecedor.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(fornecedor);
        result.Endereco.Should().NotBeNull();
        result.Endereco.Should().Be(fornecedor.Endereco);
        result.Produtos.Should().NotBeNull();
        result.Produtos.Any().Should().BeTrue();
        result.Produtos.Should().Equal(fornecedor.Produtos);

        _fornecedorRespository.Verify(
            repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} GetFornecedorProdutosEnderecoAsync Should return null")]
    public async Task GetFornecedorProdutosEnderecoAsync_ShouldReturnNull()
    {
        // Arrange
        var fornecedorId = Guid.NewGuid();

        // Act
        var result = await _fornecedorService.GetFornecedorProdutosEnderecoAsync(fornecedorId);

        // Assert
        result.Should().BeNull();

        _fornecedorRespository.Verify(
            service => service.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()),
            Times.Once());
    }

    #endregion

    #region AddAsync

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when fornecedor Nome is Empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenFornecedorIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Nome = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when fornecedor Name Lenght is invalid")]
    [InlineData(1)]
    [InlineData(101)]
    public async Task AddAsync_ShouldReturnFalse_WhenFornecedorNameLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Nome = _faker.Random.String(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Pessoa Fisica document lenght is invalid")]
    [InlineData(10)]
    [InlineData(12)]
    public async Task AddAsync_ShouldReturnFalse_WhenPessoaFisicaDocumentLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaFisica;
        fornecedor.Documento = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Pessoa Fisica document is invalid")]
    public async Task AddAsync_ShouldReturnFalse_WhenPessoaFisicaDocumentIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaFisica;
        fornecedor.Documento = _faker.Random.ReplaceNumbers("###########");

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Pessoa Juridica document lenght is invalid")]
    [InlineData(13)]
    [InlineData(15)]
    public async Task AddAsync_ShouldReturnFalse_WhenPessoaJuridicaDocumentLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaJuridica;
        fornecedor.Documento = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Pessoa Juridica document is invalid")]
    public async Task AddAsync_ShouldReturnFalse_WhenPessoaJuridicaDocumentIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaJuridica;
        fornecedor.Documento = _faker.Random.ReplaceNumbers("##############");

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Logradouro is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenLogradouroIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Logradouro = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Logradouro lenght is invalid")]
    [InlineData(1)]
    [InlineData(201)]
    public async Task AddAsync_ShouldReturnFalse_WhenLogradouroLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Logradouro = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Bairro is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenBairroIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Bairro = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Bairro lenght is invalid")]
    [InlineData(1)]
    [InlineData(201)]
    public async Task AddAsync_ShouldReturnFalse_WhenBairroLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Bairro = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Cep is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenCepIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Cep = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Cep lenght is greater than eight")]
    public async Task AddAsync_ShouldReturnFalse_WhenCepLenghtIsGreaterThanEight()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Cep = _faker.Random.ReplaceNumbers("#########");

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Cidade is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenCidadeIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Cidade = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Cidade lenght is invalid")]
    [InlineData(1)]
    [InlineData(201)]
    public async Task AddAsync_ShouldReturnFalse_WhenCidadeLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Cidade = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Estado is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenEstadoIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Estado = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Estado lenght is invalid")]
    [InlineData(1)]
    [InlineData(51)]
    public async Task AddAsync_ShouldReturnFalse_WhenEstadoLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Estado = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Numero is empty")]
    public async Task AddAsync_ShouldReturnFalse_WhenNumeroIsEmpty()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Numero = string.Empty;

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} AddAsync should return false when Numero lenght is invalid")]
    [InlineData(0)]
    [InlineData(51)]
    public async Task AddAsync_ShouldReturnFalse_WhenNumeroLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Endereco.Numero = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Fornecedor already exists")]
    public async Task AddAsync_ShouldReturnFalse_WhenFornecedorAlreadyExists()
    {
        // Arrange
        var fornecedores = FornecedorBuilder.Instance.BuildCollection(1);

        _fornecedorRespository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<Expression<Func<Fornecedor, bool>>>()))
            .ReturnsAsync(fornecedores);

        // Act
        var result = await _fornecedorService.AddAsync(fornecedores.First());

        // Assert
        result.Should().BeFalse();

        _fornecedorRespository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<Expression<Func<Fornecedor, bool>>>()),
            Times.Once);

        _fornecedorRespository.Verify(
            repository => repository.AddAsync(It.IsAny<Fornecedor>()),
            Times.Never());
    }

    [Fact(DisplayName = $"{ClassName} AddAsync should return false when Fornecedor don't exists")]
    public async Task AddAsync_ShouldReturnTrue_WhenFornecedorDoNotExists()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.SearchAsync(
                It.IsAny<Expression<Func<Fornecedor, bool>>>()))
            .ReturnsAsync(Enumerable.Empty<Fornecedor>());

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeTrue();

        _fornecedorRespository.Verify(
            repository => repository.SearchAsync(
                It.IsAny<Expression<Func<Fornecedor, bool>>>()),
            Times.Once);

        _fornecedorRespository.Verify(
            repository => repository.AddAsync(It.IsAny<Fornecedor>()),
            Times.Once);
    }

    #endregion

    #region UpdateAsync

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return false when fornecedor Nome is Empty")]
    public async Task UpdateAsync_ShouldReturnFalse_WhenFornecedorIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Nome = string.Empty;

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} UpdateAsync should return false when fornecedor Name Lenght is invalid")]
    [InlineData(1)]
    [InlineData(101)]
    public async Task UpdateAsync_ShouldReturnFalse_WhenFornecedorNameLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Nome = _faker.Random.String(lenght);

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = $"{ClassName} UpdateAsync should return false when Pessoa Fisica document lenght is invalid")]
    [InlineData(10)]
    [InlineData(12)]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPessoaFisicaDocumentLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaFisica;
        fornecedor.Documento = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return false when Pessoa Fisica document is invalid")]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPessoaFisicaDocumentIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaFisica;
        fornecedor.Documento = _faker.Random.ReplaceNumbers("###########");

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName =
        $"{ClassName} UpdateAsync should return false when Pessoa Juridica document lenght is invalid")]
    [InlineData(13)]
    [InlineData(15)]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPessoaJuridicaDocumentLenghtIsInvalid(int lenght)
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaJuridica;
        fornecedor.Documento = _faker.Random.String2(lenght);

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return false when Pessoa Juridica document is invalid")]
    public async Task UpdateAsync_ShouldReturnFalse_WhenPessoaJuridicaDocumentIsInvalid()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.TipoFornecedor = TipoFornecedor.PessoaJuridica;
        fornecedor.Documento = _faker.Random.ReplaceNumbers("##############");

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return false when fornecedor is not found")]
    public async Task UpdateAsync_ShouldReturnFalse_WhenFornecedorIsNotFound()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .Callback((Guid idCb) => idCb.Should().Be(fornecedor.Id))
            .ReturnsAsync((Fornecedor)null!);

        _notifier
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().Be(_validationMessages.SupplierNotFound));

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();

        _fornecedorRespository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return false when fornecedor already exists")]
    public async Task UpdateAsync_ShouldReturnFalse_WhenFornecedorAlreadyExist()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .Callback((Guid idCb) => idCb.Should().Be(fornecedor.Id))
            .ReturnsAsync(fornecedor);

        _notifier
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().Be(_validationMessages.SupplierAlreadyExist));

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedor);

        // Assert
        result.Should().BeFalse();

        _fornecedorRespository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UpdateAsync should return true when fornecedor is able to update")]
    public async Task UpdateAsync_ShouldReturnTrue_WhenFornecedorIsAbleToUpdate()
    {
        // Arrange
        var fornecedorToUpdate = FornecedorBuilder.Instance.Build();
        var fornecedorFromDatabase = FornecedorBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .Callback((Guid idCb) => idCb.Should().Be(fornecedorToUpdate.Id))
            .ReturnsAsync(fornecedorFromDatabase);

        _fornecedorRespository
            .Setup(repository => repository.UpdateAsync(It.IsAny<Fornecedor>()))
            .Callback((Fornecedor fornecedorCb) => fornecedorCb.Should().BeEquivalentTo(fornecedorToUpdate));

        // Act
        var result = await _fornecedorService.UpdateAsync(fornecedorToUpdate);

        // Assert
        result.Should().BeTrue();

        _fornecedorRespository.Verify(
            repository => repository.GetByIdAsync(It.IsAny<Guid>()),
            Times.Once);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Never);

        _fornecedorRespository.Verify(
            repository => repository.UpdateAsync(It.IsAny<Fornecedor>()),
            Times.Once);
    }

    #endregion

    #region DeleteAsync

    [Fact(DisplayName = $"{ClassName} DeleteAsync should return false when fornecedor has registered products")]
    public async Task DeleteAsync_ShouldReturnFalse_WhenFornecedorHasRegisteredProducts()
    {
        // Arrange
        var fornecedorId = Guid.NewGuid();
        var fornecedor = FornecedorBuilder.Instance.Build();

        _fornecedorRespository
            .Setup(repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedorId))
            .ReturnsAsync(fornecedor);

        _notifier
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().BeEquivalentTo(_validationMessages.SupplierHasRegisteredProducts));

        // Act
        var result = await _fornecedorService.DeleteAsync(fornecedorId);

        // Assert
        result.Should().BeFalse();

        _fornecedorRespository.Verify(
            repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()),
            Times.Once);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} DeleteAsync should return true when fornecedor has endereco to delete")]
    public async Task DeleteAsync_ShouldReturnTrue_WhenFornecedorHasEnderecoToDelete()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Produtos = Enumerable.Empty<Produto>();

        _fornecedorRespository
            .Setup(repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id))
            .ReturnsAsync(fornecedor);

        _enderecoRepository
            .Setup(repository => repository.GetEnderecoByFornecedorAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id))
            .ReturnsAsync(fornecedor.Endereco);

        _enderecoRepository
            .Setup(repository => repository.DeleteAsync(It.IsAny<Guid>()))
            .Callback((Guid enderecoIdCb) => enderecoIdCb.Should().Be(fornecedor.Endereco.Id));

        _fornecedorRespository
            .Setup(repository => repository.DeleteAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id));

        // Act
        var result = await _fornecedorService.DeleteAsync(fornecedor.Id);

        // Assert
        result.Should().BeTrue();

        _fornecedorRespository.Verify(
            repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()),
            Times.Once);

        _fornecedorRespository.Verify(
            repository => repository.DeleteAsync(It.IsAny<Guid>()),
            Times.Once);

        _enderecoRepository.Verify(
            repository => repository.GetEnderecoByFornecedorAsync(It.IsAny<Guid>()),
            Times.Once);

        _enderecoRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<Guid>()),
            Times.Once);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Never);
    }

    [Fact(DisplayName = $"{ClassName} DeleteAsync should return true when fornecedor has not endereco to delete")]
    public async Task DeleteAsync_ShouldReturnTrue_WhenFornecedorHasNotEnderecoToDelete()
    {
        // Arrange
        var fornecedor = FornecedorBuilder.Instance.Build();
        fornecedor.Produtos = Enumerable.Empty<Produto>();
        fornecedor.Endereco = null;

        _fornecedorRespository
            .Setup(repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id))
            .ReturnsAsync(fornecedor);

        _enderecoRepository
            .Setup(repository => repository.GetEnderecoByFornecedorAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id))
            .ReturnsAsync(fornecedor.Endereco);

        _fornecedorRespository
            .Setup(repository => repository.DeleteAsync(It.IsAny<Guid>()))
            .Callback((Guid fornecedorIdCb) => fornecedorIdCb.Should().Be(fornecedor.Id));

        // Act
        var result = await _fornecedorService.DeleteAsync(fornecedor.Id);

        // Assert
        result.Should().BeTrue();

        _fornecedorRespository.Verify(
            repository => repository.GetFornecedorProdutosEnderecoAsync(It.IsAny<Guid>()),
            Times.Once);

        _fornecedorRespository.Verify(
            repository => repository.DeleteAsync(It.IsAny<Guid>()),
            Times.Once);

        _enderecoRepository.Verify(
            repository => repository.GetEnderecoByFornecedorAsync(It.IsAny<Guid>()),
            Times.Once);

        _enderecoRepository.Verify(
            repository => repository.DeleteAsync(It.IsAny<Guid>()),
            Times.Never);

        _notifier.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Never);
    }

    #endregion
}