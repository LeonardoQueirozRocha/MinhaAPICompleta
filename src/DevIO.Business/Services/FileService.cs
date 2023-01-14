using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Services.Base;
using Microsoft.AspNetCore.Http;

namespace DevIO.Business.Services
{
    public class FileService : BaseService, IFileService
    {
        public FileService(INotifier notifier) : base(notifier) { }

        public async Task<bool> UploadAsync(string file, string fileName)
        {
            if (string.IsNullOrEmpty(file))
            {
                Notify("Forneça uma imagem para este produto!");
                return false;
            }

            var path = GetFilePath(fileName);

            if (File.Exists(path))
            {
                Notify("Já existe uma arquivo com este nome!");
                return false;
            }

            var bytes = Convert.FromBase64String(file);
            await File.WriteAllBytesAsync(path, bytes);

            return true;
        }

        public async Task<bool> UploadStreamingAsync(IFormFile file, string filePrefix)
        {
            if (file == null || file.Length == 0)
            {
                Notify("Forneça uma imagem para este produto!");
                return false;
            }

            var path = GetFilePath(filePrefix + file.FileName);

            if (File.Exists(path))
            {
                Notify("Já existe uma arquivo com este nome!");
                return false;
            }

            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return true;
        }

        public void Delete(string fileName)
        {
            File.Delete(GetFilePath(fileName));
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", fileName);
        }
    }
}
