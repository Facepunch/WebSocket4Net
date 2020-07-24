using System;
using System.Collections.Generic;
using System.Text;

namespace WebSocket4Net.Protocol
{
    /// <summary>
    /// http://tools.ietf.org/html/draft-ietf-hybi-thewebsocketprotocol-10
    /// </summary>
    class Rfc6455Processor : ProtocolProcessorBase
    {
        private const string m_Magic = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        private string m_ExpectedAcceptKey = "ExpectedAccept";

        private static Random m_Random = new Random();

        public override void SendHandshake(WebSocket websocket)
        {
            var secKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(Guid.NewGuid().ToString().Substring(0, 16)));
            var expectedAccept = (secKey + m_Magic).CalculateChallenge();

            websocket.Items[m_ExpectedAcceptKey] = expectedAccept;

            var handshakeBuilder = new StringBuilder();

            handshakeBuilder.AppendFormatWithCrLf("GET {0} HTTP/1.1", websocket.TargetUri.PathAndQuery);

            handshakeBuilder.Append("Host: ");
            handshakeBuilder.AppendWithCrLf(websocket.HandshakeHost);
            handshakeBuilder.AppendWithCrLf("Upgrade: websocket");
            handshakeBuilder.AppendWithCrLf("Connection: Upgrade");
            handshakeBuilder.Append("Sec-WebSocket-Version: ");
            handshakeBuilder.AppendWithCrLf(VersionTag);
            handshakeBuilder.Append("Sec-WebSocket-Key: ");
            handshakeBuilder.AppendWithCrLf(secKey);            
            handshakeBuilder.Append("Origin: ");
            handshakeBuilder.AppendWithCrLf(websocket.Origin);

            if (!string.IsNullOrEmpty(websocket.SubProtocol))
            {
                handshakeBuilder.Append("Sec-WebSocket-Protocol: ");
                handshakeBuilder.AppendWithCrLf(websocket.SubProtocol);
            }

            handshakeBuilder.AppendWithCrLf();

            byte[] handshakeBuffer = Encoding.UTF8.GetBytes(handshakeBuilder.ToString());

            websocket.Client.Send(handshakeBuffer, 0, handshakeBuffer.Length);
        }

        public override ReaderBase CreateHandshakeReader(WebSocket websocket)
        {
            return new Rfc6455HandshakeReader(websocket);
        }

        private void SendMessage(WebSocket websocket, int opCode, string message)
        {
            byte[] playloadData = Encoding.UTF8.GetBytes(message);
            SendDataFragment(websocket, opCode, playloadData, 0, playloadData.Length);
        }

        private byte[] EncodeDataFrame(int opCode, byte[] playloadData, int offset, int length)
        {
            return EncodeDataFrame(opCode, true, playloadData, offset, length);
        }

        private byte[] EncodeDataFrame(int opCode, bool isFinal, byte[] playloadData, int offset, int length)
        {
            byte[] fragment;

            int maskLength = 4;

            if (length < 126)
            {
                fragment = new byte[2 + maskLength + length];
                fragment[1] = (byte)length;
            }
            else if (length < 65536)
            {
                fragment = new byte[4 + maskLength + length];
                fragment[1] = (byte)126;
                fragment[2] = (byte)(length / 256);
                fragment[3] = (byte)(length % 256);
            }
            else
            {
                fragment = new byte[10 + maskLength + length];
                fragment[1] = (byte)127;

                int left = length;
                int unit = 256;

                for (int i = 9; i > 1; i--)
                {
                    fragment[i] = (byte)(left % unit);
                    left = left / unit;

                    if (left == 0)
                        break;
                }
            }

            
            if(isFinal)//Set FIN
                fragment[0] = (byte)(opCode | 0x80);
            else
                fragment[0] = (byte)opCode;

            //Set mask bit
            fragment[1] = (byte)(fragment[1] | 0x80);

            GenerateMask(fragment, fragment.Length - maskLength - length);

            if (length > 0)
                MaskData(playloadData, offset, length, fragment, fragment.Length - length, fragment, fragment.Length - maskLength - length);

            return fragment;
        }

        private void SendDataFragment(WebSocket websocket, int opCode, byte[] playloadData, int offset, int length)
        {
            byte[] fragment = EncodeDataFrame(opCode, playloadData, offset, length);

            var client = websocket.Client;

            if (client != null)
                client.Send(fragment, 0, fragment.Length);
        }

        public override void SendData(WebSocket websocket, byte[] data, int offset, int length)
        {
            SendDataFragment(websocket, OpCode.Binary, data, offset, length);
        }

        public override void SendData(WebSocket websocket, IList<ArraySegment<byte>> segments)
        {
            var fragments = new List<ArraySegment<byte>>(segments.Count);

            var lastPieceIndex = segments.Count - 1;

            for (var i = 0; i < segments.Count; i++)
            {
                var playloadData = segments[i];
                fragments.Add(new ArraySegment<byte>(EncodeDataFrame(i == 0 ? OpCode.Binary : 0, i == lastPieceIndex, playloadData.Array, playloadData.Offset, playloadData.Count)));
            }

            var client = websocket.Client;

            if (client != null)
                client.Send(fragments);
        }

        public override void SendMessage(WebSocket websocket, string message)
        {
            SendMessage(websocket, OpCode.Text, message);
        }

        public override void SendCloseHandshake(WebSocket websocket, int statusCode, string closeReason)
        {
            int size = (string.IsNullOrEmpty(closeReason) ? 0 : Encoding.UTF8.GetMaxByteCount(closeReason.Length)) + 2;

            byte[] playloadData = new byte[size];

            int highByte = statusCode / 256;
            int lowByte = statusCode % 256;

            playloadData[0] = (byte)highByte;
            playloadData[1] = (byte)lowByte;

            // don't send close handshake now because the connection was closed already
            if (websocket == null ||websocket.State == WebSocketState.Closed)
                return;

            if (!string.IsNullOrEmpty(closeReason))
            {
                int bytesCount = Encoding.UTF8.GetBytes(closeReason, 0, closeReason.Length, playloadData, 2);
                SendDataFragment(websocket, OpCode.Close, playloadData, 0, bytesCount + 2);
            }
            else
            {
                SendDataFragment(websocket, OpCode.Close, playloadData, 0, playloadData.Length);
            }
        }

        public override void SendPing(WebSocket websocket, string ping)
        {
            SendMessage(websocket, OpCode.Ping, ping);
        }

        public override void SendPong(WebSocket websocket, string pong)
        {
            SendMessage(websocket, OpCode.Pong, pong);
        }

        private const string m_Error_InvalidHandshake = "invalid handshake";
        private const string m_Error_SubProtocolNotMatch = "subprotocol doesn't match";
        private const string m_Error_AcceptKeyNotMatch = "accept key doesn't match";

        public override bool VerifyHandshake(WebSocket websocket, WebSocketCommandInfo handshakeInfo, out string description)
        {
            var handshake = handshakeInfo.Text;

            if (string.IsNullOrEmpty(handshake))
            {
                description = m_Error_InvalidHandshake;
                return false;
            }

            if (!handshakeInfo.Text.ParseMimeHeader(websocket.Items, out var verbLine))
            {
                description = m_Error_InvalidHandshake;
                return false;
            }

            if (!ValidateVerbLine(verbLine))
            {
                description = verbLine;
                return false;
            }

            if (!string.IsNullOrEmpty(websocket.SubProtocol))
            {
                var protocol = websocket.Items.GetValue("Sec-WebSocket-Protocol", string.Empty);

                if (!websocket.SubProtocol.Equals(protocol, StringComparison.OrdinalIgnoreCase))
                {
                    description = m_Error_SubProtocolNotMatch;
                    return false;
                }
            }

            var acceptKey = websocket.Items.GetValue("Sec-WebSocket-Accept", string.Empty);
            var expectedAcceptKey = websocket.Items.GetValue(m_ExpectedAcceptKey, string.Empty);

            if (!expectedAcceptKey.Equals(acceptKey, StringComparison.OrdinalIgnoreCase))
            {
                description = m_Error_AcceptKeyNotMatch;
                return false;
            }

            //more validations
            description = string.Empty;
            return true;
        }

        private void GenerateMask(byte[] mask, int offset)
        {
            int maxPos = Math.Min(offset + 4, mask.Length);

            for (var i = offset; i < maxPos; i++)
            {
                mask[i] = (byte)m_Random.Next(0, 255);
            }
        }

        private void MaskData(byte[] rawData, int offset, int length, byte[] outputData, int outputOffset, byte[] mask, int maskOffset)
        {
            for (var i = 0; i < length; i++)
            {
                var pos = offset + i;
                outputData[outputOffset++] = (byte)(rawData[pos] ^ mask[maskOffset + i % 4]);
            }
        }
    }
}
