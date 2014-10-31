namespace LandonKey.SocksWebProxy
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;

    using LandonKey.SocksWebProxy.Proxy;

    public class SocksWebProxy : IWebProxy
    {
        #region Static Fields

        private static readonly object Locker = new object();

        private static List<ProxyListener> listeners;

        #endregion

        #region Constructors and Destructors

        public SocksWebProxy(ProxyConfig config = null)
        {
            Config = config;
            GetListener(config);
        }

        #endregion

        #region Public Properties

        public ICredentials Credentials { get; set; }

        #endregion

        #region Properties

        private ProxyConfig Config { get; set; }

        #endregion

        #region Public Methods and Operators

        public Uri GetProxy(Uri destination)
        {
            return new Uri("http://127.0.0.1:" + Config.HttpPort);
        }

        public bool IsActive()
        {
            var isSocksPortListening =
                IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(x => x.Port == Config.SocksPort);
            return isSocksPortListening;
        }

        public bool IsBypassed(Uri host)
        {
            return !IsActive();
        }

        #endregion

        #region Methods

        private static void GetListener(ProxyConfig config)
        {
            lock (Locker)
            {
                if (listeners == null)
                {
                    listeners = new List<ProxyListener>();
                }

                var listener = listeners.FirstOrDefault(x => x.Port == config.HttpPort);

                if (listener == null)
                {
                    listener = new ProxyListener(config);
                    listener.Start();
                    listeners.Add(listener);
                }

                if (listener.Version != config.Version)
                {
                    throw new Exception("Socks Version Mismatch for Port " + config.HttpPort);
                }
            }
        }

        #endregion
    }
}