using System;
using System.Collections.Generic;
using System.Linq;

namespace WebSocket4Net.Command
{
    public class BadRequest : WebSocketCommandBase
    {
        public static readonly BadRequest Instance = new BadRequest();

        private const string m_WebSocketVersion = "Sec-WebSocket-Version";
        private static readonly string[] m_ValueSeparator = { ", " };

        public override void ExecuteCommand(WebSocket session, WebSocketCommandInfo commandInfo)
        {
            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            commandInfo.Text.ParseMimeHeader(dict, out _);

            var websocketVersion = dict.GetValue(m_WebSocketVersion, string.Empty);

            if (string.IsNullOrEmpty(websocketVersion))
            {
                session.FireError(new Exception("unknown server protocol version"));
                session.CloseWithoutHandshake();
                return;
            }

            var versions = websocketVersion.Split(m_ValueSeparator, StringSplitOptions.RemoveEmptyEntries);

            var versionValues = new int[versions.Length];

            for (var i = 0; i < versions.Length; i++)
            {
                if (!int.TryParse(versions[i], out var value))
                {
                    session.FireError(new Exception("invalid websocket version"));
                    session.CloseWithoutHandshake();
                    return;
                }

                versionValues[i] = value;
            }

            if (!versionValues.Contains(13))
            {
                session.FireError(new Exception("unknown server protocol version"));
                session.CloseWithoutHandshake();
                return;
            }

            session.ProtocolProcessor.SendHandshake(session);
        }

        public override string Name { get; } = OpCode.BadRequest.ToString();
    }
}
