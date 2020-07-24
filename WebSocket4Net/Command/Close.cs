using WebSocket4Net.Protocol;

namespace WebSocket4Net.Command
{
    public class Close : WebSocketCommandBase
    {
        public static readonly Close Instance = new Close();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            //Close handshake was sent from client side, now got a handshake response
            if (session.State == WebSocketStateConst.Closing)
            {
                session.CloseWithoutHandshake();
                return;
            }

            //Got server side closing handshake request, send response now
            var statusCode = commandInfo.CloseStatusCode;

            if (statusCode <= 0)
                statusCode = (int)CloseStatusCode.NoStatusCode;

            session.Close(statusCode, commandInfo.Text);
        }

        public override string Name { get; } = OpCode.Close.ToString();
    }
}
