using System.Security.Authentication;
using SuperSocket.ClientEngine;

namespace WebSocket4Net
{
    public partial class WebSocket
    {
       private SslProtocols m_SecureProtocols = SslProtocols.Tls11 | SslProtocols.Tls12;

        private TcpClientSession CreateSecureTcpSession()
        {
            var client = new SslStreamTcpSession();
            var security = client.Security = new SecurityOption();
            security.EnabledSslProtocols = m_SecureProtocols;
            return client;
        }
    }
}
