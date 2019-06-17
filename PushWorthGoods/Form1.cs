using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PushWorthGoods
{
    public partial class Form1 : Form
    {
        CookieContainer homecookie = new CookieContainer();
        CookieContainer searchcookie = new CookieContainer();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string html1 = HttpHelper.GetRequest("", "https://www.smzdm.com/", ref homecookie, Encoding.UTF8);


            string html2 = HttpHelper.GetRequest("", "http://search.smzdm.com", ref homecookie, Encoding.UTF8);

            string searchWord = Utils.UrlEncode("前置过滤器");
            int page = 1;
            string searchlink = $"http://search.smzdm.com/?c=home&s={searchWord}&p={page}";
            string html = HttpHelper.GetRequest("", searchlink, ref homecookie);

            //主页访问
            Func<CookieContainer, CookieContainer, int, bool> func
                = delegate (CookieContainer homeCookie, CookieContainer htmlCookie, int completedCount)
            {
                homecookie = homeCookie;

                var homecks = HttpHelper.GetCookies(homeCookie);
                var homeck = homecks.FirstOrDefault(p => p.Name == "amvid");

                if (homeck != null)
                {
                    //查询页访问
                    Func<CookieContainer, CookieContainer, int, bool> searchfunc
                        = delegate (CookieContainer shomeCookie, CookieContainer shtmlCookie, int scompletedCount)
                    {
                        searchcookie = shomeCookie;

                        var shomecks = HttpHelper.GetCookies(searchcookie);

                        return false;
                    };

                    WebBrowserHelper searchWeb = new WebBrowserHelper();
                    searchWeb.LoadHtml("https://search.smzdm.com/", searchfunc, searchcookie, homecookie);
                }

                return false;
            };
            WebBrowserHelper webBrowser = new WebBrowserHelper();
            webBrowser.LoadHtml("https://www.smzdm.com/", func, homecookie);




            List<FeedCardChilds> feedCardChilds = new List<FeedCardChilds>() {
                new FeedCardChilds (){ title="Haier 海尔 EG8012B39SU1 8公斤 滚筒洗衣机",
                    picURL ="https://qny.smzdm.com/201906/04/5a4e1e83ae15e7746.jpg_d250.jpg",
                    messageURL ="https://www.smzdm.com/p/14287448/#hfeeds" },
                new FeedCardChilds (){ title="Haier 海尔 BCD-223WDPT 变频风冷无霜三门冰箱 223升",
                    picURL ="https://y.zdmimg.com/201905/21/5ce40d88071801466.jpg_d250.jpg",
                    messageURL ="https://www.smzdm.com/p/14289065/#hfeeds" },
            };


            //发送钉钉推送消息
            //DingDingRobot dingDingRobot = new DingDingRobot();
            //dingDingRobot.Send(feedCardChilds);
        }


        /// <summary>
        /// 页面加载完事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Web_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);

            string webUrl = ((WebBrowser)sender).Url.AbsoluteUri;

            this.textBox1.Text = webUrl;

            this.richTextBox1.Text = ((WebBrowser)sender).DocumentText;
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

        private void button1_Click(object sender, EventArgs e)
        {
            this.webBrowser1.DocumentCompleted += Web_DocumentCompleted;
            this.webBrowser1.Navigate(this.textBox1.Text);
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }
    }
}
