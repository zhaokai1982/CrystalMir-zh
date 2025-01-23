using System.Collections;
using System.Security.Cryptography;
using System;
using System.IO;
using System.Text;

public class RSA
{

    //string pubkey = "<RSAKeyValue><Modulus>xe3teTUwLgmbiwFJwWEQnshhKxgcasglGsfNVFTk0hdqKc9i7wb+gG7HOdPZLh65QyBcFfzdlrawwVkiPEL5kNTX1q3JW5J49mTVZqWd3w49reaLd8StHRYJdyGAL4ZovBhSTThETi+zYvgQ5SvCGkM6/xXOz+lkMaEgeFcjQQs=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    //string prikey = "<RSAKeyValue><Modulus>xe3teTUwLgmbiwFJwWEQnshhKxgcasglGsfNVFTk0hdqKc9i7wb+gG7HOdPZLh65QyBcFfzdlrawwVkiPEL5kNTX1q3JW5J49mTVZqWd3w49reaLd8StHRYJdyGAL4ZovBhSTThETi+zYvgQ5SvCGkM6/xXOz+lkMaEgeFcjQQs=</Modulus><Exponent>AQAB</Exponent><P>5flMAd7IrUTx92yomBdJBPDzp1Kclpaw4uXB1Ht+YXqwLW/9icI6mcv7d2O0kuVLSWj8DPZJol9V8AtvHkC3oQ==</P><Q>3FRA9UWcFrVPvGR5bewcL7YqkCMZlybV/t6nCH+gyMfbEvgk+p04F+j8WiHDykWj+BahjScjwyF5SGADbrfJKw==</Q><DP>b4WOU1XbERNfF3JM67xW/5ttPNX185zN2Ko8bbMZXWImr1IgrD5RNqXRo1rphVbGRKoxmIOSv7flr8uLrisKIQ==</DP><DQ>otSZlSq2qomgvgg7PaOLSS+F0TQ/i1emO0/tffhkqT4ah7BgE97xP6puJWZivjAteAGxrxHH+kPY0EY1AzRMNQ==</DQ><InverseQ>Sxyz0fEf5m7GrzAngLDRP/i+QDikJFfM6qPyr3Ub6Y5RRsFbeOWY1tX3jmV31zv4cgJ6donH7W2dSBPi67sSsw==</InverseQ><D>nVqofsIgSZltxTcC8fA/DFz1kxMaFHKFvSK3RKIxQC1JQ3ASkUEYN/baAElB0f6u/oTNcNWVPOqE31IDe7ErQelVc4D26RgFd5V7dSsF3nVz00s4mq1qUBnCBLPIrdb0rcQZ8FUQTsd96qW8Foave4tm8vspbM65iVUBBVdSYYE=</D></RSAKeyValue>";


    public static string RsaEncrypt(string rawInput, string publicKey)
    {
        if (string.IsNullOrEmpty(rawInput))
        {
            return string.Empty;
        }
        using (var rsaProvider = new RSACryptoServiceProvider())
        {
            var inputBytes = Encoding.UTF8.GetBytes(rawInput);//有含义的字符串转化为字节流
            rsaProvider.FromXmlString(publicKey);//载入公钥
            int bufferSize = (rsaProvider.KeySize / 8) - 11;//单块最大长度
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(inputBytes),
                 outputStream = new MemoryStream())
            {
                while (true)
                { //分段加密
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var encryptedBytes = rsaProvider.Encrypt(temp, false);
                    outputStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                }
                return Convert.ToBase64String(outputStream.ToArray());//转化为字节流方便传输
            }
        }
    }

    public static string RsaDecrypt(string encryptedInput, string privateKey)
    {
        if (string.IsNullOrEmpty(encryptedInput))
        {
            return string.Empty;
        }

        using (var rsaProvider = new RSACryptoServiceProvider())
        {
            var inputBytes = Convert.FromBase64String(encryptedInput);
            rsaProvider.FromXmlString(privateKey);
            int bufferSize = rsaProvider.KeySize / 8;
            var buffer = new byte[bufferSize];
            using (MemoryStream inputStream = new MemoryStream(inputBytes),
                 outputStream = new MemoryStream())
            {
                while (true)
                {
                    int readSize = inputStream.Read(buffer, 0, bufferSize);
                    if (readSize <= 0)
                    {
                        break;
                    }

                    var temp = new byte[readSize];
                    Array.Copy(buffer, 0, temp, 0, readSize);
                    var rawBytes = rsaProvider.Decrypt(temp, false);
                    outputStream.Write(rawBytes, 0, rawBytes.Length);
                }
                return Encoding.UTF8.GetString(outputStream.ToArray());
            }
        }
    }
}
