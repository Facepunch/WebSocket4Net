﻿namespace WebSocket4Net.Protocol.FramePartReader
{
    class FixPartReader : DataFramePartReader
    {
        public override int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader)
        {
            if (frame.Length < 2)
            {
                nextPartReader = this;
                return -1;
            }

            if (frame.PayloadLength < 126)
            {
                if (frame.HasMask)
                    nextPartReader = MaskKeyReader;
                else
                {
                    if (frame.ActualPayloadLength == 0)
                    {
                        nextPartReader = null;
                        return (int)((long)frame.Length - 2);
                    }

                    nextPartReader = PayloadDataReader;
                }
            }
            else
            {
                nextPartReader = ExtendedLengthReader;
            }

            if (frame.Length > 2)
                return nextPartReader.Process(2, frame, out nextPartReader);

            return 0;
        }
    }
}
