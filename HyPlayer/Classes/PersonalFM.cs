﻿using HyPlayer.HyPlayControl;
using NeteaseCloudMusicApi;

namespace HyPlayer.Classes
{
    internal static class PersonalFM
    {
        public static void InitPersonalFM()
        {
            LoadNextFM();
            HyPlayList.OnSongMoveNext += HyPlayList_OnSongMoveNext;
            HyPlayList.OnMediaEnd += HyPlayList_OnMediaEnd;
            Common.GLOBAL["PERSONALFM"] = "true";
        }

        public static void ExitFm()
        {
            HyPlayList.OnSongMoveNext -= HyPlayList_OnSongMoveNext;
            HyPlayList.OnMediaEnd -= HyPlayList_OnMediaEnd;
            Common.GLOBAL["PERSONALFM"] = "false";
            HyPlayList.RemoveAllSong();
        }

        private static void HyPlayList_OnMediaEnd(HyPlayItem hpi)
        {
            if (Common.GLOBAL["PERSONALFM"].ToString() == "true")
                LoadNextFM();
        }

        public static async void LoadNextFM()
        {
            var (isOk, json) = await Common.ncapi.RequestAsync(CloudMusicApiProviders.PersonalFm);
            if (isOk)
            {
                var song = json["data"][0];
                HyPlayList.RemoveAllSong();
                var item = await HyPlayList.AppendNCSong(NCSong.CreateFromJson(song));
                item.ItemType = HyPlayItemType.Netease;
                Common.GLOBAL["PERSONALFM"] = "true";
                HyPlayList.SongAppendDone();
                HyPlayList.SongMoveTo(0);
            }
        }

        private static void HyPlayList_OnSongMoveNext()
        {
            if (Common.GLOBAL["PERSONALFM"].ToString() == "true")
                LoadNextFM();
        }
    }
}