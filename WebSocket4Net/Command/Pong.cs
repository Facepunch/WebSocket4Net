using System;

namespace WebSocket4Net.Command
{
    public class Pong : WebSocketCommandBase
    {
        public static readonly Pong Instance = new Pong();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.LastActiveTime = DateTime.Now;
            session.LastPongResponse = commandInfo.Text;
        }

        public override string Name { get; } = OpCode.Pong.ToString();
    }
}
