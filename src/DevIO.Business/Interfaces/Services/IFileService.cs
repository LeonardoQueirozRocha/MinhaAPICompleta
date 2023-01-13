using System.IO.Enumeration;

namespace DevIO.Business.Interfaces.Services
{
    public interface IFileService
    {
        Task<bool> UploadAsync(string file, string fileName);
        void Delete(string fileName);
    }
}
