using System.IO.Abstractions;
using DevIO.Business.Configurations;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Services.Base;
using Microsoft.AspNetCore.Http;

namespace DevIO.Business.Services;

public class FileService : BaseService, IFileService
{
    private readonly IFileSystem _fileSystem;
    private readonly ValidationMessages _validationMessages;

    public FileService(
        IFileSystem fileSystem,
        INotifier notifier,
        ValidationMessages validationMessages) : base(notifier)
    {
        _fileSystem = fileSystem;
        _validationMessages = validationMessages;
    }

    public async Task<bool> UploadAsync(string file, string fileName)
    {
        if (string.IsNullOrEmpty(file))
        {
            Notify(_validationMessages.EmptyFileMessage);
            return false;
        }

        var path = GetFilePath(fileName);

        if (_fileSystem.File.Exists(path))
        {
            Notify(_validationMessages.FileAlreadyExistMessage);
            return false;
        }

        var bytes = Convert.FromBase64String(file);
        await _fileSystem.File.WriteAllBytesAsync(path!, bytes);

        return true;
    }

    public async Task<bool> UploadStreamingAsync(IFormFile file, string filePrefix)
    {
        if (file == null || file.Length == 0)
        {
            Notify(_validationMessages.EmptyFileMessage);
            return false;
        }

        var path = GetFilePath(filePrefix + file.FileName);

        if (_fileSystem.File.Exists(path))
        {
            Notify(_validationMessages.FileAlreadyExistMessage);
            return false;
        }

        await using var stream = _fileSystem.FileStream.New(path!, FileMode.Create);
        await file.CopyToAsync(stream);

        return true;
    }

    public void Delete(string fileName)
        => _fileSystem.File.Delete(GetFilePath(fileName));

    private string GetFilePath(string fileName)
    {
        const string projectPah = "wwwroot/app/demo-webapi/src/assets";
        var currentPath = _fileSystem.Directory.GetCurrentDirectory();
        return _fileSystem.Path.Combine(currentPath, projectPah, fileName);
    }
}