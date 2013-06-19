using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _4Chal_Live.Controls
{
    /// <summary>
    /// GameInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class GameInfo : UserControl
    {
        public GameInfo()
        {
            InitializeComponent();
        }
        public GameInfo(bool tr)
        {
            InitializeComponent();
            listBlue.Foreground = new SolidColorBrush(Colors.DeepSkyBlue);
            listPurp.Foreground = new SolidColorBrush(Colors.DarkViolet);

            listBlue.FontSize = 15;
            listPurp.FontSize = 15;

            listBlue.Items.Add("          블루 팀");
            listPurp.Items.Add("          퍼플 팀");
        }

        public void addUsers(int teamId, int championId, string Username)
        {
            UserInfo userInfo = new UserInfo();
            userInfo.addData(championId, Username);
            if (teamId == 100)
                listBlue.Items.Add(userInfo);
            else
                listPurp.Items.Add(userInfo);
        }

        public void addBanList(int teamId, int championId)
        {
            Image imgChamp = new Image();
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 200;
            bi.CacheOption = BitmapCacheOption.None;
            bi.UriSource = new Uri(String.Format("http://devbird.programmeduniverse.com/lol/champ_image/{0}.png", championId));
            bi.EndInit();

            imgChamp.Source = bi;
            imgChamp.Width = 32;
            imgChamp.Height = 32;

            imgChamp.ToolTip = champIDtoString(championId);

            if (teamId == 100)
                listBanBlue.Items.Add(imgChamp);
            else
                listBanPurp.Items.Add(imgChamp);


        }

        public void setGameTime(long TimeStamp)
        {
            System.Windows.Threading.DispatcherTimer dTimer = new System.Windows.Threading.DispatcherTimer();

            DateTime dateGame = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dateGame = dateGame.AddSeconds(Math.Round(Convert.ToDouble(TimeStamp) / 1000)).ToLocalTime();
            DateTime dateNow = DateTime.Now;
            TimeSpan timeDiff = dateNow - dateGame;


            dTimer.Tick += delegate
            {
                dateNow = DateTime.Now;
                timeDiff = dateNow - dateGame;
                txtGameTime.Text = timeDiff.ToString(@"mm\:ss");
            };

            dTimer.Interval = new TimeSpan(0, 0, 1);
            dTimer.Start();

        }

        public string champIDtoString(int Id)
        {
            System.Xml.Linq.XDocument xDoc = System.Xml.Linq.XDocument.Load("http://devbird.programmeduniverse.com/lol/data.xml");
            string name = String.Empty;
            var Champions = from node in xDoc.Descendants("Item").AsEnumerable()
                            where node.Element("int").Value == Convert.ToString(Id)
                            select new
                            {
                                Name = node.Element("string").Value
                            };

            foreach (var Champion in Champions)
            {
                name = Champion.Name;
                break;
            }

            return name;

        }


    }
}
