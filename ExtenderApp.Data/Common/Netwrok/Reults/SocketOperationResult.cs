using System.Net;
using System.Net.Sockets;

namespace ExtenderApp.Data
{
    /// <summary>
    /// ��ʾһ���׽��ֲ���������/����/������/��������Ϣ���յȣ��Ľ�����ݡ�
    /// </summary>
    /// <remarks>
    /// �ýṹͨ���ɵײ� Socket/SocketAsyncEventArgs �ص��� awaitable ��װ�ڲ������ʱ��䣬
    /// �������ϲ㴫�ݱ��β�����״̬�����ݳ����Լ���ѡ��Զ����Ϣ�� IP �㱨����Ϣ��
    /// </remarks>
    public struct SocketOperationResult
    {
        /// <summary>
        /// ʵ�ʴ�����ֽ�����
        /// ���ڽ��ղ�����ֵΪ 0 ͨ����ʾ�Զ������Źر����ӡ�
        /// </summary>
        public int BytesTransferred;

        /// <summary>
        /// Զ���ս����Ϣ��
        /// ������ҪԶ�˵�ַ�Ĳ������� ReceiveFrom/ReceiveMessageFrom������Ч��������������Ϊ�ա�
        /// </summary>
        public EndPoint? RemoteEndPoint;

        /// <summary>
        /// ���β����������׽����쳣��
        /// �ɹ�ʱӦΪ <c>null</c>��ʧ��ʱΪ����� <see cref="SocketException"/>��
        /// </summary>
        public SocketException? SocketError;

        /// <summary>
        /// ��� <c>ReceiveMessageFrom</c> ������ IP �㱨����Ϣ���籾�� IP���ӿڵȣ���
        /// ���ڸ���������ʱ��Ч�����������ɺ��Ը��ֶΡ�
        /// </summary>
        public IPPacketInformation ReceiveMessageFromPacketInfo;

        /// <summary>
        /// ��ǰ����ֵ�Ľ��״̬�롣
        /// </summary>
        public ResultCode Code => SocketError == null ? ResultCode.Success : ResultCode.Failed;

        public SocketOperationResult(SocketException? socketError) : this(0, null, socketError, default)
        {
        }

        public SocketOperationResult(int bytesTransferred, EndPoint? remoteEndPoint, SocketException? socketError, IPPacketInformation receiveMessageFromPacketInfo)
        {
            BytesTransferred = bytesTransferred;
            RemoteEndPoint = remoteEndPoint;
            SocketError = socketError;
            ReceiveMessageFromPacketInfo = receiveMessageFromPacketInfo;
        }

        public static implicit operator Result(SocketOperationResult result)
            => new Result(result.Code, result.SocketError?.Message);
    }
}