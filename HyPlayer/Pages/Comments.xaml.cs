﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HyPlayer.Classes;
using HyPlayer.Controls;
using NeteaseCloudMusicApi;
using Newtonsoft.Json.Linq;
using Windows.Foundation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace HyPlayer.Pages
{
    /// <summary>
    ///     可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Comments : Page
    {
        private string cursor;
        private int page = 1;
        private string resourceid;
        private int resourcetype;
        private int sortType = 1;

        public Comments()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is string resstr)
            {
                resourceid = resstr.Substring(2);
                switch (resstr.Substring(0, 2))
                {
                    case "sg":
                        resourcetype = 0;
                        break;
                    case "mv":
                        resourcetype = 1;
                        break;
                    case "fm":
                        resourcetype = 4;
                        break;
                    case "mb":
                        resourcetype = 7;
                        break;
                    case "al":
                        resourcetype = 3;
                        break;
                    case "pl":
                        resourcetype = 2;
                        break;
                }
            }

            LoadHotComments();
            LoadComments(sortType);
        }


        private void LoadHotComments()
        {
            LoadComments(2, HotCommentList);
        }

        private async void LoadComments(int type, StackPanel addingPanel = null)
        {
            if (addingPanel == null)
                addingPanel = CommentList;
            // type 1:按推荐排序,2:按热度排序,3:按时间排序
            if (string.IsNullOrEmpty(resourceid)) return;
            (bool isOk, JObject json) res;

            res = await Common.ncapi.RequestAsync(CloudMusicApiProviders.CommentNew,
                new Dictionary<string, object>
                {
                    {"cursor", cursor}, {"id", resourceid}, {"type", resourcetype}, {"pageNo", page}, {"pageSize", 20},
                    {"sortType", type}
                });
            if (res.isOk)
            {
                addingPanel.Children.Clear();
                foreach (var comment in res.json["data"]["comments"].ToArray())
                    addingPanel.Children.Add(
                        new SingleComment(Comment.CreateFromJson(comment, resourceid, resourcetype)));
                if (type == 3)
                    cursor = res.json["data"]["cursor"].ToString();
                if (res.json["data"]["hasMore"].ToString() == "True")
                    NextPage.IsEnabled = true;
                else
                    NextPage.IsEnabled = false;

                if (page > 1)
                    PrevPage.IsEnabled = true;
                else
                    PrevPage.IsEnabled = false;

                PageIndicator.Text =
                    $"第 {page} 页 / 共 {Math.Ceiling((decimal)res.json["data"]["totalCount"].ToObject<long>() / 20).ToString()} 页";
            }
        }


        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            page++;
            LoadComments(sortType);
            ScrollTop();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            page--;
            LoadComments(sortType);
            ScrollTop();
        }

        private async void SendComment_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(CommentEdit.Text) && Common.Logined)
            {
                try
                {
                    await Common.ncapi.RequestAsync(CloudMusicApiProviders.Comment,
                        new Dictionary<string, object>
                            {{"id", resourceid}, {"type", resourcetype}, {"t", "1"}, {"content", CommentEdit.Text}});
                    CommentEdit.Text = string.Empty;
                    await Task.Delay(1000);
                    LoadComments(1);
                }
                catch (Exception ex)
                {
                    var dlg = new MessageDialog(ex.Message, "出现问题，评论失败");
                    await dlg.ShowAsync();
                }
            }
            else if (string.IsNullOrWhiteSpace(CommentEdit.Text))
            {
                var dlg = new MessageDialog("评论不能为空");
                await dlg.ShowAsync();
            }
            else
            {
                var dlg = new MessageDialog("请先登录");
                await dlg.ShowAsync();
            }
        }

        private void ComboBoxSortType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sortType = ComboBoxSortType.SelectedIndex + 1;
            LoadComments(sortType);
        }

        private void SkipPage_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PageSelect.Text, out page))
            {
                LoadComments(sortType);
                ScrollTop();
            }
        }
        private void ScrollTop()
        {
            Windows.UI.Xaml.Media.GeneralTransform transform = AllCmtsTB.TransformToVisual(MainScroll);
            Point point = transform.TransformPoint(new Point(0, 0));
            double y = point.Y + MainScroll.VerticalOffset;
            MainScroll.ChangeView(null, y, null, false);

        }

        private void BackToTop_Click(object sender, RoutedEventArgs e)
        {
            ScrollTop();
        }

        private void MainScroll_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            Windows.UI.Xaml.Media.GeneralTransform transform = AllCmtsTB.TransformToVisual(MainScroll);
            Point point = transform.TransformPoint(new Point(0, 0));
            double y = point.Y + MainScroll.VerticalOffset;
            if ((sender as ScrollViewer).VerticalOffset > y + 25)
                BackToTop.Visibility = Visibility.Visible;
            else BackToTop.Visibility = Visibility.Collapsed;
        }
    }
}