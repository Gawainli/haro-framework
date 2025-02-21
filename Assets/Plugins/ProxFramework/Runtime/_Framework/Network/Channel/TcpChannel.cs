using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;


namespace ProxFramework.Network
{
    public class TcpChannel : INetChannel, IDisposable
    {
        public bool Connected => socket.Connected;

        private readonly Queue<INetPackage> sendQueue = new Queue<INetPackage>();
        private readonly Queue<INetPackage> recvQueue = new Queue<INetPackage>();
        private readonly List<INetPackage> decodeTempList = new List<INetPackage>();

        private byte[] recvBuffer = new byte[DefaultNetPackage.PkgMaxSize];
        private RingBuffer encodeBuffer = new RingBuffer(DefaultNetPackage.PkgMaxSize * 4);
        private RingBuffer decodeBuffer = new RingBuffer(DefaultNetPackage.PkgMaxSize * 4);

        private INetPackageDecoder decoder;
        private INetPackageEncoder encoder;
        private Socket socket;

        private bool sending;
        private bool recving;


        private float heartBeatInterval = 1f;
        private float heartBeatWaitTime = 0f;
        private int heartBeatMissCount = 0;
        private INetPackage heartBeatPkg = null;

        public TcpChannel(INetPackageEncoder encoder, INetPackageDecoder decoder, Socket socket)
        {
            this.decoder = decoder;
            this.encoder = encoder;
            this.socket = socket;
            heartBeatPkg = new DefaultNetPackage
            {
                MsgId = 1,
                BodyBytes = System.Text.Encoding.UTF8.GetBytes("ping")
            };
        }

        public void Update(float unscaledDeltaTime)
        {
            if (!socket.Connected)
            {
                return;
            }

            ProcessHeartbeat(unscaledDeltaTime);
            ProcessSend();
            ProcessRecv();
        }


        public void SendPkg(INetPackage pkg)
        {
            lock (sendQueue)
            {
                sendQueue.Enqueue(pkg);
            }
        }

        public INetPackage PickPkg()
        {
            INetPackage pkg = null;
            lock (recvQueue)
            {
                if (recvQueue.Count > 0)
                {
                    pkg = recvQueue.Dequeue();
                }
            }

            return pkg;
        }

        private void ProcessHeartbeat(float unscaledDeltaTime)
        {
            heartBeatWaitTime += unscaledDeltaTime;
            if (heartBeatWaitTime > heartBeatInterval)
            {
                SendPkg(heartBeatPkg);
                heartBeatWaitTime = 0;
            }
        }

        private async void ProcessSend()
        {
            if (sending || sendQueue.Count == 0)
            {
                return;
            }

            sending = true;
            while (sendQueue.Count > 0)
            {
                if (encodeBuffer.WriteableBytes < DefaultNetPackage.PkgMaxSize)
                {
                    break;
                }

                var pkg = sendQueue.Dequeue();
                encoder.Encode(encodeBuffer, pkg);
            }

            var sendBytes = encodeBuffer.ReadBytes(encodeBuffer.ReadableBytes);
            await socket.SendAsync(sendBytes, SocketFlags.None);
            sending = false;
        }

        private async void ProcessRecv()
        {
            if (recving)
            {
                return;
            }

            recving = true;
            var recvBytes = await socket.ReceiveAsync(recvBuffer, SocketFlags.None);
            if (recvBytes == 0)
            {
                return;
            }

            if (!decodeBuffer.IsWriteable(recvBytes))
            {
                return;
            }

            decodeBuffer.WriteBytes(recvBuffer, 0, recvBytes);
            decodeTempList.Clear();
            decoder.Decode(decodeBuffer, decodeTempList);
            lock (recvQueue)
            {
                foreach (var pkg in decodeTempList)
                {
                    if (pkg is DefaultNetPackage { MsgId: 1 })
                    {
                        heartBeatWaitTime = 0f;
                        continue;
                    }

                    recvQueue.Enqueue(pkg);
                }
            }

            recving = false;
        }

        public void Dispose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                sendQueue.Clear();
                recvQueue.Clear();
                decodeTempList.Clear();
            }
            catch (Exception e)
            {
                PLogger.Exception("Tcp Close Exception", e);
            }
            finally
            {
                socket.Close();
            }
        }
    }
}