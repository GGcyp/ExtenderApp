using ExtenderApp.Abstract;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        public TestMainViewModel( IHttpLinkClient http)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");
            Task.Run(async () =>
            {
                var result = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://www.baidu.com/"));
                LogDebug("测试结果：" + result.StatusCode);
            });
            SubscribeMessage<KeyUpEvent>((o, e) => LogInformation("收到按键消息：" + e.Key));
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}