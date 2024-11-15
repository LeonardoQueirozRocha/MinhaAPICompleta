using Bogus;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repository;
using DevIO.Business.Models.Enums;
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

    public FornecedorServiceTests()
    {
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
        result.ToList().ForEach(supplier => supplier.Produtos.Should().BeNull());

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
        fornecedor.Nome = new Faker("pt_BR").Random.String(lenght);

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
        fornecedor.Documento = new Faker("pt_BR").Random.String(lenght);

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
        fornecedor.Documento = new Faker("pt_BR").Random.ReplaceNumbers("###########");

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
        fornecedor.Documento = new Faker("pt_BR").Random.String(lenght);

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
        fornecedor.Documento = new Faker("pt_BR").Random.ReplaceNumbers("##############");

        // Act
        var result = await _fornecedorService.AddAsync(fornecedor);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateAsync

    // TODO: Implement unit tests for UpdateAsync method

    #endregion

    #region DeleteAsync

    // TODO: Implement unit tests for DeleteAsync method

    #endregion
}