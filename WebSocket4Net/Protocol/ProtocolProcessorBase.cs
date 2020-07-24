using System;
using System.Collections.Generic;

namespace WebSocket4Net.Protocol
{
    abstract class ProtocolProcessorBase
    {
        public abstract void SendHandshake(WebSocket websocket);

        public abstract ReaderBase CreateHandshakeReader(WebSocket websocket);

        public abstract bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description);

        public abstract void SendMessage(WebSocket websocket, string message);

        public abstract void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason);

        public abstract void SendPing(WebSocket websocket, string ping);

        public abstract void SendPong(WebSocket websocket, string pong);

        public abstract void SendData(WebSocket websocket, byte[] data, int offset, int length);

        public abstract void SendData(WebSocket websocket, IList<ArraySegment<byte>> segments);

        protected string VersionTag { get; } = "13";

        private static char[] s_SpaceSpliter = { ' ' };

        protected virtual bool ValidateVerbLine(string verbLine)
        {
            var parts = verbLine.Split(s_SpaceSpliter, 3, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return false;

            if (!parts[0].StartsWith("HTTP/"))
                return false;

            if (!int.TryParse(parts[1], out var statusCode))
                return false;

            return statusCode == 101;
        }
    }
}
