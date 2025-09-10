using FFmpeg.AutoGen;

namespace ExtenderApp.Media.FFmpegEngines
{
    public unsafe class FFmpegEngine
    {
        private const int DefaultStreamIndex = -1;
        private const double DefaultFrameRate = 25.0;
        private readonly HashSet<nint> _intPtrHashSet;

        public string FFmpegVersion => ffmpeg.av_version_info();
        public string FFmpegPath => ffmpeg.RootPath;

        internal FFmpegEngine(string ffmpegPath)
        {
            _intPtrHashSet = new();
            ffmpeg.RootPath = ffmpegPath;
            ffmpeg.avformat_network_init();
        }

        public FFmpegContext OpenUri(string uri, FFmpegInputFormat inputFormat, Dictionary<string, string> options)
        {
            var formatContext = CreateFormatContext();
            var optionsIntPtr = CreateOptions(options);
            int result = ffmpeg.avformat_open_input(formatContext, uri, inputFormat, optionsIntPtr);

            if (result < 0)
            {
                throw new FFmpegException($"未找到指定uri：{uri}");
            }

            result = ffmpeg.avformat_find_stream_info(formatContext, optionsIntPtr);
            if (result < 0)
            {
                throw new FFmpegException($"无法获取流信息:{uri}");
            }

            if (!TryGetStreamIndex(formatContext, out int videoIndex, out int audioIndex, out int subtitleIndex))
            {
                throw new FFmpegException($"未找到视频或音频流:{uri}");
            }

            bool hasVideoDecoder = TryGetDecoder(formatContext, videoIndex, optionsIntPtr, out FFmpegDecoderContext videoDecoder);
            bool hasAudioDecoder = TryGetDecoder(formatContext, audioIndex, optionsIntPtr, out FFmpegDecoderContext audioDecoder);

            if (!hasVideoDecoder && !hasAudioDecoder)
            {
                throw new FFmpegException($"未找到可用的视频或音频解码器:{uri}");
            }

            var info = CreateFFmpegInfo(uri, videoDecoder, audioDecoder);

            return new FFmpegContext(this, formatContext, optionsIntPtr, info, videoDecoder, audioDecoder);
        }

        public string MediaTypeToString(AVMediaType mediaType)
        {
            return ffmpeg.av_get_media_type_string(mediaType);
        }

        #region Info

        private FFmpegInfo CreateFFmpegInfo(string uri, FFmpegDecoderContext videoContext, FFmpegDecoderContext audioContext)
        {
            FFmpegInfo info = new(uri, GetCodecNameOrDefault(videoContext), GetCodecNameOrDefault(audioContext));
            SetInfo(ref info, videoContext);
            SetInfo(ref info, audioContext);
            return info;
        }

        private void SetInfo(ref FFmpegInfo info, FFmpegDecoderContext context)
        {
            if (context.IsEmpty)
            {
                return;
            }
            int width = context.CodecParameters.Value->width;
            int height = context.CodecParameters.Value->height;
            int sampleRate = context.CodecParameters.Value->sample_rate;
            int channels = context.CodecParameters.Value->ch_layout.nb_channels;
            long duration = context.CodecStream.Value->duration;
            long bitRate = context.CodecContext.Value->bit_rate;

            double frameRate = DefaultFrameRate;
            if (context.CodecContext.Value->framerate.den != 0)
            {
                frameRate = ffmpeg.av_q2d(context.CodecContext.Value->framerate);
            }

            info.SetInfo(width, height, sampleRate, channels, frameRate, frameRate, bitRate);
        }

        #endregion

        #region Create

        public NativeIntPtr<AVFormatContext> CreateFormatContext()
        {
            NativeIntPtr<AVFormatContext> formatContext = ffmpeg.avformat_alloc_context();
            _intPtrHashSet.Add(formatContext);
            return formatContext;
        }

        public NativeIntPtr<AVDictionary> CreateOptions(Dictionary<string, string> options)
        {
            AVDictionary* dict = null;
            if (options != null)
            {
                foreach (var option in options)
                {
                    ffmpeg.av_dict_set(&dict, option.Key, option.Value, 0);
                }
            }
            return new(dict);
        }

        #endregion

        #region Get

        private bool TryGetVideoStreamIndex(AVFormatContext* formatContext, out int videoIndex)
        {
            videoIndex = GetStreamIndex(formatContext, AVMediaType.AVMEDIA_TYPE_VIDEO);
            return videoIndex != DefaultStreamIndex;
        }

        private bool TryGetAudioStreamIndex(AVFormatContext* formatContext, out int audioIndex)
        {
            audioIndex = GetStreamIndex(formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO);
            return audioIndex != DefaultStreamIndex;
        }

        private bool TryGetSubtitleStreamIndex(AVFormatContext* formatContext, out int subtitleIndex)
        {
            subtitleIndex = GetStreamIndex(formatContext, AVMediaType.AVMEDIA_TYPE_SUBTITLE);
            return subtitleIndex != DefaultStreamIndex;
        }

        private int GetStreamIndex(AVFormatContext* formatContext, AVMediaType mediaType)
        {
            uint count = formatContext->nb_streams;
            for (int i = 0; i < count; i++)
            {
                AVStream* stream = formatContext->streams[i];
                AVMediaType codecType = stream->codecpar->codec_type;
                if (codecType == mediaType)
                {
                    return i;
                }
            }
            return DefaultStreamIndex;
        }

        private bool TryGetStreamIndex(AVFormatContext* formatContext, out int videoIndex, out int audioIndex, out int subtitleIndex)
        {
            videoIndex = DefaultStreamIndex;
            audioIndex = DefaultStreamIndex;
            subtitleIndex = DefaultStreamIndex;
            uint count = formatContext->nb_streams;
            for (int i = 0; i < count; i++)
            {
                AVStream* stream = formatContext->streams[i];
                AVMediaType codecType = stream->codecpar->codec_type;

                switch (codecType)
                {
                    case AVMediaType.AVMEDIA_TYPE_VIDEO:
                        if (videoIndex == DefaultStreamIndex)
                            videoIndex = i;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_AUDIO:
                        if (audioIndex == DefaultStreamIndex)
                            audioIndex = i;
                        break;
                    case AVMediaType.AVMEDIA_TYPE_SUBTITLE:
                        if (subtitleIndex == DefaultStreamIndex)
                            subtitleIndex = i;
                        break;
                }

                // 如果已经找到视频和音频流，提前退出
                if (videoIndex != DefaultStreamIndex && audioIndex != DefaultStreamIndex && subtitleIndex != DefaultStreamIndex)
                    break;
            }

            return videoIndex != DefaultStreamIndex || audioIndex != DefaultStreamIndex;
        }

        #endregion

        #region Format

        public FFmpegInputFormat FindInputFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var input = ffmpeg.av_find_input_format(name);
            NativeIntPtr<AVInputFormat> ptr = new(input);
            return new FFmpegInputFormat(ptr);
        }

        public FFmpegOutputFormat FindOutputFormat(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            var output = ffmpeg.av_guess_format(name, null, null);
            NativeIntPtr<AVOutputFormat> ptr = new(output);
            return new FFmpegOutputFormat(ptr);
        }

        #endregion

        #region Codec

        private bool TryGetDecoder(AVFormatContext* formatContext, int streamIndex, AVDictionary** options, out FFmpegDecoderContext decoder)
        {
            decoder = FFmpegDecoderContext.Empty;
            NativeIntPtr<AVCodec> codec = NativeIntPtr<AVCodec>.Empty;
            NativeIntPtr<AVCodecParameters> codecParameters = NativeIntPtr<AVCodecParameters>.Empty;
            NativeIntPtr<AVCodecContext> codecContext = NativeIntPtr<AVCodecContext>.Empty;
            NativeIntPtr<AVStream> codecStream = NativeIntPtr<AVStream>.Empty;
            if (CheckStreamIndex(streamIndex))
            {
                return false;
            }

            codecStream = formatContext->streams[streamIndex];
            codecParameters = codecStream.Value->codecpar;
            codec = ffmpeg.avcodec_find_decoder(codecParameters.Value->codec_id);
            if (codec.IsEmpty)
            {
                //throw new FFmpegException($"未找到解码器: {codecParameters.Value->codec_id}");
                return false;
            }

            codecContext = ffmpeg.avcodec_alloc_context3(codec);
            if (codecContext.IsEmpty)
            {
                //throw new FFmpegException("无法分配解码器上下文");
                return false;
            }

            //复制流参数到解码器上下文
            int result = ffmpeg.avcodec_parameters_to_context(codecContext, codecParameters);
            if (result < 0)
            {
                //throw new FFmpegException("无法将参数复制到解码器上下文");
                return false;
            }

            //打开解码器
            result = ffmpeg.avcodec_open2(codecContext, codec, options);
            if (result < 0)
            {
                //throw new FFmpegException("无法打开解码器");
                return false;
            }
            _intPtrHashSet.Add(codecContext);
            _intPtrHashSet.Add(codecParameters);
            _intPtrHashSet.Add(codecStream);
            //不需要手动释放
            //_intPtrHashSet.Add(codec);
            decoder = new FFmpegDecoderContext(codec, codecParameters, codecContext, codecStream, streamIndex);
            return true;
        }

        public string GetCodecNameOrDefault(FFmpegDecoderContext decoder)
        {
            string result = "未找到";
            if (decoder.IsEmpty)
            {
                return result;
            }
            string temp = ffmpeg.avcodec_get_name(decoder.Codec.Value->id);
            return string.IsNullOrEmpty(temp) ? result : temp;
        }

        public string GetCodecName(FFmpegDecoderContext decoder)
        {
            if (decoder.IsEmpty)
            {
                return string.Empty;
            }
            return ffmpeg.avcodec_get_name(decoder.Codec.Value->id);
        }

        public string GetCodecName(AVCodecID codecId)
        {
            var codec = ffmpeg.avcodec_find_decoder(codecId);
            if (codec == null)
            {
                return string.Empty;
            }
            return ffmpeg.avcodec_get_name(codecId);
        }

        private bool CheckStreamIndex(int index)
        {
            return index != DefaultStreamIndex && index >= DefaultStreamIndex;
        }

        #endregion

        #region Free

        public void FreeDecoder(ref FFmpegDecoderContext decoder)
        {
            if (decoder.IsEmpty)
            {
                return;
            }

            FreeCodecContext(ref decoder.CodecContext);
            FreeCodecParameters(ref decoder.CodecParameters);
            FreeStream(ref decoder.CodecStream);
        }

        public void FreeCodecContext(ref NativeIntPtr<AVCodecContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.avcodec_free_context(ptr);
                ptr.Dispose();
            }
        }

        public void FreeCodecParameters(ref NativeIntPtr<AVCodecParameters> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.avcodec_parameters_free(ptr);
                ptr.Dispose();
            }
        }

        public void FreeStream(ref NativeIntPtr<AVStream> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                var stream = (void*)ptr;
                ffmpeg.av_free(stream);
                ptr.Dispose();
            }
        }

        public void FreeOptions(ref NativeIntPtr<AVDictionary> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.av_dict_free(ptr);
                ptr.Dispose();
            }
        }

        public void FreeFormatContext(ref NativeIntPtr<AVFormatContext> ptr)
        {
            if (CheckIntPtrAndRemove(ptr))
            {
                ffmpeg.avformat_free_context(ptr);
                ptr.Dispose();
            }
        }

        private bool CheckIntPtrAndRemove<T>(NativeIntPtr<T> ptr) where T : unmanaged
        {
            if (ptr.IsEmpty || !_intPtrHashSet.Contains(ptr))
            {
                return false;
            }
            _intPtrHashSet.Remove(ptr);
            return true;
        }

        #endregion
    }
}
