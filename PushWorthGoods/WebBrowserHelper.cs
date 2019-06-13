using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PushWorthGoods
{
    public class WebBrowserHelper
    {
        private int _completedCount = 0;
        private bool _isloading = false;
        private Func<CookieContainer, CookieContainer, int, bool> _func;
        private CookieContainer _loadCookie = null;
        private CookieContainer _htmlCookie = null;
        WebBrowser webBrowser = new WebBrowser();

        /// <summary>
        /// 加载Html页面
        /// </summary>
        /// <param name="url"></param>
        /// <param name="action">
        ///     param:
        ///         CookieContainer-loadCookie;
        ///         CookieContainer-htmlCookie;
        ///         int-CompletedCount;
        ///     return:
        ///         bool-isAllCompleted;
        /// </param>
        /// <param name="loadCookie">
        ///     页面加载时需要的Cookie,可以为空（利用对象的引用类型特点）
        /// </param>
        /// <param name="htmlCookie">
        ///     页面加载过程中产生的Cookie,可以为空（利用对象的引用类型特点）
        ///         如果为空，则将页面加载过程中产生的Cookie设置到loadCookie中
        ///         如果不为空，则页面加载过程中产生的Cookie不会设置到loadCookie中，会单独作为htmlCookie返回
        /// </param>
        public void LoadHtml(string url, Func<CookieContainer, CookieContainer, int, bool> func, CookieContainer loadCookie, CookieContainer htmlCookie = null)
        {
            _completedCount = 0;
            _isloading = true;
            _func = func;
            _loadCookie = loadCookie;
            ThreadPool.QueueUserWorkItem(fun =>
            {
                while (true)
                {
                    if (!_isloading)
                    {
                        return;
                    }
                    Thread.Sleep(500);
                }
            });

            //初始化浏览器Cookie
            if (htmlCookie != null && htmlCookie.Count > 0)
            {
                _htmlCookie = htmlCookie;
                List<Cookie> cookieList = HttpHelper.GetCookies(_htmlCookie);

                foreach (var cookie in cookieList)
                {
                    HttpHelper.InternetSetCookie(url, cookie.Name, cookie.Value);
                }
            }
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
            //开始打开Html
            webBrowser.Navigate(url);
        }

        /// <summary>
        /// 页面加载完事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            webBrowser.Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
            _completedCount++;
            //获取页面Cookie
            string webUrl = ((WebBrowser)sender).Url.AbsoluteUri;
            string cookiesStr = HttpHelper.GetUrlCookies(webUrl);
            //将Cookie添加到对象中
            string[] cookiesArr = cookiesStr.Split(';');
            foreach (string cookieStr in cookiesArr)
            {
                int beginIndex = cookieStr.IndexOf('=');
                string name = beginIndex > 0 ? cookieStr.Substring(0, beginIndex) : "";
                string value = cookieStr.Substring(beginIndex + 1, cookieStr.Length - beginIndex - 1);
                string domain = ((WebBrowser)sender).Document.Domain;

                if (_htmlCookie != null)
                {
                    HttpHelper.AddCookie(ref _htmlCookie, name, value, domain);
                }
                else
                {
                    HttpHelper.AddCookie(ref _loadCookie, name, value, domain);
                }
            }
            //判定是否全部完成
            bool isAllCompleted = _func(_loadCookie, _htmlCookie, _completedCount);

            if (isAllCompleted)
            {
                _completedCount = 0;
                _isloading = false;
                webBrowser.DocumentCompleted -= WebBrowser_DocumentCompleted;
                webBrowser.Document.Window.Error -= new HtmlElementErrorEventHandler(Window_Error);
            }
        }

        /// <summary>
        /// 禁止弹窗提示错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            // Ignore the error and suppress the error dialog box. 
            e.Handled = true;
        }


    }
}
