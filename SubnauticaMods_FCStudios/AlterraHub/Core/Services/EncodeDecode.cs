using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
namespace FCS_AlterraHub.Core.Services;

public class EncodeDecode
{
    public static class Global
    {
        // set permutations
        public const String strPermutation = "ouiveyxaqtd";
        public const Int32 bytePermutation1 = 0x19;
        public const Int32 bytePermutation2 = 0x59;
        public const Int32 bytePermutation3 = 0x17;
        public const Int32 bytePermutation4 = 0x41;
    }

    public static string CreateMD5(string input)
    {
        // Use input string to calculate MD5 hash
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Convert the byte array to hexadecimal string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }


    // encoding
    public static string Encrypt(string strData)
    {

        return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(strData)));
        // reference https://msdn.microsoft.com/en-us/library/ds4kkd55(v=vs.110).aspx

    }


    // decoding
    public static string Decrypt(string strData)
    {
        return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(strData)));
        // reference https://msdn.microsoft.com/en-us/library/system.convert.frombase64string(v=vs.110).aspx

    }

    // encrypt
    public static byte[] Encrypt(byte[] strData)
    {
        PasswordDeriveBytes passbytes =
        new PasswordDeriveBytes(Global.strPermutation,
        new byte[] { Global.bytePermutation1,
                     Global.bytePermutation2,
                     Global.bytePermutation3,
                     Global.bytePermutation4
        });

        MemoryStream memstream = new MemoryStream();
        Aes aes = new AesManaged();
        aes.Key = passbytes.GetBytes(aes.KeySize / 8);
        aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

        CryptoStream cryptostream = new CryptoStream(memstream,
        aes.CreateEncryptor(), CryptoStreamMode.Write);
        cryptostream.Write(strData, 0, strData.Length);
        cryptostream.Close();
        return memstream.ToArray();
    }

    // decrypt
    public static byte[] Decrypt(byte[] strData)
    {
        PasswordDeriveBytes passbytes =
        new PasswordDeriveBytes(Global.strPermutation,
        new byte[] { Global.bytePermutation1,
                     Global.bytePermutation2,
                     Global.bytePermutation3,
                     Global.bytePermutation4
        });

        MemoryStream memstream = new MemoryStream();
        Aes aes = new AesManaged();
        aes.Key = passbytes.GetBytes(aes.KeySize / 8);
        aes.IV = passbytes.GetBytes(aes.BlockSize / 8);

        CryptoStream cryptostream = new CryptoStream(memstream,
        aes.CreateDecryptor(), CryptoStreamMode.Write);
        cryptostream.Write(strData, 0, strData.Length);
        cryptostream.Close();
        return memstream.ToArray();
    }
}
