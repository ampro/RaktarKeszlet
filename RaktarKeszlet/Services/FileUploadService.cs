using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using SkiaSharp;

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

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", subFolder);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Egységes .jpg kiterjesztés
            string uniqueFileName = Guid.NewGuid().ToString() + ".jpg";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // --- 100% INGYENES KÉP ÁTMÉRETEZÉS SKIASHARP SEGÍTSÉGÉVEL ---

            using (var stream = file.OpenReadStream())
            using (var originalBitmap = SKBitmap.Decode(stream))
            {
                if (originalBitmap != null)
                {
                    int maxWidth = 800;
                    int maxHeight = 800;
                    int newWidth = originalBitmap.Width;
                    int newHeight = originalBitmap.Height;

                    // Csak akkor kicsinyítünk, ha a kép nagyobb a megadott 800x800-as maximumnál
                    if (originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight)
                    {
                        float ratioX = (float)maxWidth / originalBitmap.Width;
                        float ratioY = (float)maxHeight / originalBitmap.Height;
                        float ratio = Math.Min(ratioX, ratioY);

                        newWidth = (int)(originalBitmap.Width * ratio);
                        newHeight = (int)(originalBitmap.Height * ratio);
                    }

                    // Új méret beállítása és minőségi szűrő alkalmazása
                    var imageInfo = new SKImageInfo(newWidth, newHeight);

                    // JAVÍTVA: Az új SKSamplingOptions használata SKFilterQuality helyett
                    using (var resizedBitmap = originalBitmap.Resize(imageInfo, new SKSamplingOptions(SKCubicResampler.Mitchell)))
                    using (var image = SKImage.FromBitmap(resizedBitmap))
                    // Mentés 85-ös (kiváló webre optimalizált) JPEG minőségben
                    using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 85))
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        data.SaveTo(fileStream);
                    }
                }
            }

            return string.IsNullOrEmpty(subFolder) ? uniqueFileName : $"{subFolder}/{uniqueFileName}";
        }
    }
}