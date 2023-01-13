using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Services;
using DevIO.Business.Services.Base;

namespace DevIO.Business.Services
{
    public class FileService : BaseService, IFileService
    {
        public FileService(INotifier notifier) : base(notifier) { }

        public async Task<bool> UploadAsync(string file, string fileName)
        {
            var fileDataByteArray = Convert.FromBase64String(file);

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

            await File.WriteAllBytesAsync(path, fileDataByteArray);

            return true;
        }

        public void Delete(string fileName)
        {
            var path = GetFilePath(fileName);

            File.Delete(path);
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
        }
    }
}
