using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RaktarKeszlet.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileUploadService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string subFolder = "")
        {
            if (file == null || file.Length == 0)
                return null;

            // Alapértelmezett mappa: wwwroot/images. Ha megadsz almappát, azt is hozzáfűzi.
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", subFolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            // AZ ELŐZŐ BESZÉLGETÉSÜNK ALAPJÁN JAVÍTVA: 
            // Ne tegyük bele a "/images/" előtagot, hogy a HTML kártyákon a 
            // <img src="~/images/@item.PhotoUrl"> kód továbbra is tökéletesen működjön!
            return string.IsNullOrEmpty(subFolder) ? uniqueFileName : $"{subFolder}/{uniqueFileName}";
        }
    }
}