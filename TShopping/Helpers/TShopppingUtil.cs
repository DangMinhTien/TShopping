using Microsoft.AspNetCore.Hosting;
using System.Text;
using TShopping.ViewModels;

namespace TShopping.Helpers
{
    public class TShopppingUtil
    {
        public static string GenerateRandomKey(int length = 5)
        {
            var pattern = @"aadhfgfaddagaahkkZBXBXNMXXMI?!@#$&";
            var sb = new StringBuilder();
            var rd = new Random();
            for(int i = 0; i < length; i++)
            {
                sb.Append(pattern[rd.Next(0, pattern.Length)]);
            }
            return sb.ToString();
        }
        public static string UploadPhoto(IFormFile fileUpload, string folder)
        {
            var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                                                + Path.GetExtension(fileUpload.FileName);
            var file = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot", "Hinh", folder, fileName);
            using (var filestream = new FileStream(file, FileMode.Create))
            {
                fileUpload.CopyTo(filestream);
            }
            return fileName;
        }
    }
}
