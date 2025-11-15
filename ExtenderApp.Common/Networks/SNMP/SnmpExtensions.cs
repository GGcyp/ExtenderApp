using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Common.Networks.SNMP
{
    public static class SnmpExtensions
    {
        public static bool IsErrorResponse(this SnmpPdu pdu)
        {
            return pdu.ErrorStatus != SnmpErrorStatus.NoError;
        }

        public static string GetErrorMessage(this SnmpPdu pdu)
        {
            return pdu.ErrorStatus switch
            {
                SnmpErrorStatus.NoError => "No error.",
                SnmpErrorStatus.TooBig => "The response message would have been too large to transport.",
                SnmpErrorStatus.NoSuchName => "The requested OID does not exist.",
                SnmpErrorStatus.BadValue => "The value provided is not acceptable.",
                SnmpErrorStatus.ReadOnly => "The variable is read-only and cannot be modified.",
                SnmpErrorStatus.GenErr => "A general error occurred.",
                _ => "Unknown error."
            };
        }

        public static void AddVarBind(this SnmpPdu pdu, SnmpVarBind varBind)
        {
            if (pdu.IsEmpty)
                throw new ArgumentNullException(nameof(pdu), "PDU不能为空");

            pdu.VarBinds.Add(varBind);
        }
    }
}
