namespace Hotel_booking.Core.ModelsDTO
{
    public class Pagination<T>
    {
        public int PageIndex { get; }
        public int PageSize { get; }
        public int Count { get; }
        public IReadOnlyList<T> Data { get; }
        public int PageCount => Count == 0 ? 0 : (int)Math.Ceiling(Count / (double)PageSize);
        public bool HasPrevious => PageIndex > 1;
        public bool HasNext => PageIndex < PageCount;

        public Pagination(int pageIndex, int pageSize, int count, IReadOnlyList<T> data)
        {
            if (pageIndex <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex), "Page index must be greater than zero.");
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative.");

            ArgumentNullException.ThrowIfNull(data);

            PageIndex = pageIndex;
            PageSize = pageSize;
            Count = count;
            Data = data;
        }
    }
}
