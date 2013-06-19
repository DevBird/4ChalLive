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
    /// UserInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UserInfo : UserControl
    {
        public UserInfo()
        {
            InitializeComponent();
        }

        public void addData(int championId, string Username)
        {
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.DecodePixelWidth = 200;
            bi.CacheOption = BitmapCacheOption.None;
                                                #error Champion Image URL Here
            bi.UriSource = new Uri(String.Format("HERE", championId));
            bi.EndInit();

            imgChamp.Source = bi;

            imgChamp.ToolTip = "";

            txtUsername.Text = Username;

            imgChamp.ToolTip = champIDtoString(championId);
        }

        public string champIDtoString(int Id)
        {
            System.Xml.Linq.XDocument xDoc;
            try
            {
                xDoc = System.Xml.Linq.XDocument.Load("http://devbird.programmeduniverse.com/lol/data.xml");
            }
            catch
            {
                xDoc = System.Xml.Linq.XDocument.Load("http://cfs.tistory.com/custom/blog/117/1172082/skin/images/data.xml");
            }
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