﻿using System;
using System.Collections.Generic;
using System.Timers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HyPlayer.Controls;
using HyPlayer.HyPlayControl;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.Pages
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadPage : Page
    {
        private Timer timer;

        public DownloadPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed += DownloadPage_Elapsed;
                timer.Start();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            timer = null;
        }

        private void DownloadPage_Elapsed(object sender, ElapsedEventArgs e)
        {
            Common.Invoke(() =>
            {
                if (DLList.Children.Count != DownloadManager.DownloadLists.Count)
                {
                    while (DLList.Children.Count > DownloadManager.DownloadLists.Count)
                        DLList.Children.RemoveAt(DLList.Children.Count - 1);

                    while (DLList.Children.Count < DownloadManager.DownloadLists.Count)
                        DLList.Children.Add(new SingleDownload(DLList.Children.Count));
                }

                foreach (SingleDownload dl in DLList.Children) dl.UpdateUI();
            });
        }

        private void Button_Cleanall_Click(object sender, RoutedEventArgs e)
        {
            DownloadManager.DownloadLists.ForEach(t =>
            {
                if (t.Status == 1)
                    t.downloadOperation = null;
            });
            DownloadManager.DownloadLists = new List<DownloadObject>();
        }

        private async void OpenDownloadFolder_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderPathAsync(Common.Setting.downloadDir);
        }
    }
}