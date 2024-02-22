using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TShopping.Helpers
{
    public static class Generation
    {
        public static string GenerateSlug(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            // Chuyển đổi chuỗi thành chữ thường và loại bỏ dấu
            string normalizedString = RemoveDiacritics(input.ToLower());

            // Loại bỏ các ký tự không mong muốn và thay thế khoảng trắng bằng dấu gạch nối
            string slug = Regex.Replace(normalizedString, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", " ").Trim();
            slug = slug.Replace(" ", "-");

            return slug;
        }

        // Hàm loại bỏ dấu từ các ký tự
        public static string RemoveDiacritics(string input)
        {
            string normalizedString = input.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString();
        }
    }
}
