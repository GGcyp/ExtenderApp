using System.IO;
using System.Text;
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
        public TestMainViewModel(IServiceStore serviceStore) : base(serviceStore)
        {
            var info = CreatTestExpectLocalFileInfo("text");
            LogInformation("开始测试");

            SnmpPdu pdu = new(SnmpPduType.GetRequest, 10);
            pdu.AddVarBind(new(SnmpOid.SysDescr, 50));
            //pdu.AddVarBind(new(SnmpOid.SysUpTime, "asdads"));
            SnmpMessage message = new(pdu);
            ByteBlock block = new ByteBlock();
            message.Encode(ref block);
            LogInformation($"SNMP 消息编码结果：{block.Length} 字节");

        }

        private ExpectLocalFileInfo CreatTestExpectLocalFileInfo(string fileName)
        {
            return new ExpectLocalFileInfo(ProgramDirectory.ChekAndCreateFolder("test"), fileName);
        }
    }
}