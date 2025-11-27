using System.Net.NetworkInformation;
using System.Net.Sockets;
using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private UdpClient _udpClient;

        public TestMainViewModel(IServiceStore serviceStore, IHttpLinkClient http) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");
            Task.Run(async () =>
            {
                var result = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://www.baidu.com/"));
                LogDebug("测试结果：" + result.StatusCode);
            });
        }


        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}