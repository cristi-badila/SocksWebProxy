namespace LandonKey.SocksWebProxy.Proxy
{
    using System;

    using Org.Mentalis.Proxy.Http;

    public sealed class ProxyListener : HttpListener
    {
        #region Constructors and Destructors

        public ProxyListener(ProxyConfig config)
            : base(config.HttpAddress, config.HttpPort)
        {
            Port = config.HttpPort;
            Version = config.Version;
            Config = config;
        }

        #endregion

        #region Public Properties

        public new int Port { get; private set; }

        public ProxyConfig.SocksVersion Version { get; private set; }

        #endregion

        #region Properties

        private ProxyConfig Config { get; set; }

        #endregion

        #region Public Methods and Operators

        public override void OnAccept(IAsyncResult ar)
        {
            try
            {
                var newSocket = ListenSocket.EndAccept(ar);
                if (newSocket != null)
                {
                    var newClient = new ProxyClient(Config, newSocket, RemoveClient);
                    AddClient(newClient);
                    newClient.StartHandshake();
                }
            }
            catch
            {
            }
            try
            {
                //Restart Listening
                ListenSocket.BeginAccept(OnAccept, ListenSocket);
            }
            catch
            {
                Dispose();
            }
        }

        #endregion
    }
}