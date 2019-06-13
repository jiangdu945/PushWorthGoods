using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PushWorthGoods
{

    public class HttpHelper
    {
        /// <summary>
        /// 用户代理
        /// </summary>
        private static string _agent { get { return "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36"; } }

        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="referer"></param>
        /// <param name="url"></param>
        /// <param name="encoding"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public static string GetImage(string referer, string url, ref CookieContainer cookieContainer)
        {
            Stream responseStream = null;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.Referer = string.IsNullOrEmpty(referer) ? url : referer;
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = _agent;
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 3000;
                httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
                //发起请求，接收返回信息
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //获取返回流
                responseStream = httpWebResponse.GetResponseStream();
                //保存图片
                System.Drawing.Bitmap bit = new System.Drawing.Bitmap(System.Drawing.Image.FromStream(responseStream));
                string fileName = $"image{DateTime.Now.ToString("HHmmss") + DateTime.Now.Millisecond}.jpg";
                string filePath = AppDomain.CurrentDomain.BaseDirectory + $"\\Imgs\\{DateTime.Now.ToString("yyyy_MM_dd")}\\";

                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                bit.Save(filePath + fileName);

                //交换cookies
                if (httpWebResponse.Cookies != null && httpWebResponse.Cookies.Count > 0)
                {
                    CookieCollection ckColl = cookieContainer.GetCookies(new Uri("https://" + httpWebResponse.Cookies[0].Domain));
                    foreach (Cookie newCookie in httpWebResponse.Cookies)
                    {
                        newCookie.Expires = DateTime.Now.AddYears(1);

                        bool IsHasSame = false;
                        foreach (Cookie oldCookie in ckColl)
                        {
                            if (newCookie.Name == oldCookie.Name)
                            {
                                oldCookie.Value = newCookie.Value;
                                IsHasSame = true;
                                break;
                            }
                        }

                        if (!IsHasSame)
                            cookieContainer.Add(newCookie);
                    }
                }

                return filePath + fileName;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (responseStream != null)
                    responseStream.Close();
            }
        }

        /// <summary>
        /// 发起Get请求
        /// </summary>
        /// <param name="referer"></param>
        /// <param name="url"></param>
        /// <param name="cookieContainer"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string GetRequest(string referer, string url, ref CookieContainer cookieContainer, Encoding encoding = null)
        {
            Stream responseStream = null;
            StreamReader streamReader = null;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                HttpWebRequest httpWebRequest = WebRequest.CreateHttp(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.Referer = string.IsNullOrEmpty(referer) ? url : referer;
                httpWebRequest.Accept = "*/*";
                httpWebRequest.UserAgent = _agent;
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 3000;
                httpWebRequest.KeepAlive = true;
                httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                responseStream = httpWebResponse.GetResponseStream();
                streamReader = new StreamReader(responseStream, encoding ?? Encoding.Default);

                string html = HttpUtility.HtmlDecode(streamReader.ReadToEnd());

                //交换cookies
                AddCookie(ref cookieContainer, httpWebResponse.Cookies);

                return html;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
                if (responseStream != null)
                    responseStream.Close();
            }
        }

        /// <summary>
        /// 发起Post请求
        /// </summary>
        /// <param name="referer"></param>
        /// <param name="url"></param>
        /// <param name="postData"> name=value&name1=value1 </param>
        /// <param name="encoding"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public static string PostRequest(string referer, string url, string postData, ref CookieContainer cookieContainer, Encoding encoding = null)
        {
            Stream responseStream = null;
            StreamReader streamReader = null;

            try
            {
                byte[] byteRequest = Encoding.Default.GetBytes(postData);
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);

                HttpWebRequest httpWebRequest;
                httpWebRequest = WebRequest.CreateHttp(url);
                httpWebRequest.CookieContainer = cookieContainer;
                httpWebRequest.ContentType = "application/x-www-form-urlencoded;charset=utf-8";
                httpWebRequest.Referer = string.IsNullOrEmpty(referer) ? url : referer;
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 3000;
                httpWebRequest.Accept = "application/json, text/javascript, */*; q=0.01";
                httpWebRequest.UserAgent = _agent;
                httpWebRequest.KeepAlive = true;
                httpWebRequest.ContentLength = byteRequest.Length;
                httpWebRequest.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);

                //初始化参数
                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                //请求页面
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                responseStream = httpWebResponse.GetResponseStream();

                streamReader = new StreamReader(responseStream, encoding ?? Encoding.Default);
                string html = HttpUtility.HtmlDecode(streamReader.ReadToEnd());

                //交换cookies
                AddCookie(ref cookieContainer, httpWebResponse.Cookies);

                return html;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
                if (responseStream != null)
                    responseStream.Close();
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }

        /// <summary>
        /// 读取网页Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieName"></param>
        /// <param name="cookieData"></param>
        /// <param name="uintCookieData"></param>
        /// <param name="flags"></param>
        /// <param name="reserved"></param>
        /// <returns></returns>
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetGetCookie(string lpszUrlName, string lbszCookieName, StringBuilder lpszCookieData, ref int lpdwSize);
        /// <summary>
        /// 写入网页Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="cookieName"></param>
        /// <param name="cookieValue"></param>
        /// <returns></returns>
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string url, string cookieName, string cookieValue);

        /// <summary>
        /// 获取Url的Cookies
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetUrlCookies(string url)
        {
            int datasize = 1000;
            StringBuilder cookieData = new StringBuilder((int)datasize);
            if (!InternetGetCookie(url, null, cookieData, ref datasize))
            {
                if (datasize < 0)
                    return null;

                cookieData = new StringBuilder((int)datasize);
                if (!InternetGetCookie(url, null, cookieData, ref datasize))
                    return null;
            }
            return cookieData.ToString();
        }

        /// <summary>
        /// 获取CookieContainer里的所有cookie
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public static List<Cookie> GetCookies(CookieContainer cookieContainer)
        {
            List<Cookie> lstCookies = new List<Cookie>();
            Hashtable table = (Hashtable)cookieContainer.GetType().InvokeMember("m_domainTable",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
                System.Reflection.BindingFlags.Instance, null, cookieContainer, new object[] { });
            foreach (object pathList in table.Values)
            {
                SortedList lstCookieCol = (SortedList)pathList.GetType().InvokeMember("m_list",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField
                    | System.Reflection.BindingFlags.Instance, null, pathList, new object[] { });
                foreach (CookieCollection colCookies in lstCookieCol.Values)
                    foreach (Cookie c in colCookies) lstCookies.Add(c);
            }
            return lstCookies;
        }

        /// <summary>
        /// 以字符串方式显示CookieContainer内详细信息
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        public static string GetCookieString(CookieContainer cookieContainer)
        {
            List<Cookie> cookieList = GetCookies(cookieContainer);
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < cookieList.Count; i++)
            {
                builder.Append("["); builder.Append(i); builder.Append("]:");
                builder.Append("  Name:"); builder.Append(cookieList[i].Name); builder.Append("\n");
                builder.Append("  Value:"); builder.Append(cookieList[i].Value); builder.Append("\n");
            }
            return builder.ToString();
        }

        /// <summary>
        /// 获取Cookie的值
        /// </summary>
        /// <param name="cookieContainer">Cookie集合对象</param>
        /// <param name="cookieName">Cookie名称</param>
        /// <returns>返回Cookie名称对应值</returns>
        public static string GetCookie(CookieContainer cookieContainer, string cookieName)
        {
            List<Cookie> cookieList = GetCookies(cookieContainer);
            var model = cookieList.Find(p => p.Name == cookieName);
            if (model != null)
            {
                return model.Value;
            }
            return string.Empty;
        }

        /// <summary>
        /// 新增Cookie
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static void AddCookie(ref CookieContainer cookieContainer, string name, string value, string domain)
        {
            if (cookieContainer == null || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            Cookie cookie = new Cookie();
            cookie.Name = name.Trim();
            cookie.Value = value.Trim().Replace(",", "%2C");
            cookie.Expires = DateTime.Now.AddYears(1);
            cookie.Domain = domain;

            CookieCollection cookieCollectionTemp = new CookieCollection();
            cookieCollectionTemp.Add(cookie);
            AddCookie(ref cookieContainer, cookieCollectionTemp);
        }

        /// <summary>
        /// 新增Cookie
        /// </summary>
        /// <param name="cookieContainer"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static void AddCookie(ref CookieContainer cookieContainer, CookieCollection cookieCollection)
        {
            if (cookieContainer == null || cookieCollection == null || cookieCollection.Count <= 0)
            {
                return;
            }

            CookieCollection cookieCollectionTemp = new CookieCollection();
            List<Cookie> cookieList = GetCookies(cookieContainer);

            foreach (Cookie oldc in cookieList)
            {
                bool isSame = false;
                foreach (Cookie newc in cookieCollection)
                {
                    //调整过期时间，避免Cookie无法添加到CookieContainer中
                    newc.Expires = DateTime.Now.AddYears(1);
                    if (oldc.Name == newc.Name)
                    {
                        isSame = true;
                        break;
                    }
                }

                if (!isSame)
                {
                    oldc.Expires = DateTime.Now.AddYears(1);
                    cookieCollectionTemp.Add(oldc);
                    isSame = false;
                }
            }

            cookieContainer = new CookieContainer();
            cookieContainer.Add(cookieCollectionTemp);
            cookieContainer.Add(cookieCollection);
        }


    }


}
