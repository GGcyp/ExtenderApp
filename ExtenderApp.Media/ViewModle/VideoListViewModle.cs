﻿using System.Collections.ObjectModel;
using System.IO;
using ExtenderApp.Abstract;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Media
{
    /// <summary>
    /// 视频列表视图模型类，继承自 ExtenderAppViewModel 类，用于处理视频列表的显示和交互。
    /// </summary>
    /// <typeparam name="VideoListView">视频列表视图类型</typeparam>
    /// <typeparam name="MediaModel">媒体模型类型</typeparam>
    public class VideoListViewModle : ExtenderAppViewModel<VideoListView, MediaModel>
    {
        private readonly HashSet<Uri> _medaiPathHash;

        /// <summary>
        /// 视频列表集合
        /// </summary>
        public ObservableCollection<VideoInfo> Videos => Model.VideoInfos;

        /// <summary>
        /// 初始化 VideoListViewModle 类的新实例。
        /// </summary>
        /// <param name="serviceStore">服务存储对象</param>
        public VideoListViewModle(IServiceStore serviceStore) : base(serviceStore)
        {
            _medaiPathHash = new();

            Model.AddVideoPathAction = AddVideoPath;
            for (int i = 0; i < Videos.Count; i++)
            {
                _medaiPathHash.Add(Videos[i].VideoUri);
            }
        }

        /// <summary>
        /// 添加视频路径到视频列表。
        /// </summary>
        /// <param name="videoPath">视频路径</param>
        /// <returns>如果视频路径已存在，则返回 false；否则返回 true。</returns>
        private VideoInfo AddVideoPath(string videoPath)
        {
            Uri uri = new Uri(videoPath);
            if (_medaiPathHash.Contains(uri))
            {
                return null;
            }

            var videoInfo = new VideoInfo(uri);
            Videos.Add(videoInfo);
            _medaiPathHash.Add(uri);
            SaveModel();
            return videoInfo;
        }

        /// <summary>
        /// 添加选中的视频文件数据组
        /// </summary>
        /// <param name="videoPaths"></param>
        public void AddVideoPaths(string[] videoPaths)
        {
            if (videoPaths is null) return;

            for (int i = 0; i < videoPaths.Length; i++)
            {
                Uri uri = new Uri(videoPaths[i]);
                if (_medaiPathHash.Contains(uri))
                {
                    return;
                }

                var videoInfo = new VideoInfo(uri);
                Videos.Add(videoInfo);
                _medaiPathHash.Add(uri);
            }
            SaveModel();
        }

        public void AddFolder(string[] folderPaths)
        {
            if (folderPaths is null) return;

            foreach (var folderPath in folderPaths)
            {
                if (Directory.Exists(folderPath))
                {
                    var videoFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                                              .Where(file => file.EndsWith(".mp4") || file.EndsWith(".avi") || file.EndsWith(".mkv") || file.EndsWith(".mov") || file.EndsWith(".wmv"));

                    foreach (var videoFile in videoFiles)
                    {
                        Uri uri = new Uri(videoFile);
                        if (!_medaiPathHash.Contains(uri))
                        {
                            var videoInfo = new VideoInfo(uri);
                            Videos.Add(videoInfo);
                            _medaiPathHash.Add(uri);
                        }
                    }
                }
            }
            SaveModel();
        }

        /// <summary>
        /// 打开视频
        /// </summary>
        /// <param name="videoInfo">视频信息对象</param>
        public void SelectedVideo(VideoInfo videoInfo)
        {
            Model.SelectedVideoAction?.Invoke(videoInfo);
        }
    }
}
