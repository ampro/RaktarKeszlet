

namespace RaktarKeszlet.Services
{
    public interface IFileUploadService
    {
        // A subFolder paraméterrel külön almappákba (pl. "products", "buildings") rendezhetjük majd a képeket
        Task<string> UploadFileAsync(IFormFile file, string subFolder = "");
    }
}
