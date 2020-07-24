namespace WebSocket4Net
{
    public partial class WebSocket
    {
        public WebSocket(string uri, string subProtocol)
            : this(uri, subProtocol, string.Empty)
        {

        }

        public WebSocket(string uri, string subProtocol = "", string origin = "", int receiveBufferSize = 0)
        {
            Initialize(uri, subProtocol, origin, receiveBufferSize);
        }
    }
}
