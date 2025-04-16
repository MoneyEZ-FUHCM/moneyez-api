using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MoneyEz.Services.Utils
{
    public static class StringUtils
    {
        public static string ConvertToUnSign(string input)
        {
            input = input.Trim();
            for (int i = 0x20; i < 0x30; i++)
            {
                input = input.Replace(((char)i).ToString(), " ");
            }
            Regex regex = new Regex(@"\p{IsCombiningDiacriticalMarks}+");
            string str = input.Normalize(NormalizationForm.FormD);
            string str2 = regex.Replace(str, string.Empty).Replace('đ', 'd').Replace('Đ', 'D');
            while (str2.IndexOf("?") >= 0)
            {
                str2 = str2.Remove(str2.IndexOf("?"), 1);
            }
            return str2;
        }

        public static string HashToken(string token)
        {
            var secretKey = Encoding.UTF8.GetBytes("f8c3d2e4b1a7e9d0c5f3b2a8d7c6e1f9a0b2c3d4e5w=");
            using var hmac = new HMACSHA256(secretKey);
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        public static string GenerateRandomUppercaseString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                                        .Select(s => s[random.Next(s.Length)])
                                        .ToArray());
        }

        public static string GenerateInviteCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        public static bool IsDescriptionContainsSalaryKeywords(string description)
        {
            if (string.IsNullOrEmpty(description))
                return false;

            // Convert to lowercase for case-insensitive matching
            string lowerDescription = description.ToLower();

            // Vietnamese keywords
            if (lowerDescription.Contains("luong") ||
                lowerDescription.Contains("lương") ||
                lowerDescription.Contains("tien luong") ||
                lowerDescription.Contains("tiền lương"))
                return true;

            // English keywords
            string[] salaryKeywords = new[] {
                "salary",
                "wage",
                "payroll",
                "compensation",
                "pay",
                "stipend",
                "earnings",
                "income",
                "remuneration"
            };

            return salaryKeywords.Any(keyword => lowerDescription.Contains(keyword));
        }

    }
}
