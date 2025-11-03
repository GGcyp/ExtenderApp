
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.FlieSegmenters.FileResponseDtos;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Common.Networks
{
    internal static class FileSegmenterExtensions
    {
        public static IServiceCollection AddFileSegmenter(this IServiceCollection services)
        {
            services.AddSingleton<FileSegmenter>();

            services.ConfigureSingletonInstance<IBinaryFormatterStore>(s =>
            {
                s.Add<FileInfoDto, FileInfoDtoFormatter>();
                s.Add<FileTransferRequestDto, FileTransferRequestDtoFormatter>();
                s.Add<FileTransferConfigDto, FileTransferConfigDtoFormatter>();
                s.Add<FileSplitterInfoRequestDto, FileSplitterInfoRequestDtoFormatter>();
            });

            return services;
        }
    }
}
