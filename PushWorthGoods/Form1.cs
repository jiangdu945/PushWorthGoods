using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PushWorthGoods
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string searchlink = "http://search.smzdm.com/?s=%E5%89%8D%E7%BD%AE%E8%BF%87%E6%BB%A4%E5%99%A8";








            List<FeedCardChilds> feedCardChilds = new List<FeedCardChilds>() {
                new FeedCardChilds (){ title="Haier 海尔 EG8012B39SU1 8公斤 滚筒洗衣机",
                    picURL ="https://qny.smzdm.com/201906/04/5a4e1e83ae15e7746.jpg_d250.jpg",
                    messageURL ="https://www.smzdm.com/p/14287448/#hfeeds" },
                new FeedCardChilds (){ title="Haier 海尔 BCD-223WDPT 变频风冷无霜三门冰箱 223升",
                    picURL ="https://y.zdmimg.com/201905/21/5ce40d88071801466.jpg_d250.jpg",
                    messageURL ="https://www.smzdm.com/p/14289065/#hfeeds" },
            };


            DingDingRobot dingDingRobot = new DingDingRobot();
            dingDingRobot.Send(feedCardChilds);
        }
    }
}
