namespace WebSocket4Net.Command
{
    public class Binary : WebSocketCommandBase
    {
        public static readonly Binary Instance = new Binary();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.FireDataReceived(commandInfo.Data);
        }

        public override string Name { get; } = OpCode.Binary.ToString();
    }
}
