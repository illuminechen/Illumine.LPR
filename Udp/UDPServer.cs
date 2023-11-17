using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class UDPServer
    {
        public EventHandler<LPRArgs> ReceivingCallbak;

        public UDPServer(string ip, int port)
        {
            UDPServer sender = this;
            UdpClient listener = new UdpClient(port);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
            Task.Run((Func<Task>)(() =>
           {
               while (true)
               {
                   string raw;
                   EventHandler<LPRArgs> receivingCallbak;
                   do
                   {
                       raw = Container.Get<Encoding>().GetString(listener.Receive(ref ep));
                       receivingCallbak = sender.ReceivingCallbak;
                   }
                   while (receivingCallbak == null);
                   receivingCallbak((object)sender, new LPRArgs(raw));
               }
           }));
        }
    }
}
