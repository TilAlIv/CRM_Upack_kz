using System.IO;
using Microsoft.AspNetCore.Http;

namespace CRM_Upack_kz.Services
{
    public class FileUploadService
    {
        public async void Upload(string path, string fileName, IFormFile file)
        {
            using var stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
            await file.CopyToAsync(stream);
        }
    }
}