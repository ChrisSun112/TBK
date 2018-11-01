using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tbk.Common
{
	/// <summary>
	/// 帮助类
	/// </summary>
	public class HttpUtility
	{
		/// <summary>
		/// 发送请求
		/// </summary>
		/// <param name="url">Url地址</param>
		/// <param name="data">数据</param>
		public static string SendHttpRequest(string url, string data)
		{
			return SendPostHttpRequest(url, "application/x-www-form-urlencoded", data);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetData(string url)
		{
			return SendGetHttpRequest(url, "application/x-www-form-urlencoded");
		}

        public static string HttpGet(string Url, string postDataStr, string encode)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            HttpWebResponse response;
            request.ContentType = "text/html;charset=";
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                Thread.Sleep(1000 * 30);

                response = (HttpWebResponse)request.GetResponse();
            }

            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding(encode));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }


        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="url">Url地址</param>
        /// <param name="method">方法（post或get）</param>
        /// <param name="method">数据类型</param>
        /// <param name="requestData">数据</param>
        public static string SendPostHttpRequest(string url, string contentType, string requestData)
		{
			WebRequest request = (WebRequest)HttpWebRequest.Create(url);
			request.Method = "POST";
			byte[] postBytes = null;
			request.ContentType = contentType;
			postBytes = Encoding.UTF8.GetBytes(requestData);
			request.ContentLength = postBytes.Length;
			using (Stream outstream = request.GetRequestStream())
			{
				outstream.Write(postBytes, 0, postBytes.Length);
			}
			string result = string.Empty;
			using (WebResponse response = request.GetResponse())
			{
				if (response != null)
				{
					using (Stream stream = response.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						{
							result = reader.ReadToEnd();
						}
					}

				}
			}
			return result;
		}

		/// <summary>
		/// 发送请求
		/// </summary>
		/// <param name="url">Url地址</param>
		/// <param name="method">方法（post或get）</param>
		/// <param name="method">数据类型</param>
		/// <param name="requestData">数据</param>
		public static string SendGetHttpRequest(string url, string contentType)
		{
			WebRequest request = (WebRequest)HttpWebRequest.Create(url);
			request.Method = "GET";
			request.ContentType = contentType;
			string result = string.Empty;
			using (WebResponse response = request.GetResponse())
			{
				if (response != null)
				{
					using (Stream stream = response.GetResponseStream())
					{
						using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						{
							result = reader.ReadToEnd();
						}
					}
				}
			}
			return result;
		}



        public static string UploadFile(string url, string filePath)
        {
            // 时间戳，用做boundary
            string timeStamp = DateTime.Now.Ticks.ToString("x");

            //根据uri创建HttpWebRequest对象
            HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(url));
            httpReq.Method = "POST";
            httpReq.AllowWriteStreamBuffering = false; //对发送的数据不使用缓存
            httpReq.Timeout = 300000;  //设置获得响应的超时时间（300秒）
            httpReq.ContentType = "multipart/form-data; boundary=" + timeStamp;

            //文件
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);

            //头信息
            string boundary = "--" + timeStamp;
            string dataFormat = boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";filename=\"{1}\"\r\nContent-Type:application/octet-stream\r\n\r\n";
            string header = string.Format(dataFormat, "media", Path.GetFileName(filePath));
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(header);

            //结束边界
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + timeStamp + "--\r\n");

            long length = fileStream.Length + postHeaderBytes.Length + boundaryBytes.Length;

            httpReq.ContentLength = length;//请求内容长度

            try
            {
                //每次上传4k
                int bufferLength = 4096;
                byte[] buffer = new byte[bufferLength];

                //已上传的字节数
                long offset = 0;
                int size = binaryReader.Read(buffer, 0, bufferLength);
                Stream postStream = httpReq.GetRequestStream();

                //发送请求头部消息
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);

                while (size > 0)
                {
                    postStream.Write(buffer, 0, size);
                    offset += size;
                    size = binaryReader.Read(buffer, 0, bufferLength);
                }

                //添加尾部边界
                postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                postStream.Close();

                string returnValue = "";
                //获取服务器端的响应
                using (HttpWebResponse response = (HttpWebResponse)httpReq.GetResponse())
                {
                    Stream receiveStream = response.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    returnValue = readStream.ReadToEnd();
                    response.Close();
                    readStream.Close();
                    
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(typeof(HttpUtility),"文件传输异常： " + ex.Message);
                return "";
            }
            finally
            {
                fileStream.Close();
                binaryReader.Close();
            }
        }
    }
}
