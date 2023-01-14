using Microsoft.AspNetCore.Http;

namespace DevIO.Business.Interfaces.Services
{
    public interface IFileService
    {
        Task<bool> UploadAsync(string file, string fileName);
        Task<bool> UploadStreamingAsync(IFormFile file, string filePrefix);
        void Delete(string fileName);
    }
}
