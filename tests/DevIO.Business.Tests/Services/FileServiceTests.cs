using System.IO.Abstractions;
using Bogus;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using DevIO.Utils.Tests.Builders.Business.Configurations;
using FluentAssertions;
using Moq;

namespace DevIO.Business.Tests.Services;

public class FileServiceTests
{
    private const string ClassName = nameof(FileService);

    private readonly Mock<INotifier> _notifierMock;
    private readonly Mock<IFile> _fileMock;
    private readonly Mock<IFileSystem> _fileSystemMock;
    private readonly ValidationMessages _validationMessages;
    private readonly FileService _fileService;
    private readonly Faker _faker;

    public FileServiceTests()
    {
        _notifierMock = new Mock<INotifier>();
        _fileMock = new Mock<IFile>();
        _fileSystemMock = new Mock<IFileSystem>();

        _fileSystemMock
            .SetupGet(system => system.Directory)
            .Returns(GetDirectoryMock().Object);

        _validationMessages = ValidationMessagesBuilder.Instance.Build();
        _fileService = new FileService(
            _fileSystemMock.Object,
            _notifierMock.Object,
            _validationMessages);

        _faker = new Faker("pt_BR");
    }

    #region UploadAsync

    [Fact(DisplayName = $"{ClassName} UploadAsync should return false when file is null")]
    public async Task UploadAsync_ShouldReturnFalse_WhenFileIsNull()
    {
        // Arrange
        string? file = null;
        string? fileName = null;

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().Be(_validationMessages.EmptyFileMessage));

        // Act
        var result = await _fileService.UploadAsync(fileName, file);

        // Assert
        result.Should().BeFalse();

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UploadAsync should return false when file is empty")]
    public async Task UploadAsync_ShouldReturnFalse_WhenFileIsEmpty()
    {
        // Arrange
        var file = string.Empty;
        string? fileName = null;

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().Be(_validationMessages.EmptyFileMessage));

        // Act
        var result = await _fileService.UploadAsync(fileName, file);

        // Assert
        result.Should().BeFalse();

        _notifierMock.Verify(
            notification => notification.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UploadAsync should return false when file already exists")]
    public async Task UploadAsync_ShouldReturnFalse_WhenFileAlreadyExists()
    {
        // Arrange
        var bytes = _faker.Random.Bytes(50);
        var fileBase64 = Convert.ToBase64String(bytes);
        var fileName = _faker.System.FileName();
        var pathMock = GetPathMock(fileName);

        _fileSystemMock
            .SetupGet(system => system.Path)
            .Returns(pathMock.Object);

        _fileMock
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Callback((string pathCb) => pathCb.Should().Contain(fileName))
            .Returns(true);

        _fileSystemMock
            .SetupGet(system => system.File)
            .Returns(_fileMock.Object);

        _notifierMock
            .Setup(notification => notification.Handle(It.IsAny<Notification>()))
            .Callback((Notification notificationCb) =>
                notificationCb.Message.Should().Be(_validationMessages.FileAlreadyExistMessage));

        // Act
        var result = await _fileService.UploadAsync(fileBase64, fileName);

        // Assert
        result.Should().BeFalse();

        _notifierMock.Verify(
            notication => notication.Handle(It.IsAny<Notification>()),
            Times.Once);
    }

    [Fact(DisplayName = $"{ClassName} UploadAsync should return true when file do not exists")]
    public async Task UploadAsync_ShouldReturnTrue_WhenFileDoNotExists()
    {
        // Arrange
        var bytes = _faker.Random.Bytes(50);
        var fileBase64 = Convert.ToBase64String(bytes);
        var fileName = _faker.System.FileName();
        var pathMock = GetPathMock(fileName);

        _fileSystemMock
            .SetupGet(system => system.Path)
            .Returns(pathMock.Object);

        _fileMock
            .Setup(x => x.Exists(It.IsAny<string>()))
            .Callback((string pathCb) => pathCb.Should().Contain(fileName))
            .Returns(false);

        _fileMock
            .Setup(fileMock => fileMock.WriteAllBytesAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<CancellationToken>()))
            .Callback((
                string pathCb,
                byte[] bytesCb,
                CancellationToken _) =>
            {
                pathCb.Should().Contain(fileName);
                bytesCb.Should().BeEquivalentTo(bytes);
            });

        _fileSystemMock
            .SetupGet(system => system.File)
            .Returns(_fileMock.Object);

        // Act
        var result = await _fileService.UploadAsync(fileBase64, fileName);

        // Assert
        result.Should().BeTrue();

        _notifierMock.Verify(
            notication => notication.Handle(It.IsAny<Notification>()),
            Times.Never);

        _fileMock.Verify(
            fileMock => fileMock.WriteAllBytesAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Private methods

    private static Mock<IDirectory> GetDirectoryMock()
    {
        var directoryMock = new Mock<IDirectory>();

        directoryMock
            .Setup(directory => directory.GetCurrentDirectory())
            .Returns(@"c:\");

        return directoryMock;
    }

    private static Mock<IPath> GetPathMock(string fileName)
    {
        var filePath = @$"c:\wwwroot\app\demo-webapi\src\assets\{fileName}";
        var pathMock = new Mock<IPath>();

        pathMock
            .Setup(path => path.Combine(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback((
                string _,
                string pathCb2,
                string pathCb3) =>
            {
                pathCb2.Should().Be("wwwroot/app/demo-webapi/src/assets");
                pathCb3.Should().Be(fileName);
            })
            .Returns(filePath);

        return pathMock;
    }

    #endregion
}