using _4Chal_Live.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace _4Chal_Live
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
 string[] platformId = new string[5];
        string[] encryptionKey = new string[5];
        string lolServer;
        
        int[] gameId = new int[5];


        BackgroundWorker bw = new BackgroundWorker();
        public MainWindow()
        {

            lolServer = "KR";

            InitializeComponent();
            if (!Directory.Exists(Properties.Settings.Default.RADS))
                FindRADSPath();
            Style s = new Style();
            s.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
            tabGame.ItemContainerStyle = s;

            tabGame.SelectionChanged += delegate { txtNav.Text = (tabGame.SelectedIndex + 1) + " / " + tabGame.Items.Count; };

            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
        }

        #region BackgroundWorker Events
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Stream data =new MemoryStream();
            while (true)
            {
                try
                {
                    data = responseData();
                    break;
                }
                catch
                {
                    if (MessageBox.Show("데이터 요청 시간 초과! 서버 상태를 확인해주세요. \n다시 시도하겠습니까? (아니오 : 종료)", "오류", MessageBoxButton.YesNo) == MessageBoxResult.No)
                        Environment.Exit(1);
                }
            }


            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(Spectator));
            Spectator list = dcjs.ReadObject(data) as Spectator;
            data.Close();

            GameInfo[] gameInfo = new GameInfo[list.gameList.Length];
            int i, j;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
                {

                    for (i = 0; i < list.gameList.Length; i++)
                    {
                        changeLoadStatus(2);

                        gameInfo[i] = new GameInfo(false);
                        for (j = 0; j < list.gameList[i].participants.Length; j++)
                        {
                            gameInfo[i].addUsers(list.gameList[i].participants[j].teamId, list.gameList[i].participants[j].championId, list.gameList[i].participants[j].summonerName);
                            
                        }

                        for (j = 0; j < list.gameList[i].bannedChampions.Length; j++)
                        {
                            gameInfo[i].addBanList(list.gameList[i].bannedChampions[j].teamId, list.gameList[i].bannedChampions[j].championId);
                        }
                        gameInfo[i].setGameTime(list.gameList[i].gameStartTime);

                        platformId[i] = list.gameList[i].platformId;
                        encryptionKey[i] = list.gameList[i].observers.encryptionKey;
                        gameId[i] = list.gameList[i].gameId;
                    }
                }));
            
            e.Result = gameInfo;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GameInfo[] gameInfo = (GameInfo[])e.Result;

            Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(delegate()
            {
            int i;
            for (i = 0; i < gameInfo.Length; i++)
            {
                TabItem tab = new TabItem();
                tab.Content = gameInfo[i];
                tabGame.Items.Add(tab);

            }
            tabGame.SelectedIndex = 0;


                txtNav.Text = "1 / 5";

                grdMenu.IsEnabled = true;
                btPrev.IsEnabled = false;
                btNext.IsEnabled = true;
                grdLoading.Visibility = Visibility.Hidden;
                tabGame.Visibility = Visibility.Visible;
            }));
            
        }
        #endregion
        private void mainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            bw.RunWorkerAsync();
        }

        #region NavButton Click Event
        private void btPrev_Click(object sender, RoutedEventArgs e)
        {
            if (tabGame.SelectedIndex == 1) btPrev.IsEnabled = false;
            if (tabGame.SelectedIndex == 4) btNext.IsEnabled = true;
            tabGame.SelectedIndex--;
        }

        private void btNext_Click(object sender, RoutedEventArgs e)
        {
            if (tabGame.SelectedIndex == 0) btPrev.IsEnabled = true;
            if (tabGame.SelectedIndex == 3) btNext.IsEnabled = false;
            tabGame.SelectedIndex++;
        }
        #endregion

        void changeLoadStatus(int statCode)
        {
            string statString = "ERROR";
            if (statCode == 0) statString = "대기 중";
            else if (statCode == 1) statString = "데이터 가져오는 중";
            else if (statCode == 2) statString = "데이터 분석 중";
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate
            {
                if (statCode == 0)
                {
                    grdMenu.IsEnabled = false;
                    grdLoading.Visibility = Visibility.Visible;
                    tabGame.Visibility = Visibility.Hidden;
                    tabGame.Items.Clear();
                    
                    pbProgress.Value = 0;

                }
                else pbProgress.Value += 10;

                tbStatus.Text = statString;
            }));
        }

        private void btSpect_Click(object sender, RoutedEventArgs e)
        {

            string[] dirs = Directory.GetDirectories(Properties.Settings.Default.RADS + @"\solutions\lol_game_client_sln\releases", "*.*.*.*");

            ProcessStartInfo info = new ProcessStartInfo(dirs[0] + @"\deploy\League of Legends.exe");
            info.Arguments = String.Format("\"8394\" \"LoLLauncher.exe\" \"\" \"spectator spectator.{0}.lol.riotgames.com:8088 {1} {2} {3}\"", lolServer[0], encryptionKey[tabGame.SelectedIndex], gameId[tabGame.SelectedIndex] ,platformId[tabGame.SelectedIndex] );
            info.WorkingDirectory = dirs[0] + @"\deploy\";
            Process process = new Process();
            process.StartInfo = info;
            process.Start();

        }

        private void btReload_Click(object sender, RoutedEventArgs e)
        {
            bw.RunWorkerAsync();
        }

        void FindRADSPath()
        {
            const string RADSValue = "LocalRootFolder";
            string RADSPath = "";
            RegistryKey key;
            key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\RIOT GAMES\RADS");

            UserStringComparer usc = new UserStringComparer();

            if (key != null && key.GetValueNames().Contains(RADSValue, usc))
                if (Directory.Exists(key.GetValue(RADSValue).ToString()))
                {
                    RADSPath = key.GetValue(RADSValue).ToString();
                    if (MessageBox.Show(RADSPath + "가 리그 오브 레전드가 설치된 경로가 맞습니까?", "경로 설정", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Properties.Settings.Default.RADS = RADSPath;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        CustomFolderSelection();
                    }


                }
                else
                {
                    CustomFolderSelection();
                }
        }

        void CustomFolderSelection()
        {

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "리그 오브 레전드가 설치된 경로 내에 있는 RADS 폴더를 선택해주세요!";
            while (true)
            {
                if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    if (fbd.SelectedPath.Substring(fbd.SelectedPath.Length - 4,4) ==  "RADS")
                    {
                        Properties.Settings.Default.RADS = fbd.SelectedPath;
                        Properties.Settings.Default.Save();
                        break;
                    }
                    else
                    {
                        MessageBox.Show("리그 오브 레전드 설치 폴더 내에 있는 RADS 폴더를 선택해주세요!");
                    }
                }
            }
        }

        Stream responseData()
        {
            changeLoadStatus(0);
            WebRequest request = WebRequest.Create(String.Format("http://spectator.{0}.lol.riotgames.com:8088/observer-mode/rest/featured", lolServer));
            request.Timeout = 20000;
            changeLoadStatus(1);
            WebResponse response = request.GetResponse();
            return response.GetResponseStream();
        }
        private void btChgServ_Click(object sender, RoutedEventArgs e)
        {
            
            btChgServ.ContextMenu.Visibility = Visibility.Visible;
            btChgServ.ContextMenu.IsOpen = true;

        }

        private void btChgServ_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            btChgServ.ContextMenu.Visibility = Visibility.Collapsed;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mItem = (MenuItem)sender;

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(delegate()
            {
                int i;
                for (i = 0; i < btChgServ.ContextMenu.Items.Count; i++)
                {
                    if (btChgServ.ContextMenu.Items[i] != mItem) ((MenuItem)btChgServ.ContextMenu.Items[i]).IsChecked = false;
                }
                mItem.IsChecked = true;
            }));
            if (lolServer != mItem.Tag.ToString())
            {
                lolServer = mItem.Tag.ToString();
                bw.RunWorkerAsync();
            }
        }



    }



    class UserStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            if (Object.ReferenceEquals(x, y)) return true;
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;
            return x.ToUpper() == y.ToUpper();
        }
        public int GetHashCode(string x)
        {
            if (Object.ReferenceEquals(x, null)) return 0;
            return x.ToUpper().GetHashCode();
        }
    }
}
