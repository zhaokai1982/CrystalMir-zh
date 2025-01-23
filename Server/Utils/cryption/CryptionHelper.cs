using Server;
using System;

public class CryptionHelper
{
    protected static MessageQueue MessageQueue
    {
        get { return MessageQueue.Instance; }
    }

    //go服务器的公钥
    const string rsaPubkey = "<RSAKeyValue><Modulus>ubCzrrH6mU1C/O+87TywK/bdmVOQvGO3UBg4th1DORD+2fvQdWVCIY9Ak2uPmesdMrvyqDaYxgHl8CSMIISnlXwt1dUh/Eo89jfASiiGo3flbvNVsEIcHSTzjLs1Hfzw0dm8LWyzRRlXRVHcqb3ccctm1EP+deYyxqfmSphRyoM=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
    //c#服务器的私钥
    const string rsaPrikey = "<RSAKeyValue><Modulus>7WkouzTcJRtMaLAJisWNNqQW+cN94eyZoZRlCMjZ5ZdJi2V+9Ng5Eb4VEUU9wovliHQmNrwuOBvMK/d1qZH4b5dZA7/cms3abqU8REdTcRu2HfPORP/F8ftSf15P3mRqAYFdL0YY021TupcsZ/JY8wtLYHMKM9VGwTcyTZjkBR0=</Modulus><Exponent>AQAB</Exponent><P>/6eTTEnCQJSCb+0FzdtqEj8+jEEhmzKJahGu6POAxSMvKLcxBFXlSvaq8kNeQTUiu0T0UKGBGArwhTn/msNHdQ==</P><Q>7btGDS4gBfm4PKcw5ynZwXLukLejvbryWpEl0v8yqWaSF3NIyVCpveSDgLued7x2fxvg0MCKwV3zQyK/Pb46CQ==</Q><DP>TK0t+IuGv2fLV1Z3jy5nueFOGTLPqYSWwkFbfHLMBFRxKH0JLou0oa8pxk9+TYBqUlB0FlraJSPiz5J9nyVCoQ==</DP><DQ>Q/XIZdYsw2iK+ZPWEA9Pt9SSTH6i6Yc65xcdV/8qTXEMDHYhdaOAks9zpGZ+O759pYKjtymDwRmJhES3pP/veQ==</DQ><InverseQ>Kzo41fBb9XMpEuY7qI/4oYRogn0/3R253Qmn3zkta4Yt5B4rm3qiEwFL6kH/tKfE4ntOgPORECDIJKYebsQeLQ==</InverseQ><D>42xlNFBCwtkPrQlVWrY7vCgcSk5W/GFmGj7JOk0NTr5KGVq0719h6/kmvVCTBRZ2XuzE3pINxHn8b/GxqSrHzGWQBrXnrk0NVovQkI+ijAX8oG1sP8hYe4WPaen3TfwTQfoy1Vv4OkP0pmrRh3GJ66QRyjwC1aqUslZGBWWN+YE=</D></RSAKeyValue>";

    //用于验证客户端版本的私钥
    const string rsaPrikeyClient = "<RSAKeyValue><Modulus>rldBXsMwUuoxyGIO9iDxwe2T2aXO8Nh8ObYbHPZqz2KI+ytgzESTraGG2MdDk5RY8p0UM5BEbgnZD9XqkCudmknL4B5tlJaK74juE5LER5l/jpjQVl9QWeGTrDQlCo5rz5uDxHLkJMICj6HkYsUl7F8eaFyX02/v5UVSpMuIwqU=</Modulus><Exponent>AQAB</Exponent><P>25bXuEvmgSfRmLi9riRy2wEgVRvgkCHyGU1Y1V/nc9FtFWvYfG/gJbAf0sXK2dHjA/+Gq85mRY4tmMxFei0ORQ==</P><Q>yz+zwZjT8uU4+Vjbpg7UhJ60EcwQhe0luKyfd3zfFN2FHtd+iOKHh90F75bUa2xf4n0jJGvubTnjEr8dwyHY4Q==</Q><DP>HjBkw++bZxJEXIy5dyGHsNg8y7ajbu1bzWK23c7rkR6vsow7HRB8bnICTgXnV37liP0uVJxxrwpW9nTO8HcVSQ==</DP><DQ>ppRKLoSkSbIm9O4n77y93wf3i5KVhD5uKSKJ/DOC3qYD4dg6Y9RNwkObBte2t2h2Wbm4ILS5EBpeilHLAc/u4Q==</DQ><InverseQ>g7wHdAcMv/32gd9/WxPf+cluvhTTqnZNGO5ALoD6M9amwsZn38x/PykduWRUHlE+0uZuw4vHzDeKLbkmCU4dFw==</InverseQ><D>YWcgf5gfvINZsvGOGFcJmuZoLzZi6s6YgQnMFh9aR2luzJ1MpHM/6scO9rhFegNsuoemDLTydmQMILvMJCYqE2ggFt0iFHUmzzBYt/eSvYSYCYfupgAKdKUQo6F5D4ABJ9WS+lA4F2KBwtf/Guuv+1AJVdNIPRHjWfVcjLSwgIE=</D></RSAKeyValue>";
    //x编码
    public static string encode(string ori)
    {
        DES.Key = randomDesKey();
        string deskeymm = RSA.RsaEncrypt(DES.Key, rsaPubkey);
        string desmm = DES.DesEncrypt(ori);
        var result = deskeymm + "|" + desmm;
        result = result.Replace("+", "-");
        return result;
    }

    //x解码
    public static string decode(string mm)
    {
        try
        {
            mm = mm.Replace("-", "+");
            var temp = mm.Split('|');
            var deskey = RSA.RsaDecrypt(temp[0], rsaPrikey);
            DES.Key = deskey;
            var data = DES.DesDecrypt(temp[1]);
            return data;
        }
        catch
        {
            return "";
        }
    }

    //x解码
    public static string decodeClient(string mm)
    {
        try
        {
            mm = mm.Replace("-", "+");
            var temp = mm.Split('|');
            var deskey = RSA.RsaDecrypt(temp[0], rsaPrikeyClient);
            DES.Key = deskey;
            var data = DES.DesDecrypt(temp[1]);
            return data;
        }
        catch
        {
            return "";
        }
    }

    static string randomDesKey()
    {
        string temp =  GetTimeStamp() + "sx567";
        //SMain.Enqueue("准备加密:" + temp);
        if (temp.Length >= 17)
        {
            return temp.Substring(9, 8);
        }
        else
        {
            MessageQueue.Enqueue("加密出错:" + temp);
            return "fsd4567a";
        }
    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    public static string GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds).ToString();
    }
}
