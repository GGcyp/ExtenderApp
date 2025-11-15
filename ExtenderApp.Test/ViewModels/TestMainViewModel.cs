using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Windows.Documents;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Encodings;
using ExtenderApp.Common.Networks;
using ExtenderApp.Common.Networks.SNMP;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;
using Microsoft.Extensions.Logging;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private UdpClient _udpClient;

        public TestMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");

        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}