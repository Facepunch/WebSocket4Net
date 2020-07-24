namespace WebSocket4Net.Protocol.FramePartReader
{
    abstract class DataFramePartReader : IDataFramePartReader
    {
        public abstract int Process(int lastLength, WebSocketDataFrame frame, out IDataFramePartReader nextPartReader);

        public static IDataFramePartReader NewReader => FixPartReader;

        protected static IDataFramePartReader FixPartReader { get; } = new FixPartReader();

        protected static IDataFramePartReader ExtendedLengthReader { get; } = new ExtendedLengthReader();

        protected static IDataFramePartReader MaskKeyReader { get; } = new MaskKeyReader();

        protected static IDataFramePartReader PayloadDataReader { get; } = new PayloadDataReader();
    }
}
