using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ShoeStore.Application.Interfaces.Services
{
    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile file);
        Task<bool> DeleteImageAsync(string publicId);
    }
}
