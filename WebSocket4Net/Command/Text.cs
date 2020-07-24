namespace WebSocket4Net.Command
{
    public class Text : WebSocketCommandBase
    {
        public static readonly Text Instance = new Text();

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            session.FireMessageReceived(commandInfo.Text);
        }

        public override string Name { get; } = OpCode.Text.ToString();
    }
}
