namespace WebSocket4Net.Protocol
{
    class Rfc6455HandshakeReader : HandshakeReader
    {
        public Rfc6455HandshakeReader(WebSocket websocket)
            : base(websocket)
        {

        }

        public override WebSocketCommandInfo GetCommandInfo(byte[] readBuffer, int offset, int length, out int left)
        {
            var cmdInfo = base.GetCommandInfo(readBuffer, offset, length, out left);

            if (cmdInfo == null)
                return null;

            //If bad request, NextCommandReader will still be this HandshakeReader
            if (!BadRequestCode.Equals(cmdInfo.Key))
                NextCommandReader = new Rfc6455DataReader();
            
            return cmdInfo;
        }
    }
}
