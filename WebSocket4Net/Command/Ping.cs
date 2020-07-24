using System;

namespace WebSocket4Net.Command
{
    public class Ping : WebSocketCommandBase
    {
        public static readonly Ping Instance = new Ping();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.LastActiveTime = DateTime.Now;
            session.ProtocolProcessor.SendPong(session, commandInfo.Text);
        }

        public override string Name { get; } = OpCode.Ping.ToString();
    }
}
