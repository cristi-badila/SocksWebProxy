namespace LandonKey.SocksWebProxy.Proxy
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    using Org.Mentalis.Network.ProxySocket;
    using Org.Mentalis.Proxy;
    using Org.Mentalis.Proxy.Http;

    public sealed class ProxyClient : HttpClient
    {
        #region Constructors and Destructors

        public ProxyClient(ProxyConfig config, Socket clientSocket, DestroyDelegate destroyer)
            : base(clientSocket, destroyer)
        {
            Config = config;
        }

        #endregion

        #region Properties

        private ProxyConfig Config { get; set; }

        #endregion

        #region Methods

        protected override void ProcessQuery(string query)
        {
            HeaderFields = ParseQuery(query);
            if (HeaderFields == null || !HeaderFields.ContainsKey("Host"))
            {
                SendBadRequest();
                return;
            }
            int port;
            string host;
            int ret;
            if (HttpRequestType.ToUpper().Equals("CONNECT"))
            {
                //HTTPS
                ret = RequestedPath.IndexOf(":", StringComparison.Ordinal);
                if (ret >= 0)
                {
                    host = RequestedPath.Substring(0, ret);
                    port = RequestedPath.Length > ret + 1 ? int.Parse(RequestedPath.Substring(ret + 1)) : 443;
                }
                else
                {
                    host = RequestedPath;
                    port = 443;
                }
            }
            else
            {
                //Normal HTTP
                ret = HeaderFields["Host"].IndexOf(":", StringComparison.Ordinal);
                if (ret > 0)
                {
                    host = HeaderFields["Host"].Substring(0, ret);
                    port = int.Parse(HeaderFields["Host"].Substring(ret + 1));
                }
                else
                {
                    host = HeaderFields["Host"];
                    port = 80;
                }
                if (HttpRequestType.ToUpper().Equals("POST"))
                {
                    var index = query.IndexOf("\r\n\r\n", StringComparison.Ordinal);
                    m_HttpPost = query.Substring(index + 4);
                }
            }
            try
            {
                DestinationSocket = new ProxySocket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ((ProxySocket)DestinationSocket).ProxyEndPoint = new IPEndPoint(Config.SocksAddress, Config.SocksPort);
                ((ProxySocket)DestinationSocket).ProxyUser = "username";
                ((ProxySocket)DestinationSocket).ProxyPass = "password";
                ((ProxySocket)DestinationSocket).ProxyType = Config.ProxyType;

                if (HeaderFields.ContainsKey("Proxy-Connection")
                    && HeaderFields["Proxy-Connection"].ToLower().Equals("keep-alive"))
                {
                    DestinationSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                }
                ((ProxySocket)DestinationSocket).BeginConnect(host, port, OnProxyConnected, DestinationSocket);
            }
            catch
            {
                SendBadRequest();
            }
        }

        private void OnProxyConnected(IAsyncResult ar)
        {
            try
            {
                ((ProxySocket)DestinationSocket).EndConnect(ar);
                string rq;
                if (HttpRequestType.ToUpper().Equals("CONNECT"))
                {
                    //HTTPS
                    rq = HttpVersion + " 200 Connection established\r\nProxy-Agent: SocksWebProxy\r\n\r\n";
                    ClientSocket.BeginSend(
                        Encoding.ASCII.GetBytes(rq),
                        0,
                        rq.Length,
                        SocketFlags.None,
                        OnOkSent,
                        ClientSocket);
                }
                else
                {
                    //Normal HTTP
                    rq = RebuildQuery();
                    DestinationSocket.BeginSend(
                        Encoding.ASCII.GetBytes(rq),
                        0,
                        rq.Length,
                        SocketFlags.None,
                        OnQuerySent,
                        DestinationSocket);
                }
            }
            catch
            {
                Dispose();
            }
        }

        #endregion
    }
}