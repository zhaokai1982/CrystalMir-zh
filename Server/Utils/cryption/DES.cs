using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

/// <summary>
/// DESEncrypt加密解密算法。
/// </summary>
public class DES
{
    private static string key = "12345678";

    /// <summary>
    /// 对称加密解密的密钥
    /// </summary>
    public static string Key
    {
        get
        {
            return key;
        }
        set
        {
            key = value;
        }
    }

    /// <summary>
    /// DES加密
    /// </summary>
    /// <param name="encryptString"></param>
    /// <returns></returns>
    public static string DesEncrypt(string encryptString)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyIV = keyBytes;
        byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
        DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
        provider.Padding = PaddingMode.PKCS7;
        provider.Mode = CipherMode.CBC;

        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, provider.CreateEncryptor(keyBytes, keyIV), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();
     //   string str = System.Text.Encoding.Default.GetString(mStream.ToArray());
        return Convert.ToBase64String(mStream.ToArray());
    }

    /// <summary>
    /// DES解密
    /// </summary>
    /// <param name="decryptString"></param>
    /// <returns></returns>
    public static string DesDecrypt(string decryptString)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyIV = keyBytes;
        byte[] inputByteArray = Convert.FromBase64String(decryptString);
      //  string str = System.Text.Encoding.Default.GetString(inputByteArray);
        DESCryptoServiceProvider provider = new DESCryptoServiceProvider();
        provider.Padding = PaddingMode.PKCS7;
        provider.Mode = CipherMode.CBC;
        MemoryStream mStream = new MemoryStream();
        CryptoStream cStream = new CryptoStream(mStream, provider.CreateDecryptor(keyBytes, keyIV), CryptoStreamMode.Write);
        cStream.Write(inputByteArray, 0, inputByteArray.Length);
        cStream.FlushFinalBlock();
        return Encoding.UTF8.GetString(mStream.ToArray());
    }
}