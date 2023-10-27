using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ErXBoutCode.Network
{
    public class UdpMessageArgs
    {
        /// <summary>
        /// Describes that the message was sent from the client itself
        /// </summary>
        public bool IsLoopback { get; set; }

        public bool MessageHandled { get; set; }
    }

    public class ErXUdpClient
    {
        public UdpClient baseClient { get; set; }

        public int Port { get; private set; }

        private string _lastSentMsg { get; set; }

        private string _lastReceivedMsg { get; set; }

        #region Events
        /// <summary>
        /// Event Handler für das GotMessage Event
        /// </summary>
        /// <param name="message">Aufgefangene Nachricht</param>
        /// <param name="remoteEndPoint">Absender der Nachricht</param>
        public delegate void GotMessageEventHandler(string message, IPEndPoint remoteEndPoint, UdpMessageArgs e = null);
        /// <summary>
        /// Eine neue Nachricht von <paramref name="remoteEndPoint"/> ist eingetroffen
        /// </summary>
        public event GotMessageEventHandler MessageReceived;

        /// <summary>
        /// Eine neue Nachricht von <paramref name="remoteEndPoint"/> ist eingetroffen
        /// </summary>
        public event GotMessageEventHandler BeforeMessageReceived;
        #endregion

        #region Listening
        private Task Listen()
        {
            return Task.Run(() =>
            {
                if (baseClient != null)
                {
                    while (true)
                    {
                        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, Port);

                        string message = null;
                        byte[] messageAsByteArray = baseClient.Receive(ref remoteEndPoint);

                        try
                        {
                            message = Encoding.ASCII.GetString(messageAsByteArray, 0, messageAsByteArray.Length);

                            UdpMessageArgs args = new UdpMessageArgs
                            {
                                IsLoopback = message == _lastSentMsg
                            };

                            BeforeMessageReceived?.Invoke(message, remoteEndPoint, args);

                            if (!args.MessageHandled)
                                MessageReceived?.Invoke(message, remoteEndPoint);

                            _lastReceivedMsg = message;
                        }
                        catch
                        {
                            // EndReceive failed and we ended up here
                        }
                    }
                }
            });
        }
        #endregion

        #region Senden
        /// <summary>
        /// Macht einen UDP broadcast über alle verfügbaren Netzwerke auf den Port aus der Eigenschaft Port
        /// </summary>
        /// <param name="toBroadcast">Nachricht die versendet werden soll</param>
        public void broadcast(string toBroadcast)
        {
            send(toBroadcast, new IPEndPoint(IPAddress.Broadcast, Port));
        }
        /// <summary>
        /// Sendet einen string über Udp Protokoll zu dem gegebenen Ziel
        /// </summary>
        /// <param name="toSend">Der string der den zu sendenden Text beinhaltet</param>
        /// <param name="ziel">Der RemoteEndPoint des Zieles zusammengesetzt aus Ziel-Ip und Ziel-port</param>
        public void send(string toSend, IPEndPoint ziel)
        {
            _lastSentMsg = toSend;
            baseClient.EnableBroadcast = true;
            baseClient.Send(Encoding.ASCII.GetBytes(toSend), Encoding.ASCII.GetByteCount(toSend), ziel);
        }
        #endregion

        public ErXUdpClient(int port)
        {
            Port = port;

            baseClient = new UdpClient(Port);
            Listen();
        }

        public void Dispose()
        {

        }
    }
}
