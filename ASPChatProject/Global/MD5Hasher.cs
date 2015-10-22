using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ASPChatProject.Global
{
    public class MD5Hasher
    {
        static string Salt = "Labs";
        static MD5 Hasher;
        static MD5Hasher()
        {
            Hasher = MD5.Create();
        }
        public static string GetHash(string pwd)
        {
            var t = Hasher.ComputeHash(Encoding.UTF8.GetBytes(Salt + pwd));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < t.Length; ++i)
            {
                sb.Append(t[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}