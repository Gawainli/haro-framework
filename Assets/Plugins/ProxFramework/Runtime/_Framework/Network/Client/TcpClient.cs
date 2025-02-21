using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

using UnityEngine;

namespace ProxFramework.Network
{
    public class TcpClient : IDisposable
    {
        private TcpChannel channel;
        private readonly INetPackageEncoder encoder;
        private readonly INetPackageDecoder decoder;
        
        private float heartBeatInterval = 1f;
        private float heartBeatWaitTime = 0f;
        private INetPackage heartBeatPkg = null;

        public TcpClient(INetPackageEncoder encoder, INetPackageDecoder decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder;

        }
        
        public void Send(INetPackage pkg)
        {
            channel?.SendPkg(pkg);
        }

        public INetPackage PickPkg()
        {
            return channel?.PickPkg();
        }

        public bool Connected()
        {
            return channel is { Connected: true };
        }

        public async Task ConnectAsync(IPAddress ipAddress, int port)
        {
            try
            {
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(ipAddress, port);
                channel = new TcpChannel(encoder, decoder, socket);
            }
            catch (Exception e)
            {
                PLogger.Exception("Tcp Connect Exception:", e);
            }
        }

        public void Update(float unscaledDeltaTime)
        {
            channel?.Update(unscaledDeltaTime);
            heartBeatWaitTime += unscaledDeltaTime;
            if (heartBeatWaitTime > heartBeatInterval)
            {
                channel?.SendPkg(heartBeatPkg);
            }
        }

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}