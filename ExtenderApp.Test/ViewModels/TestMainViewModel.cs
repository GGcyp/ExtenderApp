using System.Diagnostics;
using System.Text;
using ExtenderApp.Abstract;
using ExtenderApp.Abstract.Networks;
using ExtenderApp.Abstract.Options;
using ExtenderApp.Contracts;
using ExtenderApp.Test.Tests;
using ExtenderApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private readonly ILZ4Compression lZ4Compression;
        private readonly IBinarySerialization binarySerialization;

        public TestMainViewModel(ILZ4Compression lZ4Compression, IBinarySerialization binarySerialization)
        {
            this.lZ4Compression = lZ4Compression;
            this.binarySerialization = binarySerialization;
        }

        public override void Inject(IServiceProvider serviceProvider)
        {
            base.Inject(serviceProvider);

            //AbstractBufferStreamTests.RunAll();

            LinkClinentTest.TestLinkClientHandlerAsync(serviceProvider).Wait();
            //LinkerTests.RunAll(serviceProvider.GetRequiredService<ILinkerFactory<ITcpLinker>>(), binarySerialization);

            // 运行 HTTP 请求序列化测试
            //TestHttpRequestSerialization();

            // 运行自包含的测试用例，检查序列化及内存回收/冻结相关行为
            //SerializationTests.RunAll(binarySerialization);
            //var factory = serviceProvider.GetRequiredService<ILinkerFactory<ITcpLinker>>();
            //Task.Run(() =>
            //{
            //    LinkerTests.RunAll(factory, binarySerialization);
            //});
            //AwaitableEventArgsTests.RunAll();
            //IOptionsTests.RunAll();
            //LinkClientPipelineTests.RunAll();
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }

        // 新增：测试 HttpRequestMessage 序列化是否包含关键部分（请求行和必要头）
        private void TestHttpRequestSerialization()
        {
            try
            {
                // 准备 GET 请求并添加查询参数
                var getReq = new HttpRequestMessage(HttpMethod.Get, "http://example.com/path");
                var getParams = new HttpParameters();
                getParams.RegisterOption(new OptionIdentifier<string>("a"), "1");
                getParams.RegisterOption(new OptionIdentifier<string>("b"), "two words");
                getReq.Params = getParams;

                string getHeader = getReq.GetRequestString();

                bool getPass = getHeader.Contains("GET ") &&
                               getHeader.Contains("/path") &&
                               (getHeader.Contains("a=1") || getHeader.Contains("a%3D1")) &&
                               getHeader.Contains("HTTP/") &&
                               getHeader.Contains(HttpRequestOptions.SchemeOption.Name.ToString()) == false; // just ensure no accidental debug

                // Host 应当被补齐
                bool hasHost = getHeader.IndexOf("Host:", StringComparison.OrdinalIgnoreCase) >= 0;

                Debug.Print(getHeader);
                Debug.Print($"[测试] GET 请求序列化: {(getPass && hasHost ? "通过" : "失败")}");

                // 准备 POST 请求：将 Params 转为表单并写入 Body
                var postReq = new HttpRequestMessage(HttpMethod.Post, "http://example.com/submit");
                var postParams = new HttpParameters();
                postParams.RegisterOption(new OptionIdentifier<string>("x"), "alpha");
                postParams.RegisterOption(new OptionIdentifier<string>("y"), "β test");
                postReq.Params = postParams;
                postReq.SetFormContentFromParams(Encoding.UTF8);

                string postHeader = postReq.GetRequestString();
                Debug.Print(postHeader);

                bool postHasContentType = postHeader.IndexOf("Content-Type:", StringComparison.OrdinalIgnoreCase) >= 0 &&
                                          postHeader.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0;

                bool postHasContentLength = postHeader.IndexOf("Content-Committed:", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                            postHeader.IndexOf("Content-Length:", StringComparison.OrdinalIgnoreCase) >= 0;

                Debug.Print($"[测试] POST 请求头包含 Content-Type: {(postHasContentType ? "是" : "否")}, 包含 Content-Length: {(postHasContentLength ? "是" : "否")}");

                // 进一步检查 POST 请求体写入到缓冲
                var seq = postReq.GetRequestBuffer(Encoding.ASCII, combineValues: false);
                try
                {
                    // 将序列缓冲转换为字符串用于检查（头部为 ASCII，body 为 UTF-8）
                    var bytes = seq.ToArray();
                    string asAscii = Encoding.ASCII.GetString(bytes);
                    bool containsForm = asAscii.IndexOf("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                        asAscii.IndexOf("x=alpha", StringComparison.OrdinalIgnoreCase) >= 0;
                    Debug.Print($"[测试] POST 请求缓冲包含表单内容: {(containsForm ? "是" : "否")}");
                }
                finally
                {
                    if (!seq.TryRelease())
                    {
                        try { seq.UnfreezeWrite(); } catch { }
                        try { seq.Unfreeze(); } catch { }
                        seq.TryRelease();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"[错误] HttpRequestMessage 序列化测试抛出异常: {ex}");
            }
        }
    }
}