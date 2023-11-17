using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Illumine.LPR
{
    public class UDPClient : IDisposable
    {
        private UdpClient udpClient { get; set; }

        public UDPClient(string ip, int port)
        {
            this.udpClient = new UdpClient();
            this.udpClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        public void Send(string Message)
        {
            byte[] bytes = Container.Get<Encoding>().GetBytes(Message);
            this.udpClient.Send(bytes, bytes.Length);
            this.udpClient.Close();
        }

        public void Dispose() => this.udpClient.Close();
    }
}
