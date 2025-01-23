using System;
using System.Text;
using System.Collections;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utils
{
    class HttpHelper
    {
        #region 字段
        private ArrayList bytesArray;
        private Encoding encoding = Encoding.UTF8;
        private string boundary = String.Empty;
        #endregion

        #region 构造方法
        public HttpHelper()
        {
            bytesArray = new ArrayList();
            string flag = DateTime.Now.Ticks.ToString("x");
            boundary = "---------------------------" + flag;
        }
        #endregion


        /// <summary>
        /// 合并请求数据
        /// </summary>
        /// <returns></returns>
        private byte[] MergeContent()
        {
            int length = 0;
            int readLength = 0;
            string endBoundary = "--" + boundary + "--\r\n";
            byte[] endBoundaryBytes = encoding.GetBytes(endBoundary);

            bytesArray.Add(endBoundaryBytes);

            foreach (byte[] b in bytesArray)
            {
                length += b.Length;
            }

            byte[] bytes = new byte[length];

            foreach (byte[] b in bytesArray)
            {
                b.CopyTo(bytes, readLength);
                readLength += b.Length;
            }

            return bytes;
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="requestUrl">请求url</param>
        /// <param name="responseText">响应</param>
        /// <returns></returns>
        public bool Upload(String requestUrl, out String responseText, string method = "POST")
        {
            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + boundary);

            byte[] responseBytes;
            byte[] bytes = MergeContent();

            try
            {
                responseBytes = webClient.UploadData(requestUrl, method, bytes);
                responseText = System.Text.Encoding.UTF8.GetString(responseBytes);
                return true;
            }
            catch (WebException ex)
            {
                Stream responseStream = ex.Response.GetResponseStream();
                responseBytes = new byte[ex.Response.ContentLength];
                responseStream.Read(responseBytes, 0, responseBytes.Length);
            }
            responseText = System.Text.Encoding.UTF8.GetString(responseBytes);
            return false;
        }

        /// <summary>
        /// 设置表单数据字段
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldValue">字段值</param>
        /// <returns></returns>
        public void SetFieldValue(String fieldName, String fieldValue)
        {
            string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
            string httpRowData = String.Format(httpRow, fieldName, fieldValue);

            bytesArray.Add(encoding.GetBytes(httpRowData));
        }

        /// <summary>
        /// 设置表单文件数据
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="filename">字段值</param>
        /// <param name="contentType">内容内型</param>
        /// <param name="fileBytes">文件字节流</param>
        /// <returns></returns>
        public void SetFieldValue(String fieldName, String filename, String contentType, Byte[] fileBytes)
        {
            string end = "\r\n";
            string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string httpRowData = String.Format(httpRow, fieldName, filename, contentType);

            byte[] headerBytes = encoding.GetBytes(httpRowData);
            byte[] endBytes = encoding.GetBytes(end);
            byte[] fileDataBytes = new byte[headerBytes.Length + fileBytes.Length + endBytes.Length];

            headerBytes.CopyTo(fileDataBytes, 0);
            fileBytes.CopyTo(fileDataBytes, headerBytes.Length);
            endBytes.CopyTo(fileDataBytes, headerBytes.Length + fileBytes.Length);

            bytesArray.Add(fileDataBytes);
        }






        public static string request(string url)
        {
            string responseString = "";
            try
            {
                //  Debug.Print("发出是：" + url);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "textml;charset=UTF-8";
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                //   Debug.Print("收到的是" + responseString);
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
            }
            return responseString;
        }

        public delegate void RequestEvent(string error, string response);
        public static void requestSyncNoJson(string url, Action<string, string> callBack = null)
        {
            Action<string, RequestEvent> caller = requestThread;
            IAsyncResult result = caller.BeginInvoke(url, delegate (string error, string info)
            {
                callBack?.Invoke(error, info);
            }, ar =>
            {
                caller.EndInvoke(ar);
            }, caller);
        }

        public struct HttpResult {
            public string error;
            public string info;
        }

        public static Task<HttpResult> requestSync(string url)
        {
            TaskCompletionSource<HttpResult> tcs = new TaskCompletionSource<HttpResult>();
            Action<string, RequestEvent> caller = requestThread;
            IAsyncResult result = caller.BeginInvoke(url, delegate (string error, string info)
            {
                tcs.SetResult(new HttpResult() { error = error, info = info });
            }, ar =>
            {
                caller.EndInvoke(ar);
            }, caller);
            return tcs.Task;
        }

        public static void requestSync<T>(string url, Action<string, T> callBack = null)
        {
            Action<string, RequestEvent> caller = requestThread;
            IAsyncResult result = caller.BeginInvoke(url, delegate (string error, string info)
            {
                if (error != null)
                {
                    callBack?.Invoke(error, default(T));
                }
                try
                {
                    var jsonResult = Coding<T>.decode(info);
                    callBack?.Invoke(error, jsonResult);
                }
                catch (Exception e)
                {
                    callBack?.Invoke("json解析错误: " + e.Message, default(T));
                }

            }, ar =>
            {
                caller.EndInvoke(ar);
            }, caller);
        }

        static void requestThread(string url, RequestEvent callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "textml;charset=UTF-8";
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string info = new StreamReader(response.GetResponseStream()).ReadToEnd();
                callback.Invoke(null, info);
            }
            catch (Exception e)
            {
                callback.Invoke(e.ToString(), null);
            }
        }



        //文件下载
        public delegate void AssetLoadEvent(string err);
        public delegate void AssetLoadProgress(float progress);
        public static void downloadFile(string url, string path, AssetLoadEvent callback, AssetLoadProgress progress = null)
        {
            int a = path.LastIndexOf('\\');
            string directory = path.Substring(0, a);
            if (File.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (Exception err)
            {
                Debug.Print("下载文件失败" + url + err.ToString());
                if (callback != null) callback(err.ToString());
                return;
            }

            long totalBytes = response.ContentLength;
            Stream responseStream = response.GetResponseStream();
            Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            long totalDownloadedByte = 0;
            try
            {
                byte[] bArr = new byte[1024];
                int size = responseStream.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    totalDownloadedByte = size + totalDownloadedByte;
                    stream.Write(bArr, 0, size);
                    size = responseStream.Read(bArr, 0, (int)bArr.Length);
                    if (progress != null)
                    {
                        progress((float)totalDownloadedByte / (float)totalBytes * 100);
                    }
                    //System.Windows.Forms.Application.DoEvents(); //必须加注这句代码，否则label1将因为循环执行太快而来不及显示信息
                    //  Thread.Sleep(10);
                }
                stream.Close();
                responseStream.Close();
                //下载成功
                if (callback != null) callback(null);
            }
            catch (Exception e)
            {
                Debug.Print("下载文件出错:" + e);
                if (callback != null) callback(e.ToString());
            }
            finally
            {
                stream.Close();
                responseStream.Close();
            }
        }
    }
}