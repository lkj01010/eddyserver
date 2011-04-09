using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Eddy
{
    public static class MD5Hash
    {
        public static string Get(string input)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder stringBuilder = new StringBuilder();

            foreach (var c in data)
                stringBuilder.Append(c.ToString("x2"));

            return stringBuilder.ToString();
        }

        public static bool Verify(string input, string hash)
        {
            string hashOfInput = Get(input);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return (0 == comparer.Compare(hashOfInput, hash));
        }
    }
}
