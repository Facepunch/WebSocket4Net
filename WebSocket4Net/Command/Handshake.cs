using System;
using WebSocket4Net.Protocol;

namespace WebSocket4Net.Command
{
    public class Handshake : WebSocketCommandBase
    {
        public static readonly Handshake Instance = new Handshake();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            if (!session.ProtocolProcessor.VerifyHandshake(session, commandInfo, out var description))
            {
                session.FireError(new Exception(description));
                session.Close((int)CloseStatusCode.ProtocolError, description);
                return;
            }

            session.OnHandshaked();
        }

        public override string Name { get; } = OpCode.Handshake.ToString();
    }
}
