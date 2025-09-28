using System.Buffers;
using ExtenderApp.Abstract;
using ExtenderApp.Media.Models;
using ExtenderApp.ViewModels;
using Microsoft.Win32;
using SoundTouch;

namespace ExtenderApp.Media.ViewModels
{
    public class VideoViewModel : ExtenderAppViewModel<VideoView, MediaModel>
    {
        private readonly MediaEngine _engine;

        public VideoViewModel(IServiceStore serviceStore, MediaEngine engine) : base(serviceStore)
        {
            if (!Environment.Is64BitProcess)
            {
                Error("仅支持64位系统", new Exception());
            }

            _engine = engine;

            // 创建文件选择对话框实例
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // 设置文件筛选器，仅显示 .torrent 文件
            openFileDialog.Filter = "MP4 文件 (*.mp4)|*.mp4";
            // 打开文件选择对话框并获取用户选择结果
            bool? result = openFileDialog.ShowDialog();

            string filePath = openFileDialog.FileName;
            OpenVideo(filePath);
        }

        public void OpenVideo(string filePath)
        {
            var player = _engine.OpenMedia(filePath);
            Model.SetPlayer(player);
            Model.Volume = 0.5f;
            player.OnVideoFrame += f =>
            {
                DispatcherInvoke(() =>
                {
                    Model.Bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, f.Width, f.Height), f.Data, f.Stride, 0);
                });
            };

            var _soundTouch = new SoundTouchProcessor();
            _soundTouch.SampleRate = 44100;
            _soundTouch.Channels = 2;
            _soundTouch.Tempo = 0.5f * 100;           // 保持原速

            //player.OnAudioFrame += f =>
            //{
            //    try
            //    {
            //        float[] floatSamples = ConvertPcm16BytesToFloat(f.Data);

            //        int numSamples = floatSamples.Length / _soundTouch.Channels;
            //        _soundTouch.PutSamples(floatSamples, numSamples);

            //        // 获取可输出样本数
            //        int available = _soundTouch.AvailableSamples;
            //        if (available > 0)
            //        {
            //            float[] processed = ArrayPool<float>.Shared.Rent(available * _soundTouch.Channels);
            //            int samplesProcessed = _soundTouch.ReceiveSamples(processed, available);

            //            // 将 float 数组转换回 byte 数组
            //            int length = samplesProcessed * _soundTouch.Channels * 2;
            //            byte[] processedBytes = ArrayPool<byte>.Shared.Rent(length);
            //            for (int i = 0; i < samplesProcessed * _soundTouch.Channels; i++)
            //            {
            //                short s = (short)(Math.Clamp(processed[i], -1f, 1f) * 32767);
            //                processedBytes[i * 2] = (byte)(s & 0xFF);
            //                processedBytes[i * 2 + 1] = (byte)((s >> 8) & 0xFF);
            //            }

            //            ArrayPool<float>.Shared.Return(processed);
            //            Model.BufferedWave?.AddSamples(processedBytes, 0, length);
            //            ArrayPool<byte>.Shared.Return(processedBytes);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Model.BufferedWave?.ClearBuffer();
            //        Error("音频播放出错，已清空缓冲区", ex);
            //    }
            //};

            //Model.WaveOut!.Play();
            player.Play();
            player.RateSpeed = 1.5f;
        }

        public static float[] ConvertPcm16BytesToFloat(byte[] pcmBytes)
        {
            int samples = pcmBytes.Length / 2;
            float[] floatSamples = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(pcmBytes, i * 2);
                floatSamples[i] = sample / 32768f; // 归一化到[-1,1]
            }
            return floatSamples;
        }
    }
}