using System;
using System.Buffers.Binary;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ExtenderApp.Abstract;
using ExtenderApp.Common.Networks.SNMP;
using ExtenderApp.Data;
using ExtenderApp.ViewModels;

namespace ExtenderApp.Test
{
    public class TestMainViewModel : ExtenderAppViewModel
    {
        private UdpClient _udpClient;


        public TestMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");
            LogInformation(ProgramDirectory.AppRootPath);
        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}