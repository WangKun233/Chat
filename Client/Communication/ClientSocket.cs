﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Client.UIL.Model;

namespace Client.Communication
{
    public class ClientSocket
    {
        private static string svrIP = "127.0.0.1";    //服务器IP
        public static string SvrIP { get => svrIP; set => svrIP = value; }

        private static int svrPort = 8888;     //服务器端口
        public static int SvrPort { get => svrPort; set => svrPort = value; }

        private static Socket connectSvrSocket;
        public static Socket ConnectSvrSocket { get => connectSvrSocket; set => connectSvrSocket = value; }

        /// <summary>
        /// 与服务器建立TCP连接
        /// </summary>
        /// <returns></returns>
        public bool ConnectSvr()
        {
            try
            {
                IPAddress ip = IPAddress.Parse(svrIP);
                IPEndPoint ipe = new IPEndPoint(ip, svrPort);//把ip和端口转化为IPEndpoint实例

                //创建socket并连接到服务器
                connectSvrSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//创建Socket

                /*
                 *如果这三次发送非常紧密时间非常短，会被优化算法优化成一个数据包发送出去，
                 *这属于客户端粘包，可以关闭Nagle算法，那么调用几次send就会发送几次包
                 */

                //ConnectSvrSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);     //关闭Nagle算法，解决客户端沾包
                connectSvrSocket.Connect(ipe);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 发送网络包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="package"></param>
        /// <returns></returns>
        public bool SendData(PackageModel package)
        {
            try
            {
                NetPacket netPacket = new NetPacket();
                byte[] sendBytes = netPacket.Package(package);
                int sendByteNums = connectSvrSocket.Send(sendBytes, sendBytes.Length, 0);//发送信息
                return sendByteNums == sendBytes.Length;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        #region dele
        //public int SendData(UserInfoSignIn userRecvMsg, string sendMsg)
        //{
        //    byte[] bs = Encoding.ASCII.GetBytes(sendMsg);//把字符串编码为字节
        //    return ConnectSvrSocket.Send(bs, bs.Length, 0);//发送信息
        //}
        #endregion

        /// <summary>
        /// 接收网络包
        /// </summary>
        /// <param name="userRecvData"></param>
        public void RecvData(UserInfoSignIn userRecvData)
        {
            try
            {
                //全部接收到缓冲区后，再异步处理
                byte[] recvBufferTemp = new byte[1024];
                int bytesRecv = 0;

                //接收缓冲区中数据全部放到RecvBuffer中
                while ((bytesRecv = userRecvData.ClientConnectSocket.Receive(recvBufferTemp, 0)) > 0)
                {
                    userRecvData.recvBuffer.AddRange(recvBufferTemp.ToList<byte>());
                }

                //拆包
                NetPacket netPacket = new NetPacket();
                PackageModel onePackage;
                while (netPacket.UnPackage(ref userRecvData.recvBuffer, out onePackage))
                {
                    if (userRecvData.recvEvent != null)
                    {
                        userRecvData.recvEvent(this, onePackage);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }




    }
}
