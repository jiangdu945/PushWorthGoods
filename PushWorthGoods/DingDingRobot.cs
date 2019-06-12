using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PushWorthGoods
{
    /// <summary>
    /// 钉钉机器人
    /// </summary>
    public class DingDingRobot
    {
        string webhook = "https://oapi.dingtalk.com/robot/send?access_token=d5564d89839210a25bced90e6e75ed7204058ce96ae4905a1fd1096b0553b72f";

        /// <summary>
        /// 消息类型
        /// </summary>
        public enum MsgType
        {
            /// <summary>
            /// 文本
            /// </summary>
            text = 0,
            /// <summary>
            /// 链接
            /// </summary>
            link = 1,
            /// <summary>
            /// 文本标记语言
            /// </summary>
            markdown = 2,
            /// <summary>
            /// 活动卡片
            /// </summary>
            ActionCard = 3,
            /// <summary>
            /// 自由卡片
            /// </summary>
            FeedCard = 4
        }


        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="webhook"></param>
        /// <param name="msgType"></param>
        /// <param name="content"></param>
        public void Send(List<FeedCardChilds> feedCardChilds)
        {
            var feedCard = new
            {
                msgtype = "feedCard",
                feedCard = new
                {
                    links = feedCardChilds.ToArray()
                }
            };

            string content = JsonConvert.SerializeObject(feedCard);

            string str = PostRequest(webhook, content);
            //{ "errcode":0,"errmsg":"ok"}
        }


        /// <summary>
        /// 发起Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"> name=value&name1=value1 </param>
        /// <param name="encoding"></param>
        /// <param name="cookieContainer"></param>
        /// <returns></returns>
        private static string PostRequest(string url, string postData, Encoding encoding = null)
        {
            Stream responseStream = null;
            StreamReader streamReader = null;

            try
            {
                byte[] byteRequest = Encoding.UTF8.GetBytes(postData);

                HttpWebRequest httpWebRequest;
                httpWebRequest = WebRequest.CreateHttp(url);
                httpWebRequest.ContentType = "application/json ;charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = byteRequest.Length;

                //初始化参数
                Stream stream = httpWebRequest.GetRequestStream();
                stream.Write(byteRequest, 0, byteRequest.Length);
                stream.Close();

                //请求页面
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                responseStream = httpWebResponse.GetResponseStream();

                streamReader = new StreamReader(responseStream, encoding ?? Encoding.Default);
                string html = HttpUtility.HtmlDecode(streamReader.ReadToEnd());

                return html;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (streamReader != null)
                {
                    streamReader.Close();
                }
                if (responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }

    }

    public class FeedCardChilds
    {
        /// <summary>
        /// 单条信息文本
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 点击单条信息到跳转链接
        /// </summary>
        public string messageURL { get; set; }
        /// <summary>
        /// 单条信息后面图片的URL
        /// </summary>
        public string picURL { get; set; }
    }
}
