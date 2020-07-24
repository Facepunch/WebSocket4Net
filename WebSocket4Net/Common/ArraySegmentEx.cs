namespace WebSocket4Net.Common
{
    internal readonly struct ArraySegmentEx<T>
    {
        /// <summary>
        /// Gets the array.
        /// </summary>
        public T[] Array { get; }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        public int Offset { get; }

        public int From { get; }

        public int To { get; }

        public ArraySegmentEx(T[] array, int offset, int count, int from, int to)
        {
            Array = array;
            Offset = offset;
            Count = count;
            From = from;
            To = to;
        }
    }
}
