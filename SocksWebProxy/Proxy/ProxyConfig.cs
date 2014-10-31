namespace LandonKey.SocksWebProxy.Proxy
{
    using System.Net;

    using Org.Mentalis.Network.ProxySocket;

    public class ProxyConfig
    {
        #region Constructors and Destructors

        public ProxyConfig()
        {
            HttpAddress = IPAddress.Parse("127.0.0.1");
            HttpPort = 12345;
            SocksAddress = IPAddress.Parse("127.0.0.1");
            SocksPort = 9150;
            Version = SocksVersion.Five;
        }

        public ProxyConfig(IPAddress httpIP, int httpPort, IPAddress socksIP, int socksPort, SocksVersion version)
        {
            HttpAddress = httpIP;
            HttpPort = httpPort;
            SocksAddress = socksIP;
            SocksPort = socksPort;
            Version = version;
        }

        #endregion

        #region Enums

        public enum SocksVersion
        {
            Four,

            Five
        }

        #endregion

        #region Public Properties

        public IPAddress HttpAddress { get; set; }

        public int HttpPort { get; set; }

        public ProxyTypes ProxyType
        {
            get
            {
                return (Version == SocksVersion.Five) ? ProxyTypes.Socks5 : ProxyTypes.Socks4;
            }
        }

        public IPAddress SocksAddress { get; set; }

        public int SocksPort { get; set; }

        public SocksVersion Version { get; set; }

        #endregion
    }
}